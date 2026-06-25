using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shared volume / display settings persisted via PlayerPrefs (same keys as MainMenuManager).
/// </summary>
public class GameSettingsController : MonoBehaviour
{
    private const string PrefMasterVolume = "MainMenu_MasterVolume";
    private const string PrefFullscreen = "MainMenu_Fullscreen";
    private const string MixerVolumeParam = "MasterVolume";

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private bool useAudioListenerVolume = true;
    [SerializeField] private bool loadOnEnable = true;
    [SerializeField] private bool saveOnDisable = true;

    private void OnEnable()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (screenModeDropdown != null)
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);

        if (loadOnEnable)
            LoadFromPlayerPrefs();
    }

    private void OnDisable()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (screenModeDropdown != null)
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);

        if (saveOnDisable)
            SaveToPlayerPrefs();
    }

    public void OnVolumeChanged(float normalizedVolume) => ApplyMasterVolume(normalizedVolume);

    public void OnScreenModeChanged(int index) => Screen.fullScreen = index == 0;

    public void SaveToPlayerPrefs()
    {
        float volume = volumeSlider != null ? volumeSlider.value : 1f;
        int fullscreen = screenModeDropdown != null ? screenModeDropdown.value : (Screen.fullScreen ? 0 : 1);

        PlayerPrefs.SetFloat(PrefMasterVolume, volume);
        PlayerPrefs.SetInt(PrefFullscreen, fullscreen);
        PlayerPrefs.Save();
    }

    public void LoadFromPlayerPrefs()
    {
        float volume = PlayerPrefs.GetFloat(PrefMasterVolume, 1f);
        int fullscreenIndex = PlayerPrefs.GetInt(PrefFullscreen, Screen.fullScreen ? 0 : 1);

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
            ApplyMasterVolume(volume);
        }

        if (screenModeDropdown != null)
        {
            if (screenModeDropdown.options.Count == 0)
            {
                screenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Full Screen"));
                screenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Windowed"));
            }

            screenModeDropdown.SetValueWithoutNotify(fullscreenIndex);
            screenModeDropdown.RefreshShownValue();
        }

        Screen.fullScreen = fullscreenIndex == 0;
    }

    private void ApplyMasterVolume(float normalizedVolume)
    {
        normalizedVolume = Mathf.Clamp01(normalizedVolume);

        if (masterMixer != null)
        {
            float db = normalizedVolume > 0.0001f
                ? Mathf.Log10(normalizedVolume) * 20f
                : -80f;
            masterMixer.SetFloat(MixerVolumeParam, db);
            return;
        }

        if (useAudioListenerVolume)
            AudioListener.volume = normalizedVolume;
    }
}
