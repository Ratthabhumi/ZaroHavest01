using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject stepPrefab; // ลาก Prefab บันไดมาใส่
    public int totalSteps = 1000;
    
    [Header("Path Adjustments")]
    public float verticalOffset = 1.5f; // ความสูงระหว่างขั้น
    public float horizontalOffset = 2.0f; // ความห่างแนวนอน (ไปทางขวา)
    public float randomJitter = 0.5f; // ความไม่สม่ำเสมอ (ให้ดูธรรมชาติ)

    // ลิสต์เก็บตำแหน่งของบันไดทุกขั้น เอาไว้ให้ GameManager ใช้
    [HideInInspector] 
    public List<Vector3> stepPositions = new List<Vector3>();

    void Awake() // ใช้ Awake เพื่อให้ทำงานก่อน GameManager
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        Vector3 currentPos = transform.position; // เริ่มที่ตำแหน่งของตัวนี้

        for (int i = 0; i < totalSteps; i++)
        {
            // คำนวณตำแหน่งขั้นต่อไป (เฉียงขึ้นขวา + สุ่มนิดหน่อย)
            float xJitter = Random.Range(-randomJitter, randomJitter);
            Vector3 nextPos = currentPos + new Vector3(horizontalOffset + xJitter, verticalOffset, 0);

            // สร้างบันได
            GameObject newStep = Instantiate(stepPrefab, nextPos, Quaternion.identity, this.transform);
            newStep.name = "Step_" + i;

            // เก็บตำแหน่งไว้ในลิสต์
            stepPositions.Add(nextPos);

            // ขยับจุดอ้างอิงไปที่ขั้นใหม่
            currentPos = nextPos;
        }
    }
}