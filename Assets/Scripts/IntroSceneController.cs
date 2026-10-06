using UnityEngine;

public class IntroSceneController : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Plays background music (long, looping).")]
    public AudioSource musicSource;
    [Tooltip("Plays short sound effects (non-looping).")]
    public AudioSource sfxSource;

    private const float k_MusicVolume = 1f;

    // These methods are invoked from the Timeline.

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.volume = k_MusicVolume;
            musicSource.Play();
        }
    }

    public void PlaySfx(AudioClip sfxClip)
    {
        if (sfxSource != null && sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void EndIntro()
    {
        // Hooked to the Timeline; the scene transition is configured in the Timeline signal.
    }
}
