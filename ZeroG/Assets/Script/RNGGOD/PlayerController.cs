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
        
        // 🏆 แก้ไขบรรทัดนี้: ต้องส่ง 3 ค่า (ชื่อ, URL รูปโปรไฟล์, และข้อความ Info)
        if (VictoryManager.instance != null)
        {
            // เราส่งค่า "" (ว่างๆ) ไปในช่อง info เพราะเฮียไม่ต้องการโชว์เวลาใน Leaderboard แล้ว
            VictoryManager.instance.RegisterWinner(myName, myAvatarUrl, "", this);
        }

        // ตั้งค่าฟิสิกส์ให้หยุดนิ่งเหมือนเดิม
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