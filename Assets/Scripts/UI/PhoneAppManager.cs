using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// App-based smartphone UI: home grid, five apps, gameplay freeze while open.
/// </summary>
public class PhoneAppManager : MonoBehaviour
{
    public enum PhoneApp
    {
        Home,
        Checklist,
        Emergency112,
        RobotChat,
        Settings,
        Quit
    }

    [Serializable]
    public class ChecklistEntry
    {
        public ItemType itemType;
        public Toggle checkToggle;
        public TMP_Text labelText;
    }

    [Serializable]
    public class AppScreen
    {
        public PhoneApp app;
        public GameObject panel;
        public Button homeButton;
    }

    [Header("Phone Shell")]
    [SerializeField] private GameObject phoneRoot;

    [Header("App Screens")]
    [SerializeField] private GameObject homeScreenPanel;
    [SerializeField] private AppScreen[] appScreens;

    [Header("Home Screen Icons")]
    [SerializeField] private Button checklistAppButton;
    [SerializeField] private Button emergency112AppButton;
    [SerializeField] private Button robotChatAppButton;
    [SerializeField] private Button settingsAppButton;
    [SerializeField] private Button quitAppButton;

    [Header("Checklist App")]
    [SerializeField] private ChecklistEntry[] checklistEntries;

    [Header("112 Emergency App")]
    [SerializeField] private TMP_Text dialDisplayText;
    [SerializeField] private TMP_Text emergencyStatusText;
    [SerializeField] private string emergencyNumber = "112";
    [SerializeField] private string dispatchSuccessMessage =
        "Dispatch: Emergency services notified. Stay calm and follow simulation protocols.";

    [Header("Robot Chat App")]
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private RectTransform chatContentRect;
    [SerializeField] private TMP_Text chatLogText;
    [SerializeField] private bool autoScrollChatToBottom = true;
    [SerializeField] private string[] initialChatMessages =
    {
        "ROBOT: Welcome to the Disaster Readiness Simulation.",
        "ROBOT: Collect emergency items in the environment — they auto-sync to your Checklist app.",
        "ROBOT: Dial 112 in the Emergency app to practice reporting a disaster.",
        "ROBOT: Adjust training preferences in Settings anytime."
    };

    [Header("Settings App")]
    [SerializeField] private GameSettingsController settingsController;

