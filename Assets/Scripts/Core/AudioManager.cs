using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource SFXSource;
    public AudioSource MusicSource;

    [Header("Mixer Groups")]
    public AudioMixerGroup SFXGroup;
    public AudioMixerGroup MusicGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
}
