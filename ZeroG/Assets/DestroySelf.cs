using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    [Header("Settings")]
    public float delay = 3.0f; // เวลาหน่วยเป็นวินาที (ปรับใน Inspector ได้)

    void Start()
    {
        // สั่งให้ระเบิดตัวเองทิ้ง เมื่อเวลาผ่านไปตามค่า delay
        Destroy(gameObject, delay);
    }
}