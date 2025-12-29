using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq; 
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

[System.Serializable]
public struct PlantType
{
    public string plantName;
    public Sprite dirt;
    public Sprite seed;
    public Sprite sprout;
    public Sprite mature;
}

public class TikTokGameManager : MonoBehaviour
{
    [Header("Setup")]
    public string serverFolderPath = @"C:\Users\munic\OneDrive\Desktop\ZaroHavest\TiktokBridge";

    [Header("Farm System")]
    public FarmPlot[] farmPlots;
    public PlantType[] availablePlants; 
    public Sprite defaultDirt; 

    private HashSet<string> activePlayers = new HashSet<string>();
    
    private bool isGameEnded = false; 

    [Header("Leaderboard System")]
    public TextMeshProUGUI leaderboardText; 

    [Header("UI System")]
    public GameObject winPanel;       
    public TextMeshProUGUI winInfoText; 
    public UnityEngine.UI.Image winPlantImage; 

    [Header("Visual Effects & Rain RNG")]
    public GameObject blueRainPrefab;  
    public GameObject goldRainPrefab;  
    [Range(0, 100)] public int goldRainChance = 10; 
    
    public float spawnHeight = 8.0f;
    public AudioClip plantSound;   
    public AudioClip waterSound;   
    public AudioClip goldSound; 
    public AudioClip winSound; 
    private AudioSource audioSource;

    [Header("Server Info")]
    private ClientWebSocket ws = new ClientWebSocket();
    private Uri serverUri = new Uri("ws://127.0.0.1:8080");
    private Queue<string> messageQueue = new Queue<string>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        farmPlots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
        
        foreach(var plot in farmPlots) plot.SetDirtSprite(defaultDirt);

        UpdateLeaderboardUI(); 
        StartNodeServer();
        Invoke("ConnectToNode", 2f);
        
        if(winPanel != null) winPanel.SetActive(false);
        
