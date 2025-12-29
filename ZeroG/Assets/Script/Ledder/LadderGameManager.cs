using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

public class LadderGameManager : MonoBehaviour
{
    [Header("Setup")]
    // 🔴 เช็ค Path นี้ให้ตรงกับเครื่องของคุณ
    public string serverFolderPath = @"C:\Users\munic\OneDrive\Desktop\ZaroHavest\TiktokBridge"; 
    
    // 🔴 ลากตัว LevelSpawner มาใส่ช่องนี้
    public LevelGenerator levelGen; 
    
    // 🔴 ลากแท่น StartBase มาใส่ช่องนี้
    public Transform startPoint; 

    [Header("Player Settings")]
    public Transform playerTransform; 
    public Transform cameraTransform; 
    public float moveSpeed = 5.0f; 

    [Header("Game Rules")]
    private int targetSteps;
    private int currentStep = -1; // เริ่มที่ -1 (ยืนรอที่ StartBase)
    private int likesBuffer = 0;
    public int likesToStep = 10; // 10 Like = 1 ก้าว

    [Header("UI System")]
    public TextMeshProUGUI stepText; 
    public GameObject winPanel;
    public TextMeshProUGUI winInfoText;

    [Header("Effects")]
    public ParticleSystem jumpEffectPrefab; // 🔴 ลาก Prefab ที่สร้างเมื่อกี้มาใส่

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip fallSound;
    public AudioClip winSound;
    private AudioSource audioSource;

    // Server Info
    private ClientWebSocket ws = new ClientWebSocket();
    private Uri serverUri = new Uri("ws://127.0.0.1:8080");
    private Queue<string> messageQueue = new Queue<string>();
    private bool isGameEnded = false;
    private Vector3 targetPosition;

    void Start()
    {
        // ดึงจำนวนชั้นทั้งหมดจาก Generator
        if (levelGen != null) targetSteps = levelGen.totalSteps;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        if (winPanel != null) winPanel.SetActive(false);
        
        StartNodeServer();
        Invoke("ConnectToNode", 2f);

        // --- ตั้งค่าเริ่มต้น ---
        currentStep = -1; // เริ่มที่พื้น
        likesBuffer = 0;
        UpdateTargetPosition(); // สั่งให้ย้ายตำแหน่งทันที
        
        // วาร์ปตัวละครไปที่จุดเริ่มต้นทันที (ไม่ต้องรอ Lerp)
        if (playerTransform != null) playerTransform.position = targetPosition;
        
        UpdateUI();
    }

    void Update()
    {
        // WebSocket Processing
        lock (messageQueue) {
            while (messageQueue.Count > 0) ProcessMessage(messageQueue.Dequeue());
        }

        // --- Logic การขยับตัว ---
        if (playerTransform != null)
        {
            // ขยับตัวแบบนุ่มนวล
            playerTransform.position = Vector3.Lerp(playerTransform.position, targetPosition, Time.deltaTime * moveSpeed);
            
            // กล้องตาม (มุมมองด้านข้าง)
            if (cameraTransform != null)
            {
                Vector3 camPos = cameraTransform.position;
                // ให้กล้องมองที่ตัวละคร + ถอยหลังออกมา (Z-15) + ยกสูงนิดหน่อย (Y+2)
                Vector3 desiredCamPos = new Vector3(playerTransform.position.x, playerTransform.position.y + 2, -15);
                
                cameraTransform.position = Vector3.Lerp(camPos, desiredCamPos, Time.deltaTime * 2f);
            }
        }

        // --- ปุ่มทดสอบ (Dev Only) ---
        if (Input.GetKeyDown(KeyCode.UpArrow)) MoveUp(1, "Tester");
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveDown(10); 
    }

    void MoveUp(int steps, string username)
    {
        if (isGameEnded) return;

        currentStep += steps;
        
        // ห้ามเกินบันไดขั้นสุดท้าย
        if (currentStep >= targetSteps) currentStep = targetSteps - 1;

        // เช็คเงื่อนไขชนะ (ถึงยอด)
        if (currentStep == targetSteps - 1) HandleWinner(username);

        PlaySound(jumpSound);

        // 🔴🔴 เล่นเอฟเฟกต์ตรงตำแหน่งตัวละคร 🔴🔴
        if (jumpEffectPrefab != null && playerTransform != null)
        {
            // สร้างเอฟเฟกต์ขึ้นมาที่ตำแหน่งตัวละคร
            ParticleSystem effect = Instantiate(jumpEffectPrefab, playerTransform.position, Quaternion.identity);
            effect.Play(); // สั่งเล่น
            Destroy(effect.gameObject, 2f); // ลบทิ้งเมื่อเล่นจบ (2 วินาที)
        }

        UpdateTargetPosition();
        UpdateUI();
    }

    void MoveDown(int steps)
    {
        if (isGameEnded) return;
        
        // ถ้าอยู่ที่พื้น (-1) แล้ว ไม่ต้องลดต่อ
        if (currentStep <= -1) return; 

        currentStep -= steps;
        
        // ห้ามต่ำกว่า -1 (พื้น StartBase)
        if (currentStep < -1) currentStep = -1; 

        PlaySound(fallSound);
        UpdateTargetPosition();
        UpdateUI();
    }

