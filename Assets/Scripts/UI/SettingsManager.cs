using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio")]
    public AudioMixer MasterMixer;
    
    [Header("UI Elements (Optional Bindings)")]
    public Slider MasterVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadSettings();

        // Add Listeners if sliders are assigned
        if (MasterVolumeSlider != null) MasterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (MusicVolumeSlider != null) MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (SFXVolumeSlider != null) SFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        if (MasterMixer != null) MasterMixer.SetFloat("MasterVol", db);
        if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(volume);
        
        PlayerPrefs.SetFloat("MasterVol", volume);
    }

    public void SetMusicVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        if (MasterMixer != null) MasterMixer.SetFloat("MusicVol", db);
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(volume);

        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        if (MasterMixer != null) MasterMixer.SetFloat("SFXVol", db);
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(volume);

        PlayerPrefs.SetFloat("SFXVol", volume);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void LoadSettings()
    {
        float master = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        float music = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);

        if (MasterVolumeSlider != null) MasterVolumeSlider.value = master;
        if (MusicVolumeSlider != null) MusicVolumeSlider.value = music;
        if (SFXVolumeSlider != null) SFXVolumeSlider.value = sfx;
    }
}
