using UnityEngine;

public class SimpleBoom : MonoBehaviour
{
    public float expandSpeed = 10f; // ความเร็วในการขยายตัว
    public float lifetime = 0.5f;   // เวลาชีวิตก่อนหายไป

    void Start()
    {
        // ทำลายตัวเองเมื่อหมดเวลา
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // ขยายขนาดขึ้นเรื่อยๆ
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;
        // ทำให้ค่อยๆ จางลง (ถ้า Sprite มีสีขาว)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.color;
            color.a -= Time.deltaTime / lifetime;
            sr.color = color;
        }
    }
}