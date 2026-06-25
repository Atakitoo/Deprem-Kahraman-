using UnityEngine;

/// <summary>
/// Place behind jump / crouch obstacles. Advances the tutorial when the player enters the trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialProgressTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        JumpCheckpoint,
        CrouchCheckpoint
    }

    [SerializeField] private TriggerType triggerType;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireCrouchingForCrouchCheckpoint = true;

    private bool hasFired;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasFired || !other.CompareTag(playerTag))
            return;

        if (TutorialManager.Instance == null)
            return;

        switch (triggerType)
        {
            case TriggerType.JumpCheckpoint:
                TutorialManager.Instance.NotifyJumpZoneReached();
                hasFired = true;
                break;
            case TriggerType.CrouchCheckpoint:
                if (requireCrouchingForCrouchCheckpoint)
                {
                    PlayerMovement movement = other.GetComponent<PlayerMovement>();
                    if (movement == null || !movement.IsCrouching)
                        return;
                }

                TutorialManager.Instance.NotifyCrouchZoneReached();
                hasFired = true;
                break;
        }
    }
}
