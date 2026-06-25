using UnityEngine;

/// <summary>
/// Sleep / progression gate for the Before scene. Handled by BeforeSceneManager on left-click.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BedInteractable : MonoBehaviour
{
    [SerializeField] private string promptWhenReady = "Click to sleep";
    [SerializeField] private string promptWhenLocked = "Click to interact";

    public string PromptWhenReady => promptWhenReady;
    public string PromptWhenLocked => promptWhenLocked;
}
