using UnityEngine;

public class TestDice : MonoBehaviour
{
    public DiceManager diceManager;

    void Update()
    {
        // กด Spacebar เพื่อจำลองการส่งของขวัญ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int rng = Random.Range(1, 7); // สุ่มเลข 1-6
            
            // ส่งเลขสุ่ม + ชื่อสมมติ + URL รูปว่างๆ ไปให้ DiceManager
            Debug.Log("Simulate Gift: Random Result = " + rng);
            
            // แก้ตรงนี้: เพิ่มชื่อ "UserTest" ตามด้วยเลขสุ่ม ให้ดูเหมือนคนส่งจริงๆ
            diceManager.RollTheDice(rng, "UserTest_" + rng, ""); 
        }
    }
}