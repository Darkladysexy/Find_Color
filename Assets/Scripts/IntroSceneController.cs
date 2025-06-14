using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneController : MonoBehaviour
{
    [Header("Audio Sources (Loa)")]
    [Tooltip("Loa chuyên phát nhạc nền (dài, lặp lại)")]
    public AudioSource musicSource; 
    [Tooltip("Loa chuyên phát hiệu ứng âm thanh (ngắn, không lặp)")]
    public AudioSource sfxSource;

    // Các hàm này sẽ được gọi từ Timeline
    
    // Hàm chung để phát một đoạn nhạc nền
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.volume = 1f;
            musicSource.Play();
        }
    }

    // Hàm chung để phát một hiệu ứng âm thanh ngắn
    public void PlaySfx(AudioClip sfxClip)
    {
        if (sfxSource != null && sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip);
        }
    }

    // Hàm để dừng nhạc nền
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // Hàm được gọi khi kết thúc intro để chuyển scene
    public void EndIntro()
    {
        Debug.Log("Kết thúc Intro, chuyển sang màn chơi...");
        // Ví dụ: SceneManager.LoadScene("Level_1");
    }
}