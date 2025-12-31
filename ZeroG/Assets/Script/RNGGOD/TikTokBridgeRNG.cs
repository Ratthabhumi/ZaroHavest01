using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

[System.Serializable]
public class BridgeData
{
    public string eventName;
    public string username;
    public string giftName;
    public int giftId;
    public string avatarUrl;
}

public class TikTokBridgeRNG : MonoBehaviour
{
    [Header("Auto Run Settings")]
    public bool autoStartServer = true;
    public string serverFolderPath = @"C:\Users\munic\OneDrive\Desktop\ZaroHavest\TiktokBridge"; 
    
    private string serverWindowTitle = "TikTokBridge_Server_Unique"; 
    private Process currentServerProcess;

    [Header("Game References")]
    public DiceManager diceManager;

    private ClientWebSocket ws = new ClientWebSocket();
    private CancellationTokenSource cts = new CancellationTokenSource();
    private Queue<string> messageQueue = new Queue<string>();
    private string serverUrl = "ws://localhost:8080";

    void Start()
    {
        if (autoStartServer)
        {
            KillOldProcesses();
            StartNodeServer();
        }
        Invoke("ConnectAfterDelay", 2f);
    }

    void ConnectAfterDelay()
    {
        ConnectToNodeServer();
    }

    void KillOldProcesses()
    {
        try
        {
            ProcessStartInfo killInfo = new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/F /FI \"WINDOWTITLE eq {serverWindowTitle}*\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(killInfo)?.WaitForExit();
        }
        catch (Exception) {}
    }

    void StartNodeServer() 
    { 
        try 
        { 
            string command = $"/k \"title {serverWindowTitle} && node server.js\"";
            ProcessStartInfo s = new ProcessStartInfo("cmd.exe", command); 
            s.WorkingDirectory = serverFolderPath;
            s.UseShellExecute = true;
            currentServerProcess = Process.Start(s); 
        } 
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("❌ เปิด CMD ไม่สำเร็จ: " + e.Message);
        } 
    }

    async void ConnectToNodeServer()
    {
        try
        {
            if (ws.State == WebSocketState.Open) return;
            await ws.ConnectAsync(new Uri(serverUrl), cts.Token);
            UnityEngine.Debug.Log("✅ เชื่อมต่อกับ CMD สำเร็จแล้ว!");
            ReceiveLoop();
        }
        catch (Exception) {}
    }

    async void ReceiveLoop()
    {
        var buffer = new byte[1024 * 4];
        while (ws.State == WebSocketState.Open)
        {
            try
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;

                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                lock (messageQueue) { messageQueue.Enqueue(json); }
            }
            catch { break; }
        }
    }

    void Update()
    {
        lock (messageQueue)
        {
            while (messageQueue.Count > 0) ProcessData(messageQueue.Dequeue());
        }
    }

    void ProcessData(string json)
    {
        try
        {
            BridgeData data = JsonUtility.FromJson<BridgeData>(json);
            
            // ✅ เหลือเงื่อนไขเดียว: ต้องส่ง Rose หรือ ID 5655 เท่านั้น!
            if (data.eventName == "gift" && (data.giftName == "Rose" || data.giftId == 5655))
            {
                UnityEngine.Debug.Log($"🌹 {data.username} ส่งกุหลาบ!");
                DoRoll(data.username, data.avatarUrl);
            }
            // ❌ ส่วนที่เป็น test_roll ลบทิ้งไปแล้ว
        }
        catch (Exception) { }
    }

    void DoRoll(string username, string avatarUrl)
    {
        if (diceManager != null)
        {
            int rng = UnityEngine.Random.Range(1, 7);
            diceManager.RollTheDice(rng, username, avatarUrl);
        }
    }

    private void OnApplicationQuit()
    {
        CloseServer();
    }

    private void OnDestroy()
    {
        CloseServer();
        cts.Cancel();
        if (ws != null) ws.Dispose();
    }

    void CloseServer()
    {
        if (currentServerProcess != null && !currentServerProcess.HasExited)
        {
            try { currentServerProcess.Kill(); } catch {}
        }
        KillOldProcesses();
    }
}