using UnityEngine;
using TMPro; // จำเป็นมากสำหรับ TextMeshPro

public class PlayerController : MonoBehaviour
{
    [Header("UI Refs")]
    // 🔴 แก้ตรงนี้! เปลี่ยนจาก TextMeshPro เป็น TextMeshProUGUI
    public TextMeshProUGUI nameText; 

    [Header("Movement Settings")]
    public float moveForce = 2f; 
    
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
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0)
        {
            RandomNudge();
            moveTimer = Random.Range(1.0f, 3.0f);
        }
    }

    void RandomNudge()
    {
        if (rb == null) return;
        float randomX = Random.Range(-0.5f, 0.5f);
        rb.AddForce(new Vector3(randomX, 0, 0) * moveForce, ForceMode.Impulse);
    }
}