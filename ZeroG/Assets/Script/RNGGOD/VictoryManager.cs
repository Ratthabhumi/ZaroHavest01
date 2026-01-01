using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager instance;

    [Header("UI References")]
    public GameObject victoryPanel;
    public Image winnerAvatar;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI winInfoText;

    [Header("Winner Display Settings")]
    public float displayDuration = 5.0f; // ลดเหลือ 5 วิจะได้ไม่รอนาน
    public float delayBetweenWinners = 1.0f;
    
    [Header("Grid Layout Settings")]
    public Vector3 firstPosition = new Vector3(-4f, 3f, 0f);
    public float xSpacing = 2.0f;
    public float ySpacing = -2.0f;
    public int maxColumns = 5;

    private int winnerCount = 0;

    private class WinnerData
    {
        public string username;
        public string avatarUrl;
        public string info;
        public PlayerController playerScript;
    }

    private Queue<WinnerData> winnerQueue = new Queue<WinnerData>();
    private bool isShowing = false;

    void Awake()
    {
        if (instance == null) instance = this; else Destroy(gameObject);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public void RegisterWinner(string username, string avatarUrl, string info, PlayerController pc)
    {
        WinnerData newData = new WinnerData {
            username = username,
            avatarUrl = avatarUrl,
            info = info,
            playerScript = pc
        };
        winnerQueue.Enqueue(newData);
        
        if (!isShowing) 
        {
            ProcessNextWinner();
        }
    }

    void ProcessNextWinner()
    {
        if (isShowing || winnerQueue.Count == 0) return;
        StartCoroutine(WinRoutine());
    }

    IEnumerator WinRoutine()
    {
        isShowing = true;
        WinnerData data = winnerQueue.Dequeue();

        // 🏆 1. ส่งเข้า Leaderboard
        if (LeaderboardManager.instance != null)
            LeaderboardManager.instance.AddWinner(data.username);

        // 📏 2. จัดระเบียบตำแหน่ง
        if (data.playerScript != null)
        {
            int column = winnerCount % maxColumns;
            int row = winnerCount / maxColumns;
            Vector3 gridPos = firstPosition + new Vector3(column * xSpacing, row * ySpacing, 0);
            data.playerScript.transform.position = gridPos;
            winnerCount++;

            if (data.playerScript.nameText != null)
            {
                data.playerScript.nameText.text = $"<color=green><b>WINNER!</b></color>\n{data.username}";
            }
        }

        // 🔊 3. เสียงชนะ
        if (AudioManager.instance != null) AudioManager.instance.PlayWin();

        // 🖼️ 4. โหลดรูป (เพิ่มระบบกันค้าง)
        if (winnerAvatar != null) winnerAvatar.sprite = null;
        if (!string.IsNullOrEmpty(data.avatarUrl) && winnerAvatar != null)
        {
            yield return StartCoroutine(DownloadAvatar(data.avatarUrl));
        }

        // ⏳ 5. โชว์ค้างไว้ตามเวลา
        yield return new WaitForSeconds(displayDuration);

        if (victoryPanel != null) victoryPanel.SetActive(false);

        // 🏁 6. ปลดล็อคคิวแน่นอน
        yield return new WaitForSeconds(delayBetweenWinners);
        isShowing = false; 
        
        // เรียกคนต่อไป
        ProcessNextWinner();
    }

    IEnumerator DownloadAvatar(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            uwr.timeout = 3; // ถ้า 3 วิโหลดไม่ได้ให้ข้ามเลย จะได้ไม่ค้าง
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                if (texture != null && winnerAvatar != null)
                {
                    winnerAvatar.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }

    public void ResetWinnerGrid() { winnerCount = 0; }
}