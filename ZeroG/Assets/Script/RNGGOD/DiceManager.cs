using UnityEngine;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    [Header("--- Settings ---")]
    public float idleSpeed = 10f;
    public float spinSpeed = 1500f;
    public float spinDuration = 2.0f;

    [Header("--- Game Rules (ตั้งเวลารอดชีวิตที่นี่) ---")]
    [Tooltip("เวลาที่ต้องรอดให้ครบ (วินาที) เช่น 180 = 3 นาที")]
    public float gameDuration = 180f; // 👈 ปรับเวลาตรงนี้ทีเดียวจบ!

    [Header("--- Spawn Settings ---")]
    public Transform spawnPoint;
    public GameObject[] characterPrefabs;

    private bool isRolling = false;
    private Quaternion targetRotation;

    void Update()
    {
        if (!isRolling && idleSpeed > 0)
        {
            transform.Rotate(new Vector3(0.5f, 1f, 0.2f) * idleSpeed * Time.deltaTime);
        }
    }

    public void RollTheDice(int result, string username, string profilePicUrl)
    {
        if (isRolling) return;
        StartCoroutine(SpinRoutine(result, username, profilePicUrl));
    }

    IEnumerator SpinRoutine(int result, string username, string profilePicUrl)
    {
        isRolling = true;
        float timer = 0f;

        if (AudioManager.instance != null) AudioManager.instance.PlayRoll();

        Vector3 randomAxis = Random.onUnitSphere; 

        while (timer < spinDuration)
        {
            timer += Time.deltaTime;
            transform.Rotate(randomAxis * spinSpeed * Time.deltaTime);
            yield return null;
        }

        switch (result)
        {
            case 1: targetRotation = Quaternion.Euler(0, 0, 0); break;
            case 2: targetRotation = Quaternion.Euler(0, -90, 0); break;
            case 3: targetRotation = Quaternion.Euler(90, 0, 0); break;
            case 4: targetRotation = Quaternion.Euler(-90, 0, 0); break;
            case 5: targetRotation = Quaternion.Euler(0, 90, 0); break;
            case 6: targetRotation = Quaternion.Euler(180, 0, 0); break;
            default: targetRotation = Quaternion.Euler(0, 0, 0); break;
        }

        float snapDuration = 0.5f; 
        float snapTimer = 0f;
        Quaternion startRot = transform.rotation;

        while (snapTimer < snapDuration)
        {
            snapTimer += Time.deltaTime;
            float t = snapTimer / snapDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);
            yield return null;
        }
        
        transform.rotation = targetRotation;
        isRolling = false;
        
        Debug.Log($"Dice Result: {result} - Spawning {username}");

        // ✅ ส่งเวลา gameDuration ไปด้วย
        SpawnCharacter(result, username, profilePicUrl);
    }

    void SpawnCharacter(int diceNumber, string playerName, string avatarUrl)
    {
        int index = diceNumber - 1;

        if (characterPrefabs == null || characterPrefabs.Length == 0) return;
        if (index < 0 || index >= characterPrefabs.Length) index = 0;

        GameObject newChar = Instantiate(characterPrefabs[index], spawnPoint.position, Quaternion.identity);
        
        if (AudioManager.instance != null) AudioManager.instance.PlaySpawn();

        PlayerController pc = newChar.GetComponent<PlayerController>();
        if (pc != null)
        {
            // ✅ ส่ง gameDuration ไปบอกตัวละคร
            pc.SetupCharacter(playerName, avatarUrl, gameDuration);
        }
    }

    public void ResetAllCharacters()
    {
        Debug.Log("🧹 ล้างกระดาน...");
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in allPlayers)
        {
            Destroy(player.gameObject);
        }
    }
}