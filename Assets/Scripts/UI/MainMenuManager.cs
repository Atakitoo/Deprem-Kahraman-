using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu controller: panel navigation, settings persistence, and async scene loading.
/// Wire button OnClick events to the public methods below in the Inspector.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    private const string PrefMasterVolume = "MainMenu_MasterVolume";
    private const string PrefFullscreen = "MainMenu_Fullscreen";
    private const string MixerVolumeParam = "MasterVolume";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings UI")]
    [SerializeField] private Dropdown screenModeDropdown;
    [SerializeField] private Slider volumeSlider;

    [Header("Scene Loading")]
    [SerializeField] private string tutorialSceneName = "Tutorial";
    [SerializeField] private bool holdActivationUntilReady = true;
    [SerializeField] private float minimumLoadDisplayTime = 0.5f;
    [SerializeField] private GameObject loadingOverlay;

    [Header("Audio")]
    [Tooltip("Optional. When assigned, volume is applied via exposed MasterVolume parameter (dB).")]
    [SerializeField] private AudioMixer masterMixer;
    [Tooltip("Used when no AudioMixer is assigned.")]
    [SerializeField] private bool useAudioListenerVolume = true;

    private bool isLoadingScene;

    private void Awake()
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);

        EnsureSettingsPanelHidden();
    }

    private void Start()
    {
        LoadSettingsFromPlayerPrefs();
        ShowMainPanel();

        if (screenModeDropdown != null)
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnDestroy()
    {
        if (screenModeDropdown != null)
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    // --- Main panel buttons (assign in Inspector OnClick) ---

    public void OnStartGameClicked()
    {
        if (isLoadingScene)
            return;

        if (!IsSceneInBuildSettings(tutorialSceneName))
        {
            Debug.LogError(
                $"Scene \"{tutorialSceneName}\" is not in Build Settings. " +
                "Open File > Build Profiles / Build Settings and add the Tutorial scene.");
            return;
        }

        StartCoroutine(LoadTutorialSceneAsync());
    }

    public void OnSettingsClicked()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OnExitClicked()
    {
        SaveSettingsToPlayerPrefs();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Settings panel ---

    public void OnScreenModeChanged(int dropdownIndex)
    {
        ApplyFullscreen(dropdownIndex == 0);
    }

    public void OnVolumeChanged(float normalizedVolume)
    {
        ApplyMasterVolume(normalizedVolume);
    }

    public void OnSettingsBackClicked()
    {
        SaveSettingsToPlayerPrefs();
        ShowMainPanel();
    }

    // --- Persistence ---

    public void SaveSettingsToPlayerPrefs()
    {
        float volume = volumeSlider != null ? volumeSlider.value : 1f;
        int fullscreen = screenModeDropdown != null ? screenModeDropdown.value : (Screen.fullScreen ? 0 : 1);

        PlayerPrefs.SetFloat(PrefMasterVolume, volume);
        PlayerPrefs.SetInt(PrefFullscreen, fullscreen);
        PlayerPrefs.Save();
    }

    public void LoadSettingsFromPlayerPrefs()
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
            screenModeDropdown.SetValueWithoutNotify(fullscreenIndex);
            screenModeDropdown.RefreshShownValue();
        }

        ApplyFullscreen(fullscreenIndex == 0);
    }

    // --- Internal ---

    private void ShowMainPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    private void EnsureSettingsPanelHidden()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
            settingsPanel.SetActive(false);
    }

    private static void ApplyFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    private void ApplyMasterVolume(float normalizedVolume)
    {
        normalizedVolume = Mathf.Clamp01(normalizedVolume);

        if (masterMixer != null)
        {
            // Linear 0–1 → decibels; -80 dB is effectively silent on most mixers.
            float db = normalizedVolume > 0.0001f
                ? Mathf.Log10(normalizedVolume) * 20f
                : -80f;
            masterMixer.SetFloat(MixerVolumeParam, db);
            return;
        }

        if (useAudioListenerVolume)
            AudioListener.volume = normalizedVolume;
    }

    private static bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }

        return false;
    }

    private IEnumerator LoadTutorialSceneAsync()
    {
        isLoadingScene = true;

        if (loadingOverlay != null)
            loadingOverlay.SetActive(true);

        float loadStartTime = Time.unscaledTime;

        AsyncOperation operation = SceneManager.LoadSceneAsync(tutorialSceneName);
        if (operation == null)
        {
            Debug.LogError($"Failed to start loading scene \"{tutorialSceneName}\".");
            isLoadingScene = false;
            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);
            yield break;
        }

        if (holdActivationUntilReady)
            operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
            yield return null;

        while (Time.unscaledTime - loadStartTime < minimumLoadDisplayTime)
            yield return null;

        operation.allowSceneActivation = true;

        while (!operation.isDone)
            yield return null;

        isLoadingScene = false;
    }
}
