using TMPro;
using UnityEngine;

/// <summary>
/// Virtual disaster-training tutorial state machine driven by on-screen subtitles.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public enum TutorialState
    {
        Welcome = 0,
        Jump = 1,
        Crouch = 2,
        Phone = 3,
        Complete = 4
    }

    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text subtitleText;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PhoneAppManager phoneUI;

    [Header("State 0 - Welcome")]
    [SerializeField] private float requiredMoveDistance = 5f;
    [SerializeField] private string welcomeSubtitle =
        "Welcome to the Disaster Readiness Simulation. Move around using WASD and hold Shift to run.";

    [Header("State 1 - Jump")]
    [SerializeField] private string jumpSubtitle =
        "An earthquake can cause debris. Practice jumping over the obstacle ahead.";

    [Header("State 2 - Crouch")]
    [SerializeField] private string crouchSubtitle =
        "Ceilings might collapse. Crouch under the low clearance structure ahead.";

    [Header("State 3 - Phone")]
    [SerializeField] private string phoneSubtitle =
        "Pull out your smartphone to check your emergency checklist.";

    [Header("Complete")]
    [SerializeField] private string completeSubtitle =
        "Training module complete. Proceed to the exit zone at the end of the course.";

    private TutorialState currentState = TutorialState.Welcome;
    private float moveBaseline;

    public TutorialState CurrentState => currentState;
    public bool IsTutorialComplete => currentState >= TutorialState.Complete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (playerMovement != null)
        {
            moveBaseline = playerMovement.TotalDistanceMoved;
            playerMovement.OnDistanceMoved += HandleDistanceMoved;
        }

        if (phoneUI != null)
            phoneUI.OnPhoneOpened += HandlePhoneOpened;

        EnterState(TutorialState.Welcome);
    }

    private void OnDisable()
    {
        if (playerMovement != null)
            playerMovement.OnDistanceMoved -= HandleDistanceMoved;

        if (phoneUI != null)
            phoneUI.OnPhoneOpened -= HandlePhoneOpened;
    }

    private void HandleDistanceMoved(float totalDistance)
    {
        if (currentState != TutorialState.Welcome || playerMovement == null)
            return;

        float movedSinceStart = totalDistance - moveBaseline;
        if (movedSinceStart >= requiredMoveDistance)
            AdvanceToState(TutorialState.Jump);
    }

    public void NotifyJumpZoneReached()
    {
        if (currentState == TutorialState.Jump)
            AdvanceToState(TutorialState.Crouch);
    }

    public void NotifyCrouchZoneReached()
    {
        if (currentState == TutorialState.Crouch)
            AdvanceToState(TutorialState.Phone);
    }

    private void HandlePhoneOpened()
    {
        if (currentState == TutorialState.Phone)
            AdvanceToState(TutorialState.Complete);
    }

    private void AdvanceToState(TutorialState nextState)
    {
        if ((int)nextState <= (int)currentState)
            return;

        EnterState(nextState);
    }

    private void EnterState(TutorialState state)
    {
        currentState = state;

        switch (state)
        {
            case TutorialState.Welcome:
                SetSubtitle(welcomeSubtitle);
                if (playerMovement != null)
                    moveBaseline = playerMovement.TotalDistanceMoved;
                break;
            case TutorialState.Jump:
                SetSubtitle(jumpSubtitle);
                break;
            case TutorialState.Crouch:
                SetSubtitle(crouchSubtitle);
                break;
            case TutorialState.Phone:
                SetSubtitle(phoneSubtitle);
                break;
            case TutorialState.Complete:
                SetSubtitle(completeSubtitle);
                break;
        }
    }

    private void SetSubtitle(string text)
    {
        if (subtitleText != null)
            subtitleText.text = text;
    }
}
