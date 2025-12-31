using UnityEngine;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    [Header("--- Settings ---")]
    public float idleSpeed = 10f;       // ความเร็วตอนหมุนโชว์ (ใส่ 0 ถ้าอยากให้นิ่ง)
    public float spinSpeed = 1500f;     // ความเร็วตอนหมุนสุ่ม (แรงๆ สะใจ)
    public float spinDuration = 2.0f;   // ระยะเวลาหมุนกี่วินาที
    
    [Header("--- Spawn Settings ---")]
    public Transform spawnPoint;        // ลากจุด SpawnPoint มาใส่ตรงนี้
    public GameObject[] characterPrefabs; // ใส่ 6 ช่อง (Element 0 = เบอร์ 1, Element 5 = เบอร์ 6)

    private bool isRolling = false;
    private Quaternion targetRotation;

    void Update()
    {
        // 1. ถ้าว่างอยู่ (Idle) ให้หมุนโชว์เอื่อยๆ
        if (!isRolling && idleSpeed > 0)
        {
            transform.Rotate(new Vector3(0.5f, 1f, 0.2f) * idleSpeed * Time.deltaTime);
        }
    }

    // ฟังก์ชันรับคำสั่ง (เรียกจาก TestDice หรือ TikTok System)
    public void RollTheDice(int result, string username, string profilePicUrl)
    {
        if (isRolling) return; // ถ้ากำลังหมุนอยู่ ห้ามแทรก
        StartCoroutine(SpinRoutine(result, username));
    }

    IEnumerator SpinRoutine(int result, string username)
    {
        isRolling = true;
        float timer = 0f;

        // --- PHASE 1: หมุนติ้วๆ (Spinning Action) ---
        // สุ่มแกนหมุนใหม่ทุกครั้ง เพื่อความไม่ซ้ำซาก
        Vector3 randomAxis = Random.onUnitSphere; 

        while (timer < spinDuration)
        {
            timer += Time.deltaTime;
            // หมุนรอบแกนที่สุ่มได้ ด้วยความเร็วสูง
            transform.Rotate(randomAxis * spinSpeed * Time.deltaTime);
            yield return null;
        }

        // --- PHASE 2: กำหนดมุมจบ (Calibration Mapping) ---
        // แมพตามค่าที่คุณจดมาเป๊ะๆ
        switch (result)
        {
            case 1: targetRotation = Quaternion.Euler(0, 0, 0); break;     // เลข 1
            case 2: targetRotation = Quaternion.Euler(0, -90, 0); break;   // เลข 2
            case 3: targetRotation = Quaternion.Euler(90, 0, 0); break;    // เลข 3
            case 4: targetRotation = Quaternion.Euler(-90, 0, 0); break;   // เลข 4
            case 5: targetRotation = Quaternion.Euler(0, 90, 0); break;    // เลข 5
            case 6: targetRotation = Quaternion.Euler(180, 0, 0); break;   // เลข 6
            default: targetRotation = Quaternion.Euler(0, 0, 0); break;    // กันเหนียว
        }

        // --- PHASE 3: ดูดเข้าที่ (Snapping) ---
        float snapDuration = 0.5f; 
        float snapTimer = 0f;
        Quaternion startRot = transform.rotation;

        while (snapTimer < snapDuration)
        {
            snapTimer += Time.deltaTime;
            float t = snapTimer / snapDuration;
            
            // สูตร Ease Out Cubic (เพื่อให้ตอนจบมันดูดเข้าแบบนุ่มๆ ไม่แข็งทื่อ)
            t = 1f - Mathf.Pow(1f - t, 3f); 
            
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);
            yield return null;
        }
        
        // จบงาน: ล็อคตำแหน่ง
        transform.rotation = targetRotation;
        isRolling = false;
        
        Debug.Log($"Dice Result: {result} - Spawning {username}'s Character!");

        // --- PHASE 4: เสกตัวละคร (Spawn) ---
        SpawnCharacter(result, username);
    }

    void SpawnCharacter(int diceNumber, string playerName)
    {
        // แปลงเลขลูกเต๋า (1-6) เป็น Index ของอาเรย์ (0-5)
        int index = diceNumber - 1;

        // เช็คความปลอดภัย: ถ้าไม่ได้ใส่ Prefab ไว้ หรือเลขหลุด
        if (characterPrefabs == null || characterPrefabs.Length == 0) return;
        if (index < 0 || index >= characterPrefabs.Length) index = 0; // ถ้าหาไม่เจอให้ใช้ตัวแรกแทน

        // 1. เสกตัวละครออกมา
        GameObject newChar = Instantiate(characterPrefabs[index], spawnPoint.position, Quaternion.identity);
        
        // 2. ตั้งชื่อบนหัว (เชื่อมกับ Script PlayerController)
        PlayerController pc = newChar.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.SetupCharacter(playerName);
        }
    }
}