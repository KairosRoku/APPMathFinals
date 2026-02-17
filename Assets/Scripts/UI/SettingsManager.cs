using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public AudioMixer MasterMixer;
    
    public Slider MasterVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;

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
    }

    private void Start()
    {
        RefreshUIBindings();
        LoadSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RefreshUIBindings()
    {
        if (MasterVolumeSlider == null) MasterVolumeSlider = FindSliderByName("MasterVolumeSlider");
        if (MusicVolumeSlider == null) MusicVolumeSlider = FindSliderByName("MusicVolumeSlider");
        if (SFXVolumeSlider == null) SFXVolumeSlider = FindSliderByName("SFXVolumeSlider");

        if (MasterVolumeSlider != null) 
        {
            MasterVolumeSlider.onValueChanged.RemoveAllListeners();
            MasterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        if (MusicVolumeSlider != null) 
        {
            MusicVolumeSlider.onValueChanged.RemoveAllListeners();
            MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (SFXVolumeSlider != null) 
        {
            SFXVolumeSlider.onValueChanged.RemoveAllListeners();
            SFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    private Slider FindSliderByName(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) return go.GetComponent<Slider>();
        return null;
    }

    public void SetMasterVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        if (MasterMixer != null) 
        {
            MasterMixer.SetFloat("MasterVol", db);
        }
        
        if (AudioManager.Instance != null) 
        {
            AudioManager.Instance.SetMasterVolume(volume);
        }
        
        PlayerPrefs.SetFloat("MasterVol", volume);
    }

    public void SetMusicVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        if (MasterMixer != null) 
        {
            MasterMixer.SetFloat("MusicVol", db);
        }

        if (AudioManager.Instance != null) 
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }

        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        if (MasterMixer != null) 
        {
            MasterMixer.SetFloat("SFXVol", db);
        }

        if (AudioManager.Instance != null) 
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }

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