        isGameEnded = false; 
    }

    void Update()
    {
        lock (messageQueue) {
            while (messageQueue.Count > 0) ProcessMessage(messageQueue.Dequeue());
        }

        if (Input.GetKeyDown(KeyCode.T)) TryPlantSeed("Tester");
        if (Input.GetKeyDown(KeyCode.R)) WaterRandomPlant();
        if (Input.GetKeyDown(KeyCode.Backspace)) ResetFarm();
        
        if (Input.GetKeyDown(KeyCode.L)) 
        { 
            PlayerPrefs.DeleteAll(); 
            UpdateLeaderboardUI(); 
            UnityEngine.Debug.Log("🗑️ ล้างข้อมูล Leaderboard เรียบร้อย!");
        }

        // 🔴 ปุ่มลับใหม่ F1: กดเพื่อลบอันดับ 1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            RemoveTopLeaderboardEntry();
        }
    }

    // 🔴 ฟังก์ชันใหม่: ลบข้อมูลอันแรกสุด
    void RemoveTopLeaderboardEntry()
    {
        string currentData = PlayerPrefs.GetString("Leaderboard", "");
        if (string.IsNullOrEmpty(currentData)) return;

        List<string> entries = currentData.Split('|').ToList();
        if (entries.Count > 0)
        {
            UnityEngine.Debug.Log($"กำลังลบ: {entries[0]}");
            entries.RemoveAt(0); // ลบตัวแรก
            string newData = string.Join("|", entries.ToArray());
            PlayerPrefs.SetString("Leaderboard", newData);
            PlayerPrefs.Save();
            UpdateLeaderboardUI(); // อัปเดตหน้าจอทันที
        }
    }

    void TryPlantSeed(string playerName)
    {
        if (isGameEnded) return;
        if (activePlayers.Contains(playerName)) return;

        List<FarmPlot> emptyPlots = new List<FarmPlot>();
        foreach (var plot in farmPlots)
        {
            if (!plot.isPlanted)
            {
                emptyPlots.Add(plot);
            }
        }

        if (emptyPlots.Count == 0) return;

        FarmPlot randomPlot = emptyPlots[UnityEngine.Random.Range(0, emptyPlots.Count)];

        if (availablePlants.Length == 0) return;
        PlantType randomPlant = availablePlants[UnityEngine.Random.Range(0, availablePlants.Length)];
        randomPlant.dirt = defaultDirt; 

        if (randomPlot.PlantSeed(playerName, randomPlant))
        {
            activePlayers.Add(playerName);
            PlaySound(plantSound);
        }
    }

    void WaterRandomPlant()
    {
        if (isGameEnded) return;

        List<FarmPlot> activePlots = new List<FarmPlot>();
        foreach (var plot in farmPlots) 
        {
            if (plot.isPlanted && plot.growthStage < 3) activePlots.Add(plot);
        }

        if (activePlots.Count > 0) 
        {
            int randIndex = UnityEngine.Random.Range(0, activePlots.Count);
            FarmPlot targetPlot = activePlots[randIndex];

            bool isGold = UnityEngine.Random.Range(0, 100) < goldRainChance;
            int waterAmount = isGold ? 5 : 1; 
            GameObject rainToSpawn = isGold ? goldRainPrefab : blueRainPrefab;
            AudioClip soundToPlay = isGold ? goldSound : waterSound;

            bool isFullyGrown = targetPlot.Water(waterAmount);
            
            if (rainToSpawn != null) 
            {
                Vector3 rainPos = targetPlot.transform.position;
                rainPos.y = spawnHeight + UnityEngine.Random.Range(0f, 1f); 
                rainPos.x += UnityEngine.Random.Range(-0.2f, 0.2f); 
                
                GameObject rainObj = Instantiate(rainToSpawn, rainPos, Quaternion.identity);
                
                string msg = isGold ? "+5 Like!" : "1 Like";
                var text3D = rainObj.GetComponentInChildren<TextMeshPro>();
                if (text3D != null) text3D.text = msg;
                else {
                    var textUI = rainObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (textUI != null) textUI.text = msg;
                }
            }
            
            PlaySound(soundToPlay);

            if (isFullyGrown)
            {
                float timeTaken = Time.time - targetPlot.startTime;
                Sprite winningSprite = targetPlot.GetComponent<SpriteRenderer>().sprite; 
                HandleWinner(targetPlot.nameText.text, timeTaken, winningSprite);
            }
        }
    }

    void HandleWinner(string winnerName, float timeTaken, Sprite plantSprite)
    {
        if (isGameEnded) return; 

        isGameEnded = true;

        PlaySound(winSound);
        SaveScore(winnerName, timeTaken); 
        UpdateLeaderboardUI(); 

        TimeSpan t = TimeSpan.FromSeconds(timeTaken);
        string timeStr = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

        if (winPanel != null)
        {
            if (winInfoText != null) 
                winInfoText.text = $"{winnerName}\nเวลา: {timeStr}";
            
            if (winPlantImage != null)
            {
                winPlantImage.sprite = plantSprite;
                winPlantImage.preserveAspect = true; 
            }

            winPanel.SetActive(true);
            StartCoroutine(HideWinAndReset(8f)); 
        }
    }

    IEnumerator HideWinAndReset(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (winPanel != null) winPanel.SetActive(false);
        ResetFarm(); 
    }

    void SaveScore(string name, float time)
    {
        string currentData = PlayerPrefs.GetString("Leaderboard", "");
        List<ScoreData> scores = new List<ScoreData>();
        
        if (!string.IsNullOrEmpty(currentData))
        {
            string[] entries = currentData.Split('|');
            foreach (var entry in entries)
            {
                string[] parts = entry.Split(',');
                if (parts.Length == 2) scores.Add(new ScoreData { name = parts[0], time = float.Parse(parts[1]) });
            }
        }

        scores.Add(new ScoreData { name = name, time = time });
        scores = scores.OrderBy(s => s.time).Take(5).ToList(); 

        string newData = "";
        foreach (var s in scores) newData += $"{s.name},{s.time}|";
        if (newData.Length > 0) newData = newData.Substring(0, newData.Length - 1);
        PlayerPrefs.SetString("Leaderboard", newData);
    }

    void UpdateLeaderboardUI()
    {
        if (leaderboardText == null) return;
        
        string currentData = PlayerPrefs.GetString("Leaderboard", "");
        string displayText = "TOP 5 FASTEST\n";
        
        if (!string.IsNullOrEmpty(currentData))
        {
            string[] entries = currentData.Split('|');
            int rank = 1;
            foreach (var entry in entries)
            {
                string[] parts = entry.Split(',');
                TimeSpan t = TimeSpan.FromSeconds(float.Parse(parts[1]));
                string tStr = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
                displayText += $"{rank}. {parts[0]} - {tStr}\n";
                rank++;
            }
        }
        else { displayText += "No Record Yet"; }
        
        leaderboardText.text = displayText;
    }

    class ScoreData { public string name; public float time; }

    void ResetFarm()
    {
        foreach (var plot in farmPlots) plot.ResetPlot();
        activePlayers.Clear();
        
        var drops = GameObject.FindGameObjectsWithTag("RainDrop"); 
        foreach (var drop in drops) Destroy(drop);
        
        foreach(var plot in farmPlots) plot.SetDirtSprite(defaultDirt);
        
        isGameEnded = false; 
    }
    
     void PlaySound(AudioClip clip) { if (clip != null && audioSource != null) audioSource.PlayOneShot(clip); }
     void ProcessMessage(string json) { try { TikTokEvent data = JsonUtility.FromJson<TikTokEvent>(json); if (data.type == "gift" && !string.IsNullOrEmpty(data.msg) && data.msg.ToLower().Contains("rose")) TryPlantSeed(data.name); else if (data.type == "like") WaterRandomPlant(); } catch { } }
     void StartNodeServer() { try { ProcessStartInfo s = new ProcessStartInfo("cmd.exe", "/k node server.js"); s.WorkingDirectory = serverFolderPath; Process.Start(s); } catch {} }
    async void ConnectToNode() { try { if (ws.State == WebSocketState.Open) return; await ws.ConnectAsync(serverUri, CancellationToken.None); ReceiveMessages(); } catch {} }
    async void ReceiveMessages() { var buffer = new byte[1024]; while (ws.State == WebSocketState.Open) { try { var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None); string message = Encoding.UTF8.GetString(buffer, 0, result.Count); lock (messageQueue) messageQueue.Enqueue(message); } catch { break; } } }
    private async void OnDestroy() { if (ws != null && ws.State == WebSocketState.Open) await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None); }
}

[System.Serializable]
public class TikTokEvent { public string type; public string name; public string msg; public int count; }