    void UpdateTargetPosition()
    {
        // 1. กรณีอยู่บนบันได (Step 0 ถึง 999)
        if (currentStep >= 0 && levelGen != null && levelGen.stepPositions.Count > currentStep)
        {
            // ตำแหน่งบันได + ยืนบนหัวนิดหน่อย
            targetPosition = levelGen.stepPositions[currentStep] + Vector3.up * 1.5f;
            
            // 🔴 Fix: ดึงออกมาข้างหน้าแกน Z เพื่อไม่ให้จมในบล็อก
            targetPosition.z = -2f; 
        }
        // 2. กรณีอยู่ที่พื้น StartBase (Step -1)
        else
        {
            if (startPoint != null)
            {
                // ตำแหน่งแท่น + ลอยสูงขึ้น 1.5 หน่วย (ปรับเลขตรงนี้ถ้าอยากให้ลอยสูงขึ้นอีก)
                targetPosition = startPoint.position + Vector3.up * 1.5f;
                
                // 🔴 Fix: ดึงออกมาข้างหน้าเหมือนกัน
                targetPosition.z = -2f; 
            }
        }
    }

    void HandleWinner(string winnerName)
    {
        if (isGameEnded) return;
        isGameEnded = true;
        PlaySound(winSound);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winInfoText != null)
                winInfoText.text = $"{winnerName}\nพิชิตยอดเขา!";
        }
        
        Invoke("ResetGame", 10f); // รีเซ็ตใน 10 วิ
    }

    void ResetGame()
    {
        isGameEnded = false;
        currentStep = -1; // กลับไปที่ StartBase
        likesBuffer = 0;
        UpdateTargetPosition();
        UpdateUI();
        if (winPanel != null) winPanel.SetActive(false);
    }

    void UpdateUI()
    {
        if (stepText != null)
        {
            // แสดงผล +2 เพราะเริ่มที่ -1 (StartBase = 0, Step 1 = 1) หรือจะแสดงตามใจชอบ
            // เอาแบบเข้าใจง่าย: Step -1 โชว์ว่า "Ready", Step 0 โชว์ "1"
            string stepShow = currentStep == -1 ? "Start" : (currentStep + 1).ToString();
            stepText.text = $"Step: {stepShow} / {targetSteps}\nLike Gauge: {likesBuffer}/{likesToStep}";
        }
    }

    void PlaySound(AudioClip clip) { if (clip != null && audioSource != null) audioSource.PlayOneShot(clip); }

    // ---------------- TikTok Logic ----------------
    void ProcessMessage(string json)
    {
        try
        {
            // ใช้ TikTokEvent จากไฟล์ TikTokSharedData.cs
            TikTokEvent data = JsonUtility.FromJson<TikTokEvent>(json);
            
            // 1. ระบบ Like (แก้เรื่องนับจำนวนแล้ว)
            if (data.type == "like")
            {
                // 🔴 Fix: อ่านจำนวนไลก์จริงๆ ที่ส่งมา (Batching)
                int likeAmount = data.count > 0 ? data.count : 1;
                likesBuffer += likeAmount;

                if (likesBuffer >= likesToStep)
                {
                    // คำนวณโควต้าก้าวเดิน (เผื่อคนกดมาเยอะมากทีเดียว)
                    int stepsToMove = likesBuffer / likesToStep;
                    likesBuffer = likesBuffer % likesToStep; // เก็บเศษที่เหลือ
                    
                    MoveUp(stepsToMove, "Like Power"); 
                }
                UpdateUI();
            }
            // 2. ระบบ Gift
            else if (data.type == "gift")
            {
                string giftName = data.msg.ToLower(); 
                
                // --- โซนช่วย (Help) ---
                if (giftName.Contains("rose")) MoveUp(5, data.name);
                else if (giftName.Contains("heart")) MoveUp(10, data.name);
                
                // --- โซนแกล้ง (Troll) ---
                else if (giftName.Contains("gg") || giftName.Contains("boxing") || giftName.Contains("weight"))
                {
                    MoveDown(20); 
                }
            }
        }
        catch { }
    }

    // ---------------- Server Connection ----------------
    void StartNodeServer() { try { ProcessStartInfo s = new ProcessStartInfo("cmd.exe", "/k node server.js"); s.WorkingDirectory = serverFolderPath; Process.Start(s); } catch { } }
    async void ConnectToNode() { try { if (ws.State == WebSocketState.Open) return; await ws.ConnectAsync(serverUri, CancellationToken.None); ReceiveMessages(); } catch { } }
    async void ReceiveMessages() { var buffer = new byte[1024]; while (ws.State == WebSocketState.Open) { try { var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None); string message = Encoding.UTF8.GetString(buffer, 0, result.Count); lock (messageQueue) messageQueue.Enqueue(message); } catch { break; } } }
    private async void OnDestroy() { if (ws != null && ws.State == WebSocketState.Open) await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None); }
}