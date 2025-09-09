using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource; // background music
    public AudioSource sfxSource;   // sound effects

    [Header("Clips")]
    public AudioClip themeSong;
    public AudioClip clickSound;
    public AudioClip splashSound;
    public AudioClip rainSound;
    public GameObject rainVideo;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persists between scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        PlayMusic(themeSong);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayRain()
    {
        if (rainVideo.activeInHierarchy)
        {
            // If rain video is ON, play looping rain sound
            if (!sfxSource.isPlaying || sfxSource.clip != rainSound)
            {
                sfxSource.clip = rainSound;
                sfxSource.loop = true;
                sfxSource.Play();
            }
        }
        else
        {
            // If rain video is OFF, stop rain sound
            if (sfxSource.isPlaying && sfxSource.clip == rainSound)
            {
                sfxSource.Stop();
                sfxSource.loop = false; // reset for normal SFX usage
                sfxSource.clip = null;
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    void Update()
    {
        PlayRain();
    }
}
