using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// Enum cho nhạc nền
public enum MusicType
{
    None,
    MainTheme,
    MenuTheme,
    GameplayTheme,
    VictoryTheme,
    DefeatTheme
}

// Enum cho hiệu ứng âm thanh
public enum SFXType
{
    None,
    ButtonClick,
    LetterDetected,
    Success,
    Fail,
    Notification
}

public class SoundManager : SingletonMonoBehavior<SoundManager>
{
    [System.Serializable]
    public class Sound
    {
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }

    // SerializableDictionary cho Sound
    [System.Serializable]
    public class MusicDictionary : SerializableDictionary<MusicType, Sound> { }

    [System.Serializable]
    public class SFXDictionary : SerializableDictionary<SFXType, Sound> { }

    [Title("Sound Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1.0f;

    [Title("Sounds")]
    [SerializeField] private MusicDictionary musicLibrary = new MusicDictionary();
    [SerializeField] private SFXDictionary sfxLibrary = new SFXDictionary();

    private MusicType currentMusicType = MusicType.None;
    private Sound currentMusic;
    private bool isMusicMuted = false;
    private bool isSfxMuted = false;

    // Riêng một AudioSource cho nhạc nền
    private AudioSource musicSource;

    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private int poolSize = 5;

    protected override void Awake()
    {
        base.Awake();

        // Tạo AudioSource cho nhạc nền
        musicSource = gameObject.AddComponent<AudioSource>();

        InitializeSounds();
        InitializeAudioPool();
        PreloadUISound();
    }

    private void InitializeSounds()
    {
        // Chỉ cần tạo AudioSource cho hiệu ứng âm thanh
        foreach (var kvp in sfxLibrary)
        {
            Sound sound = kvp.Value;
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.playOnAwake = false;
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume * sfxVolume * masterVolume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }
    }

    private void InitializeAudioPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSourcePool.Add(source);
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
                return source;
        }

        // Nếu không có source khả dụng, tạo thêm
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        audioSourcePool.Add(newSource);
        return newSource;
    }

    #region Music Methods

    public void PlayMusic(MusicType musicType)
    {
        if (musicType == MusicType.None) return;

        if (musicLibrary.TryGetValue(musicType, out Sound music))
        {
            // Dừng nhạc hiện tại nếu đang phát
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }

            currentMusicType = musicType;
            currentMusic = music;

            // Cấu hình AudioSource và phát nhạc
            musicSource.clip = music.clip;
            musicSource.volume = music.volume * musicVolume * masterVolume;
            musicSource.pitch = music.pitch;
            musicSource.loop = music.loop;
            musicSource.mute = isMusicMuted;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music '{musicType}' not found in SoundManager");
        }
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        currentMusicType = MusicType.None;
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    public void CrossFadeMusic(MusicType musicType, float duration)
    {
        if (musicType == MusicType.None) return;

        if (musicLibrary.TryGetValue(musicType, out Sound newMusic))
        {
            if (musicSource.isPlaying)
            {
                StartCoroutine(CrossFade(newMusic, duration));
                currentMusicType = musicType;
            }
            else
            {
                PlayMusic(musicType);
            }
        }
        else
        {
            Debug.LogWarning($"Music '{musicType}' not found in SoundManager");
        }
    }

    private IEnumerator CrossFade(Sound newMusic, float duration)
    {
        float timer = 0;
        float startVolume = musicSource.volume;
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();

        // Cấu hình source tạm thời
        tempSource.clip = newMusic.clip;
        tempSource.volume = 0;
        tempSource.pitch = newMusic.pitch;
        tempSource.loop = newMusic.loop;
        tempSource.mute = isMusicMuted;
        tempSource.Play();

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            musicSource.volume = startVolume * (1 - t);
            tempSource.volume = newMusic.volume * musicVolume * masterVolume * t;

            yield return null;
        }

        musicSource.Stop();

