using UnityEngine;

/// <summary>
/// Opens/closes a door via Animator trigger or bool parameter.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractableDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string closeTriggerName = "Close";
    [SerializeField] private bool useOpenBoolParameter;
    [SerializeField] private string openBoolParameter = "IsOpen";
    [SerializeField] private string promptOpen = "Press E / Click to open door";
    [SerializeField] private string promptClose = "Press E / Click to close door";

    private bool isOpen;

    public string InteractionPrompt => isOpen ? promptClose : promptOpen;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = false;
    }

    public bool CanInteract(GameObject interactor) => animator != null;

    public void Interact(GameObject interactor)
    {
        if (animator == null)
            return;

        isOpen = !isOpen;

        if (useOpenBoolParameter)
        {
            animator.SetBool(openBoolParameter, isOpen);
            return;
        }

        animator.SetTrigger(isOpen ? openTriggerName : closeTriggerName);
    }
}
