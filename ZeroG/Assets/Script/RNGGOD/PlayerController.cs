using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("UI Refs")]
    public TextMeshProUGUI nameText; // ป้ายชื่อบนหัว (ลากแบบ UI ใส่)

    [Header("Movement Settings")]
    public float moveForce = 2f;

    [Header("King of the Hill")]
    private float timeToWin = 180f; // รับค่ามาจาก DiceManager
    private float survivalTimer = 0f;
    private bool hasWon = false;
    
    // ข้อมูลผู้เล่น
    private string myName;
    private string myAvatarUrl;
    
    private Rigidbody rb; // ใช้ Rigidbody สำหรับ 3D
    private float moveTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); //
    }

    // ✅ รับค่าชื่อ, รูป, และเวลารอดชีวิตมาจาก DiceManager
    public void SetupCharacter(string username, string avatarUrl, float survivalTime)
    {
        myName = username;
        myAvatarUrl = avatarUrl;
        timeToWin = survivalTime; //

        UpdateNameTag(timeToWin);
    }

    void FixedUpdate()
    {
        // 1. ระบบ AI ขยับตัวสุ่ม
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0)
        {
            RandomNudge();
            moveTimer = Random.Range(1.0f, 3.0f);
        }

        // 2. ⏳ ระบบนับเวลาถอยหลังและเช็คการชนะ
        if (!hasWon)
        {
            survivalTimer += Time.deltaTime; //
            
            float timeLeft = Mathf.Max(0, timeToWin - survivalTimer); //
            UpdateNameTag(timeLeft);

            // เช็คตกแมพ (ตาย)
            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
                return;
            }

            // เช็คว่ารอดครบเวลาหรือยัง
            if (survivalTimer >= timeToWin)
            {
                WinGame();
            }
        }
    }

    // อัปเดตเวลาถอยหลังสีเหลืองบนหัว
    void UpdateNameTag(float timeRemaining)
    {
        if (nameText != null)
        {
            float m = Mathf.FloorToInt(timeRemaining / 60);
            float s = Mathf.FloorToInt(timeRemaining % 60);
            nameText.text = $"{myName}\n<size=80%><color=yellow>{m:0}:{s:00}</color></size>";
        }
    }

    void WinGame()
    {
        if (hasWon) return;
        hasWon = true;

        if (VictoryManager.instance != null)
        {
            // เริ่มต้นด้วยชื่อเต็มๆ ก่อน
            string displayName = myName;

            // 🔍 เช็คว่าถ้าเป็นตัวจาก LIKE POWER ให้แกะเอาแค่ชื่อคนออกมา
            if (myName.Contains("LIKE POWER"))
            {
                // สมมติชื่อมาเป็น: "LIKE POWER (Somchai)"
                // 1. ลบคำว่า "LIKE POWER (" ทิ้ง
                displayName = displayName.Replace("LIKE POWER (", "");
                
                // 2. ลบวงเล็บปิด ")" ทิ้ง
                displayName = displayName.Replace(")", "");
                
                // ผลลัพธ์: "Somchai" (ชื่อเพียวๆ สีปกติ)
            }

            // ส่งชื่อที่คลีนแล้ว ไปเข้า Leaderboard (สีจะขาวปกติเหมือนคนส่งของขวัญ)
            VictoryManager.instance.RegisterWinner(displayName, myAvatarUrl, "", this);
        }
        
        // ... (ส่วนฟิสิกส์และการทำลายตัวละคร เหมือนเดิมเป๊ะ) ...
        if (nameText != null) nameText.text = $"<color=green>WINNER!</color>\n{myName}"; // บนหัวยังโชว์ชื่อเต็มได้ หรือจะใช้ displayName ก็ได้ครับ
        
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        transform.rotation = Quaternion.identity;
        transform.localScale *= 0.8f;

        Destroy(gameObject, 15f);
    }

    void RandomNudge()
    {
        if (rb == null) return;
        float randomX = Random.Range(-0.5f, 0.5f);
        rb.AddForce(new Vector3(randomX, 0, 0) * moveForce, ForceMode.Impulse); //
    }
}