        // Chuyển đổi cài đặt sang musicSource chính
        musicSource.clip = newMusic.clip;
        musicSource.volume = newMusic.volume * musicVolume * masterVolume;
        musicSource.pitch = newMusic.pitch;
        musicSource.loop = newMusic.loop;
        musicSource.Play();

        Destroy(tempSource);
        currentMusic = newMusic;
    }

    #endregion

    #region SFX Methods

    public void PlaySFX(SFXType sfxType)
    {
        if (sfxType == SFXType.None) return;

        if (sfxLibrary.TryGetValue(sfxType, out Sound sfx))
        {
            AudioSource source = GetAvailableSource();
            source.clip = sfx.clip;
            source.volume = sfx.volume * sfxVolume * masterVolume;
            source.pitch = sfx.pitch;
            source.Play();
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxType}' not found in SoundManager");
        }
    }

    public void PlaySFXWithPitch(SFXType sfxType, float pitch)
    {
        if (sfxType == SFXType.None) return;

        if (sfxLibrary.TryGetValue(sfxType, out Sound sfx))
        {
            float originalPitch = sfx.pitch;
            sfx.source.pitch = pitch;
            sfx.source.Play();
            sfx.source.pitch = originalPitch;
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxType}' not found in SoundManager");
        }
    }

    public void StopSFX(SFXType sfxType)
    {
        if (sfxType == SFXType.None) return;

        if (sfxLibrary.TryGetValue(sfxType, out Sound sfx))
        {
            sfx.source.Stop();
        }
    }

    #endregion

    #region Volume Controls

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        UpdateAllVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        UpdateMusicVolume();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        UpdateSFXVolume();
    }

    private void UpdateAllVolumes()
    {
        UpdateMusicVolume();
        UpdateSFXVolume();
    }

    private void UpdateMusicVolume()
    {
        if (musicSource != null && currentMusic != null)
        {
            musicSource.volume = currentMusic.volume * musicVolume * masterVolume;
        }
    }

    private void UpdateSFXVolume()
    {
        foreach (var kvp in sfxLibrary)
        {
            kvp.Value.source.volume = kvp.Value.volume * sfxVolume * masterVolume;
        }
    }

    public void MuteMusic(bool mute)
    {
        isMusicMuted = mute;
        if (musicSource != null)
        {
            musicSource.mute = mute;
        }
    }

    public void MuteSFX(bool mute)
    {
        isSfxMuted = mute;
        foreach (var kvp in sfxLibrary)
        {
            kvp.Value.source.mute = mute;
        }
    }

    public void MuteAll(bool mute)
    {
        MuteMusic(mute);
        MuteSFX(mute);
    }

    #endregion

    #region Utility Methods

    public bool IsMusicPlaying(MusicType musicType)
    {
        return musicType != MusicType.None &&
               currentMusicType == musicType &&
               musicSource.isPlaying;
    }

    public bool IsCurrentMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    public MusicType GetCurrentMusicType()
    {
        return currentMusicType;
    }

    public void SetMusicLoop(bool loop)
    {
        if (musicSource != null)
        {
            musicSource.loop = loop;
            if (currentMusic != null)
            {
                currentMusic.loop = loop;
            }
        }
    }

    public void SetSFXLoop(SFXType sfxType, bool loop)
    {
        if (sfxType == SFXType.None) return;

        if (sfxLibrary.TryGetValue(sfxType, out Sound sfx))
        {
            sfx.loop = loop;
            sfx.source.loop = loop;
        }
    }

    #endregion

    public void PreloadUISound()
    {
        // Đảm bảo ButtonClick sound đã được load vào bộ nhớ
        if (sfxLibrary.TryGetValue(SFXType.ButtonClick, out Sound sfx))
        {
            // Phát âm thanh với volume = 0 để preload
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.clip = sfx.clip;
            tempSource.volume = 0f;
            tempSource.Play();

            // Destroy sau khi đã load
            Destroy(tempSource, 0.1f);
        }
    }
}