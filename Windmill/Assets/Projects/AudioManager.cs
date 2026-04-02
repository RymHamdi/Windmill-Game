using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Library")]
    public SoundLibrary soundLibrary;

    [Header("Audio Source Pool")]
    public int initialPoolSize = 5;

    private List<AudioSource> audioSources = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePool();
        Application.targetFrameRate = 60;
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
            CreateNewAudioSource();
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject go = new GameObject("SFX_AudioSource");
        go.transform.SetParent(transform);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        audioSources.Add(src);
        return src;
    }

    public void Play(string soundName, float pitch = 1f)
    {
        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
                return;
        }
        if (soundLibrary == null)
        {
            Debug.LogError("❌ No SoundLibrary assigned to AudioManager!");
            return;
        }

        var sound = soundLibrary.sounds.Find(s => s.name == soundName);
        if (sound == null || sound.clip == null)
        {
            Debug.LogWarning($"⚠️ Sound not found: {soundName}");
            return;
        }

        AudioSource src = audioSources.Find(s => !s.isPlaying);
        if (src == null)
            src = CreateNewAudioSource();

        src.clip = sound.clip;
        src.volume = sound.volume;
        src.pitch = pitch;
        src.Play();
    }

    public void StopAll()
    {
        foreach (var s in audioSources)
            s.Stop();
    }

    void Update()
    {
        if (!Screen.fullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.fullScreen = true;
        }
    }
}
