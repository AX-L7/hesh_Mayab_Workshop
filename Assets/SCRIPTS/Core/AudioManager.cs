using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] int sfxPoolSize = 10;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] float masterVolume = 1f;
    [SerializeField] [Range(0f, 1f)] float musicVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] float sfxVolume = 0.8f;

    [Header("Music")]
    [SerializeField] AudioClip menuMusic;
    [SerializeField] AudioClip gameplayMusic;
    [SerializeField] float musicFadeDuration = 1f;

    AudioSource[] sfxPool;
    int sfxPoolIndex;

    public static AudioManager Instance { get; private set; }

    public float MasterVolume { get => masterVolume; set { masterVolume = Mathf.Clamp01(value); UpdateVolumes(); } }
    public float MusicVolume { get => musicVolume; set { musicVolume = Mathf.Clamp01(value); UpdateVolumes(); } }
    public float SFXVolume { get => sfxVolume; set { sfxVolume = Mathf.Clamp01(value); } }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeSFXPool();
    }

    void InitializeSFXPool()
    {
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFX_{i}");
            sfxObj.transform.SetParent(transform);
            sfxPool[i] = sfxObj.AddComponent<AudioSource>();
            sfxPool[i].playOnAwake = false;
            sfxPool[i].spatialBlend = 0f;
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource source = GetNextSFXSource();
        source.spatialBlend = 0f;
        source.clip = clip;
        source.volume = sfxVolume * masterVolume * volumeScale;
        source.Play();
    }

    public void PlaySFX(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource source = GetNextSFXSource();
        source.transform.position = position;
        source.spatialBlend = 1f;
        source.clip = clip;
        source.volume = sfxVolume * masterVolume * volumeScale;
        source.maxDistance = 30f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.Play();
    }

    AudioSource GetNextSFXSource()
    {
        AudioSource source = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;
        return source;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void PlayMenuMusic() => PlayMusic(menuMusic);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic);

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }
}
