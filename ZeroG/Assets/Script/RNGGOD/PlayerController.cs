using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("UI Refs")]
    public TextMeshPro nameText;

    [Header("Movement Settings")]
    public float moveForce = 2f; // ลดแรงขยับลงหน่อย จะได้ไม่ลื่นปรื๊ด
    // public float jumpForce = 5f; // ❌ ไม่ใช้แล้ว (ปิดไว้)
    
    private Rigidbody rb;
    private float moveTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetupCharacter(string username)
    {
        if (nameText != null)
        {
            nameText.text = username;
        }
    }

    void FixedUpdate()
    {
        // ยังให้มันขยับนิดๆ หน่อยๆ ได้ (จะได้ดูมีชีวิตชีวา ไม่แข็งทื่อ)
        // แต่ถ้าอยากให้แข็งเป็นหินเลย ให้ลบทั้งก้อนนี้ทิ้งครับ
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0)
        {
            RandomNudge(); // เปลี่ยนชื่อฟังก์ชันเป็น Nudge (ขยับเบาๆ)
            moveTimer = Random.Range(1.0f, 3.0f); // ขยับนานๆ ที
        }
    }

    void RandomNudge() // เปลี่ยนจาก RandomMove
    {
        if (rb == null) return;

        // สุ่มขยับซ้ายขวา "เบาๆ" พอให้รู้ว่ายังไม่ตาย
        float randomX = Random.Range(-0.5f, 0.5f);
        
        // ใส่แรงแค่แกน X (แนวนอน) ไม่ใส่แกน Y (แนวตั้ง) แล้ว
        rb.AddForce(new Vector3(randomX, 0, 0) * moveForce, ForceMode.Impulse);

        // ❌ ตัดส่วนกระโดด (Jump) ทิ้งไปเลย!
        // if (Random.value < 0.3f) { ... } << ลบเกลี้ยง
        
        // ❌ ตัดส่วนหมุนตัว (Torque) ทิ้งด้วย จะได้ไม่กลิ้ง
    }
}