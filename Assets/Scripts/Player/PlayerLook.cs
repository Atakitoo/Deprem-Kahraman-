using UnityEngine;

/// <summary>
/// First-person mouse look. Rotates the player body on Y and the camera on X.
/// </summary>
public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private bool invertY;

    private float pitch;
    private bool lookEnabled = true;

    public bool LookEnabled
    {
        get => lookEnabled;
        set => lookEnabled = value;
    }

    private void Start()
    {
        if (playerBody == null)
            playerBody = transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!lookEnabled || cameraTransform == null)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

        playerBody.Rotate(Vector3.up * mouseX);

        pitch = Mathf.Clamp(pitch + mouseY, minPitch, maxPitch);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
