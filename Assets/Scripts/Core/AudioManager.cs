using UnityEngine;
using UnityEngine.Audio;
<<<<<<< Updated upstream

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource SFXSource;
    public AudioSource MusicSource;

    [Header("Mixer Groups")]
    public AudioMixerGroup SFXGroup;
    public AudioMixerGroup MusicGroup;
=======
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Settings")]
    public AudioMixer MainMixer;
    public string MasterParam = "MasterVol";
    public string MusicParam = "MusicVol";
    public string SFXParam = "SFXVol";

    [Header("Sources")]
    public AudioSource MusicSource;
    public AudioSource SFXSource; // General SFX source for UI or non-positional sounds

    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
>>>>>>> Stashed changes

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
<<<<<<< Updated upstream
            InitializeSources();
=======
            LoadClips();
>>>>>>> Stashed changes
        }
        else
        {
            Destroy(gameObject);
        }
    }

<<<<<<< Updated upstream
    private void InitializeSources()
    {
        if (SFXSource == null)
        {
            SFXSource = gameObject.AddComponent<AudioSource>();
            SFXSource.outputAudioMixerGroup = SFXGroup;
        }

        if (MusicSource == null)
        {
            MusicSource = gameObject.AddComponent<AudioSource>();
            MusicSource.outputAudioMixerGroup = MusicGroup;
            MusicSource.loop = true;
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        // One-shot is better for overlapping sounds like tower fire
        SFXSource.pitch = pitch;
        SFXSource.PlayOneShot(clip, volume);
    }

    public void PlayButtonSound(AudioClip clip)
    {
        PlaySFX(clip, 1f, 1f);
    }

    public void PlayMusic(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null || MusicSource.clip == clip) return;

        MusicSource.clip = clip;
        MusicSource.volume = volume;
        MusicSource.Play();
    }
=======
    private void LoadClips()
    {
        // Auto-load all clips from Resources/Audio
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");
        foreach (var clip in clips)
        {
            _audioClips[clip.name] = clip;
        }
        Debug.Log($"[AudioManager] Loaded {_audioClips.Count} audio clips.");
    }

    public void PlayMusic(string clipName, bool loop = true)
    {
        if (MusicSource == null) return;
        if (!_audioClips.ContainsKey(clipName)) return;

        MusicSource.clip = _audioClips[clipName];
        MusicSource.loop = loop;
        MusicSource.Play();
    }

    public void PlaySFX(string clipName, float volumeScale = 1f, float pitchRandomness = 0.1f)
    {
        if (SFXSource == null) return;
        if (!_audioClips.ContainsKey(clipName)) return;

        // Apply slight pitch randomization for variety
        SFXSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        SFXSource.PlayOneShot(_audioClips[clipName], volumeScale);
    }

    // Volume Control methods (0.0001 to 1.0)
    public void SetMasterVolume(float volume)
    {
        SetMixerVolume(MasterParam, volume);
    }

    public void SetMusicVolume(float volume)
    {
        SetMixerVolume(MusicParam, volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetMixerVolume(SFXParam, volume);
    }

    private void SetMixerVolume(string parameter, float volume)
    {
        if (MainMixer == null) return;
        // Convert 0.0001-1 to Decibels (-80 to 20)
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        MainMixer.SetFloat(parameter, db);
    }
>>>>>>> Stashed changes
}
