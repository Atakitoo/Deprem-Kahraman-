using UnityEngine;

/// <summary>
/// Toggles a Light component on/off when interacted with.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractableLight : MonoBehaviour, IInteractable
{
    [SerializeField] private Light targetLight;
    [SerializeField] private string promptOn = "Press E / Click to turn on light";
    [SerializeField] private string promptOff = "Press E / Click to turn off light";

    private bool isOn;

    public string InteractionPrompt => isOn ? promptOff : promptOn;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        if (targetLight != null)
            isOn = targetLight.enabled;
    }

    public bool CanInteract(GameObject interactor) => targetLight != null;

    public void Interact(GameObject interactor)
    {
        if (targetLight == null)
            return;

        isOn = !isOn;
        targetLight.enabled = isOn;
    }
}
