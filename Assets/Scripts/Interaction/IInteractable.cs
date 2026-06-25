using UnityEngine;

/// <summary>
/// Implement on doors, lights, and other world objects the player can use with E / left click.
/// </summary>
public interface IInteractable
{
    string InteractionPrompt { get; }

    bool CanInteract(GameObject interactor);

    void Interact(GameObject interactor);
}
