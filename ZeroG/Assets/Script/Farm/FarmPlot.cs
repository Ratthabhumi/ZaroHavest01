using UnityEngine;
using TMPro;

public class FarmPlot : MonoBehaviour
{
    [Header("Status")]
    public bool isPlanted = false;
    public int growthStage = 0;
    public float startTime;

    [Header("Growth Settings")]
    public int waterCount = 0;      
    public int waterRequired = 10;  

    [Header("Components")]
    public TextMeshPro nameText;
    private SpriteRenderer sr;
    
    // 🔴 ส่วนที่เพิ่ม: ตัวแปรเก็บข้อมูลว่าต้นนี้คือต้นอะไร
    private PlantType currentPlantType;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }
    void Start() { ResetPlot(); }

    // 🔴 ฟังก์ชันปลูกแบบใหม่ (รับข้อมูลพืชมาด้วย)
    public bool PlantSeed(string playerName, PlantType typeData)
    {
        if (!isPlanted)
        {
            isPlanted = true;
            growthStage = 1;
            waterCount = 0;
            startTime = Time.time;
            currentPlantType = typeData; // จำไว้ว่าฉันคือต้นอะไร
            
            if (nameText != null) {
                nameText.text = playerName;
                nameText.gameObject.SetActive(true);
            }
            UpdateSprite();
            return true;
        }
        return false;
    }

    // 🔴 ฟังก์ชันรดน้ำแบบใหม่ (รองรับฝนทองคำ)
    public bool Water(int amount = 1)
    {
        if (isPlanted && growthStage < 3)
        {
            waterCount += amount; // บวกน้ำตามจำนวนที่ส่งมา
            
            if (waterCount >= waterRequired)
            {
                growthStage++; 
                waterCount = 0; 
                UpdateSprite();
                
                if (growthStage == 3) return true;
            }
        }
        return false;
    }

    public void ResetPlot()
    {
        isPlanted = false;
        growthStage = 0;
        waterCount = 0;
        
        if (nameText != null) {
            nameText.text = "";
            nameText.gameObject.SetActive(false);
        }
        
        // รีเซ็ตเป็นสีขาวหรือรูปดินเริ่มต้น (ต้องตั้งค่า Default ใน Manager หรือให้ Manager สั่ง)
        // เพื่อความง่าย เราจะรอให้ UpdateSprite ทำงานตอนเป็น Stage 0
        if (sr != null) sr.sprite = null; 
    }

    public void UpdateSprite()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (currentPlantType.plantName == null) return; // ถ้ายังไม่มีข้อมูลพืชให้ข้ามไป

        switch (growthStage)
        {
            case 0: sr.sprite = currentPlantType.dirt; break;
            case 1: sr.sprite = currentPlantType.seed; break;
            case 2: sr.sprite = currentPlantType.sprout; break;
            case 3: sr.sprite = currentPlantType.mature; break;
        }
        sr.color = Color.white; 
    }
    
    // ฟังก์ชันช่วยสำหรับตอนเริ่มเกม (ให้ Manager ส่งรูปดินมาให้โชว์ก่อน)
    public void SetDirtSprite(Sprite dirt)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = dirt;
    }
}