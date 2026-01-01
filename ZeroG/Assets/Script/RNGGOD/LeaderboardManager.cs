using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager instance;

    [Header("UI Reference")]
    public TextMeshProUGUI rankListText; // ลาก Text ที่จะใช้โชว์รายชื่อมาใส่

    [Header("Settings")]
    public int maxEntries = 5; // โชว์สูงสุดกี่คน

    // เก็บข้อมูลคนชนะ (ชื่อ + เวลาที่ชนะ)
    private List<string> winners = new List<string>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddWinner(string username)
    {
        // เพิ่มชื่อคนชนะลงไปในรายการ
        // รูปแบบ: "1. mewmewnakub"
        winners.Add(username);

        // ถ้าเกินจำนวนที่ตั้งไว้ ให้ลบคนเก่าสุดออก (หรือจะเอาแบบคะแนนสูงสุดก็ได้ แต่อันนี้เอาแบบล่าสุดก่อน)
        if (winners.Count > maxEntries)
        {
            winners.RemoveAt(0); // ลบคนแรกออก
        }

        UpdateUI();
    }

    void UpdateUI()
{
    if (rankListText == null) return;

    // 🏆 หัวข้อ (ใช้ตัวหนาและสีทอง)
    string header = "";
    string listContent = "";

    for (int i = winners.Count - 1; i >= 0; i--)
    {
        // ตัดเวลาออก (ถ้ามี)
        string nameOnly = winners[i];
        if (nameOnly.Contains(" (")) { nameOnly = nameOnly.Split(" (")[0]; }

        // ✨ ตกแต่งรายชื่อ
        if (i == winners.Count - 1)
        {
            // คนล่าสุด: สีขาวสว่าง + ตัวหนา + มีดาว
            listContent += $"<color=#FFFFFF><b>{nameOnly}</b></color>\n";
        }
        else
        {
            // คนก่อนหน้า: สีเทา และลดขนาดลงนิดนึงเพื่อไม่ให้เบียด
            listContent += $"<size=90%><color=#AAAAAA> {nameOnly}</color></size>\n";
        }
    }

    // รวมข้อความเข้าด้วยกัน
    rankListText.text = header + listContent;
}
}