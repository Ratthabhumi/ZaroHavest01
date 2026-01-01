using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // ทำเป็น Singleton เรียกใช้ได้จากทุกที่

    [Header("Music & Sources")]
    public AudioSource bgmSource; // ลาก AudioSource อันที่ 1 ใส่
    public AudioSource sfxSource; // ลาก AudioSource อันที่ 2 ใส่

    [Header("Sound Clips")]
    public AudioClip bgmClip;       // เพลงประกอบ
    public AudioClip spawnClip;     // เสียงตอนตัวละครตก
    public AudioClip explosionClip; // เสียงระเบิด
    public AudioClip rollClip;      // เสียงลูกเต๋าหมุน
    public AudioClip winClip;       // เสียงคนชนะ/ปิดงาน

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // เริ่มเกมมาเปิดเพลงคลอเลย
        if (bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }
    }

    // ฟังก์ชันเรียกใช้เสียงต่างๆ
    public void PlaySpawn() => PlaySFX(spawnClip);
    public void PlayExplosion() => PlaySFX(explosionClip);
    public void PlayRoll() => PlaySFX(rollClip);
    public void PlayWin() => PlaySFX(winClip);

    void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}