using System;

// นี่คือกล่องเก็บข้อมูลที่ทั้งเกมปลูกผักและเกมบันได ใช้ร่วมกันได้
[System.Serializable]
public class TikTokEvent 
{ 
    public string type; 
    public string name; 
    public string msg; 
    public int count; 
}