    [Header("Quit App")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private TMP_Text quitConfirmationText;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKeyPrimary = KeyCode.I;
    [SerializeField] private KeyCode toggleKeySecondary = KeyCode.Tab;

    private readonly StringBuilder dialBuffer = new StringBuilder();
    private PhoneApp currentApp = PhoneApp.Home;
    private bool isPhoneOpen;
    private bool emergencyCallSucceeded;
    private Coroutine scrollChatCoroutine;

    public bool IsPhoneOpen => isPhoneOpen;
    public bool EmergencyCallSucceeded => emergencyCallSucceeded;

    public event Action OnPhoneOpened;
    public event Action OnPhoneClosed;
    public event Action OnEmergencyCallSuccess;

    private void Awake()
    {
        if (phoneRoot != null)
            phoneRoot.SetActive(false);

        if (chatContentRect == null && chatLogText != null)
            chatContentRect = chatLogText.rectTransform.parent as RectTransform;

        WireHomeButtons();
        WireAppBackButtons();
        ConfigureChecklistToggles();
        RefreshChecklistUI();
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemPickedUp += HandleItemPickedUp;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemPickedUp -= HandleItemPickedUp;
    }

    private void Start()
    {
        BuildInitialChatLog();
        ShowApp(PhoneApp.Home);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKeyPrimary) || Input.GetKeyDown(toggleKeySecondary))
            TogglePhone();
    }

    public void TogglePhone()
    {
        SetPhoneOpen(!isPhoneOpen);
    }

    public void SetPhoneOpen(bool open)
    {
        if (isPhoneOpen == open)
            return;

        isPhoneOpen = open;

        if (phoneRoot != null)
            phoneRoot.SetActive(open);

        SetGameplayFrozen(open);

        if (open)
        {
            ShowApp(PhoneApp.Home);
            RefreshChecklistUI();
            OnPhoneOpened?.Invoke();
        }
        else
        {
            OnPhoneClosed?.Invoke();
        }
    }

    public void OpenApp(PhoneApp app)
    {
        if (!isPhoneOpen)
            return;

        ShowApp(app);

        if (app == PhoneApp.Checklist)
            RefreshChecklistUI();

        if (app == PhoneApp.RobotChat)
            ScrollChatToBottom();
    }

    public void GoHome()
    {
        ShowApp(PhoneApp.Home);
    }

    public void OpenChecklistApp() => OpenApp(PhoneApp.Checklist);
    public void OpenEmergency112App() => OpenApp(PhoneApp.Emergency112);
    public void OpenRobotChatApp() => OpenApp(PhoneApp.RobotChat);
    public void OpenSettingsApp() => OpenApp(PhoneApp.Settings);
    public void OpenQuitApp() => OpenApp(PhoneApp.Quit);

    public void DialAppendDigit(string digit)
    {
        if (string.IsNullOrEmpty(digit) || dialBuffer.Length >= 16)
            return;

        dialBuffer.Append(digit);
        UpdateDialDisplay();
    }

    public void DialAppendDigit(int digit) => DialAppendDigit(digit.ToString());

    public void Dial0() => DialAppendDigit(0);
    public void Dial1() => DialAppendDigit(1);
    public void Dial2() => DialAppendDigit(2);
    public void Dial3() => DialAppendDigit(3);
    public void Dial4() => DialAppendDigit(4);
    public void Dial5() => DialAppendDigit(5);
    public void Dial6() => DialAppendDigit(6);
    public void Dial7() => DialAppendDigit(7);
    public void Dial8() => DialAppendDigit(8);
    public void Dial9() => DialAppendDigit(9);

    public void DialClear()
    {
        dialBuffer.Clear();
        UpdateDialDisplay();

        if (emergencyStatusText != null)
            emergencyStatusText.text = string.Empty;
    }

    public void DialBackspace()
    {
        if (dialBuffer.Length == 0)
            return;

        dialBuffer.Length--;
        UpdateDialDisplay();
    }

    public void DialCall()
    {
        string number = dialBuffer.ToString();

        if (number == emergencyNumber)
        {
            emergencyCallSucceeded = true;
            if (emergencyStatusText != null)
                emergencyStatusText.text = dispatchSuccessMessage;

            AppendChatMessage("112 çağrısı alındı. Yardım koordine ediliyor.");
            OnEmergencyCallSuccess?.Invoke();
            return;
        }

        if (emergencyStatusText != null)
            emergencyStatusText.text = "Geçersiz numara. 112'yi arayın.";
    }

    public void QuitToMainMenu()
    {
        if (settingsController != null)
            settingsController.SaveToPlayerPrefs();

        if (!IsSceneInBuildSettings(mainMenuSceneName))
        {
            Debug.LogError($"Scene \"{mainMenuSceneName}\" is not in Build Settings.");
            return;
        }

        StartCoroutine(LoadSceneAsync(mainMenuSceneName));
    }

    public void QuitApplication()
    {
        if (settingsController != null)
            settingsController.SaveToPlayerPrefs();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowQuitConfirmation()
    {
        if (quitConfirmationText != null)
            quitConfirmationText.text = "Simülasyondan çıkıp ana menüye mi dönmek, yoksa uygulamadan çıkmak mı istiyorsunuz?";
    }

    public void AppendRobotMessage(string message) => AppendChatMessage(message);

    private void WireHomeButtons()
    {
        checklistAppButton?.onClick.AddListener(OpenChecklistApp);
        emergency112AppButton?.onClick.AddListener(OpenEmergency112App);
        robotChatAppButton?.onClick.AddListener(OpenRobotChatApp);
        settingsAppButton?.onClick.AddListener(OpenSettingsApp);
        quitAppButton?.onClick.AddListener(OpenQuitApp);
    }

    private void WireAppBackButtons()
    {
        if (appScreens == null)
            return;

        foreach (AppScreen screen in appScreens)
        {
            if (screen.homeButton != null)
                screen.homeButton.onClick.AddListener(GoHome);
        }
    }

    private void ConfigureChecklistToggles()
    {
        if (checklistEntries == null)
            return;

        foreach (ChecklistEntry entry in checklistEntries)
        {
            if (ItemTypeCatalog.IsExtra(entry.itemType))
                continue;

            if (entry.labelText != null)
                entry.labelText.text = InventoryManager.GetDisplayName(entry.itemType);

            if (entry.checkToggle != null)
            {
                entry.checkToggle.interactable = false;
                entry.checkToggle.isOn = false;
            }
        }
    }

    private void RefreshChecklistUI()
    {
        if (checklistEntries == null || InventoryManager.Instance == null)
            return;

        foreach (ChecklistEntry entry in checklistEntries)
        {
            if (entry.checkToggle == null || ItemTypeCatalog.IsExtra(entry.itemType))
                continue;

            bool collected = InventoryManager.Instance.IsCollected(entry.itemType);
            entry.checkToggle.SetIsOnWithoutNotify(collected);
        }
    }

    private void HandleItemPickedUp(ItemType itemType, InventoryManager.ItemPickupKind pickupKind, int pointsAwarded)
    {
        if (pickupKind == InventoryManager.ItemPickupKind.FirstCore)
            RefreshChecklistUI();

        // BeforeSceneManager routes robot education in the Before scene.
        if (BeforeSceneManager.Instance != null)
            return;

        switch (pickupKind)
        {
            case InventoryManager.ItemPickupKind.FirstCore:
                AppendRobotMessage(ItemEducationMessages.GetFirstCoreMessage(itemType));
                break;
            case InventoryManager.ItemPickupKind.DuplicateCore:
                AppendRobotMessage(ItemEducationMessages.GetDuplicateCoreMessage(itemType));
                break;
            case InventoryManager.ItemPickupKind.Extra:
                AppendRobotMessage(ItemEducationMessages.GetExtraMessage(itemType));
                break;
        }
    }

    private void ShowApp(PhoneApp app)
    {
        currentApp = app;

        if (homeScreenPanel != null)
            homeScreenPanel.SetActive(app == PhoneApp.Home);

        if (appScreens == null)
            return;

        foreach (AppScreen screen in appScreens)
        {
            if (screen.panel == null)
                continue;

            screen.panel.SetActive(screen.app == app);
        }

        if (app == PhoneApp.Quit)
            ShowQuitConfirmation();

        if (app == PhoneApp.Settings && settingsController != null)
            settingsController.LoadFromPlayerPrefs();
    }

    private void SetGameplayFrozen(bool frozen)
    {
        if (playerMovement != null)
            playerMovement.MovementEnabled = !frozen;

        if (playerLook != null)
        {
            playerLook.LookEnabled = !frozen;
            playerLook.SetCursorLocked(!frozen);
        }

        if (playerInteraction != null)
            playerInteraction.InteractionEnabled = !frozen;
    }

    private void UpdateDialDisplay()
    {
        if (dialDisplayText != null)
            dialDisplayText.text = dialBuffer.ToString();
    }

    private void BuildInitialChatLog()
    {
        if (chatLogText == null || initialChatMessages == null)
            return;

        chatLogText.text = string.Join("\n\n", initialChatMessages);
        ScrollChatToBottom();
    }

    private void AppendChatMessage(string message)
    {
        if (chatLogText == null || string.IsNullOrEmpty(message))
            return;

        if (string.IsNullOrEmpty(chatLogText.text))
            chatLogText.text = message;
        else
            chatLogText.text += "\n\n" + message;

        ScrollChatToBottom();
    }

    private void ScrollChatToBottom()
    {
        if (!autoScrollChatToBottom || chatScrollRect == null)
            return;

        if (scrollChatCoroutine != null)
            StopCoroutine(scrollChatCoroutine);

        scrollChatCoroutine = StartCoroutine(ScrollChatToBottomNextFrame());
    }

    private IEnumerator ScrollChatToBottomNextFrame()
    {
        yield return null;

        if (chatContentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatContentRect);

        Canvas.ForceUpdateCanvases();

        if (chatScrollRect != null)
            chatScrollRect.verticalNormalizedPosition = 0f;

        scrollChatCoroutine = null;
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

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
            yield break;

        while (!op.isDone)
            yield return null;
    }
}
