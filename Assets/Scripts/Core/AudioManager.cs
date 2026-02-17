using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer MainMixer;
    public string MasterParam = "MasterVol";
    public string MusicParam = "MusicVol";
    public string SFXParam = "SFXVol";

    public AudioSource MusicSource;
    public AudioSource SFXSource;

    public AudioClip DefaultBGM;
    public AudioClip DefaultClickSFX;

    private Dictionary<string, AudioClip> _clipLibrary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Instance.gameObject != gameObject)
            {
                Destroy(Instance.gameObject);
            }
            else
            {
                Destroy(Instance);
            }
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadLibrary();
    }

    private void LoadLibrary()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");
        foreach (var clip in clips)
        {
            _clipLibrary[clip.name] = clip;
        }
    }

    private void Start()
    {
        if (DefaultBGM != null)
        {
            PlayMusic(DefaultBGM);
        }
        else
        {
            PlayMusic("BackgroundMusic");
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (MusicSource == null || clip == null) return;
        if (MusicSource.clip == clip && MusicSource.isPlaying) return;

        MusicSource.clip = clip;
        MusicSource.loop = loop;
        MusicSource.Play();
    }

    public void PlayMusic(string clipName, bool loop = true)
    {
        if (_clipLibrary.TryGetValue(clipName, out AudioClip clip))
        {
            PlayMusic(clip, loop);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitchRandomness = 0.1f)
    {
        if (SFXSource == null || clip == null) return;

        SFXSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        SFXSource.PlayOneShot(clip, volume);
    }

    public void PlaySFX(string clipName, float volume = 1f, float pitchRandomness = 0.1f)
    {
        if (_clipLibrary.TryGetValue(clipName, out AudioClip clip))
        {
            PlaySFX(clip, volume, pitchRandomness);
        }
    }

    public void PlayClickSFX()
    {
        if (DefaultClickSFX != null)
        {
            PlaySFX(DefaultClickSFX, 0.6f);
        }
        else
        {
            PlaySFX("Button_01", 0.6f);
        }
    }

    public void SetMasterVolume(float volume)
    {
        SetMixerParameter(MasterParam, volume);
    }

    public void SetMusicVolume(float volume)
    {
        SetMixerParameter(MusicParam, volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetMixerParameter(SFXParam, volume);
    }

    private void SetMixerParameter(string paramName, float volume)
    {
        if (MainMixer == null) return;
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        MainMixer.SetFloat(paramName, db);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
