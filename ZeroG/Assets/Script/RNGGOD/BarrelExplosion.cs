using UnityEngine;
using System.Collections;

public class BarrelExplosion : MonoBehaviour
{
    [Header("ตั้งค่าการระเบิด")]
    [Tooltip("ลาก Prefab เอฟเฟกต์ระเบิดมาใส่ตรงนี้ (ถ้ามี)")]
    public GameObject explosionEffectPrefab;
    [Tooltip("หน่วงเวลาก่อนที่ถังจะหายไป (วินาที)")]
    public float explosionDelay = 0.5f;

    void Start()
    {
        // พอถังเกิดมา ให้เริ่มกระบวนการระเบิดทันที
        StartCoroutine(ExplodeRoutine());
    }

    IEnumerator ExplodeRoutine()
    {
        Debug.Log("💥 ถังระเบิดทำงาน! กำลังล้างบาง...");

        // 🔊 3. ใส่เสียงระเบิดตูมตาม (Explosion Sound) ตรงนี้!
        if (AudioManager.instance != null) AudioManager.instance.PlayExplosion();

        // 1. แสดงเอฟเฟกต์ระเบิด
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. ค้นหาตัวละครทั้งหมดในฉาก (ใช้ FindObjectsByType แบบใหม่)
        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            // เช็คว่าเป็นตัวละครที่เราสร้างมา (ชื่อขึ้นต้น Char_ และเป็น Clone)
            if (obj.name.StartsWith("Char_") && obj.name.EndsWith("(Clone)"))
            {
                // 🛡️ ข้อยกเว้น: ถ้าเป็นบอส (Char_06) หรือเป็นถังใบนี้เอง -> ห้ามทำลาย
                if (obj.name.Contains("Char_06_Boss") || obj == this.gameObject)
                {
                    // Debug.Log($"🛡️ ยกเว้น {obj.name}");
                    continue; 
                }

                // นอกนั้นบึ้ม!
                // Debug.Log($"🔥 ทำลาย: {obj.name}");
                Destroy(obj);
            }
        }

        // 3. รอแป๊บนึง แล้วทำลายตัวเอง
        yield return new WaitForSeconds(explosionDelay);
        Destroy(gameObject);
    }
}