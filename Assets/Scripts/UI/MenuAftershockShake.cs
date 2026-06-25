using System.Collections;
using UnityEngine;

/// <summary>
/// Periodic subtle shake on UI and/or camera to suggest aftershocks on the main menu.
/// Attach to the same GameObject as MainMenuManager or to a dedicated "MenuEffects" object.
/// </summary>
public class MenuAftershockShake : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private RectTransform uiShakeRoot;
    [SerializeField] private Camera shakeCamera;

    [Header("Timing")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private Vector2 intervalSeconds = new Vector2(4f, 9f);

    [Header("UI Shake")]
    [SerializeField] private float uiPositionIntensity = 6f;
    [SerializeField] private float uiRotationIntensity = 1.5f;
    [SerializeField] private float uiShakeDuration = 0.35f;

    [Header("Camera Shake (optional)")]
    [SerializeField] private float cameraPositionIntensity = 0.04f;
    [SerializeField] private float cameraRotationIntensity = 0.6f;
    [SerializeField] private float cameraShakeDuration = 0.3f;

    private Vector2 uiBaseAnchoredPosition;
    private Vector3 uiBaseEulerAngles;
    private Vector3 cameraBaseLocalPosition;
    private Vector3 cameraBaseLocalEuler;
    private Coroutine aftershockLoop;

    private void Awake()
    {
        CacheBaseTransforms();
    }

    private void OnEnable()
    {
        if (playOnStart)
            StartAftershockLoop();
    }

    private void OnDisable()
    {
        StopAftershockLoop();
        ResetTransforms();
    }

    public void StartAftershockLoop()
    {
        if (aftershockLoop != null)
            return;

        aftershockLoop = StartCoroutine(AftershockLoop());
    }

    public void StopAftershockLoop()
    {
        if (aftershockLoop == null)
            return;

        StopCoroutine(aftershockLoop);
        aftershockLoop = null;
    }

    /// <summary>Trigger one shake immediately (e.g. from a button hover or title pulse).</summary>
    public void TriggerAftershockNow()
    {
        StartCoroutine(PlaySingleAftershock());
    }

    private IEnumerator AftershockLoop()
    {
        var wait = new WaitForSecondsRealtime(Random.Range(intervalSeconds.x, intervalSeconds.y));

        while (true)
        {
            yield return wait;
            yield return PlaySingleAftershock();
            wait = new WaitForSecondsRealtime(Random.Range(intervalSeconds.x, intervalSeconds.y));
        }
    }

    private IEnumerator PlaySingleAftershock()
    {
        float uiDuration = uiShakeRoot != null ? uiShakeDuration : 0f;
        float camDuration = shakeCamera != null ? cameraShakeDuration : 0f;
        float duration = Mathf.Max(uiDuration, camDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float falloff = 1f - t;
            float noiseSeed = Time.unscaledTime * 40f;

            if (uiShakeRoot != null && elapsed <= uiDuration)
            {
                float px = (Mathf.PerlinNoise(noiseSeed, 0f) - 0.5f) * 2f * uiPositionIntensity * falloff;
                float py = (Mathf.PerlinNoise(0f, noiseSeed) - 0.5f) * 2f * uiPositionIntensity * falloff;
                float rot = (Mathf.PerlinNoise(noiseSeed, noiseSeed) - 0.5f) * 2f * uiRotationIntensity * falloff;

                uiShakeRoot.anchoredPosition = uiBaseAnchoredPosition + new Vector2(px, py);
                uiShakeRoot.localEulerAngles = uiBaseEulerAngles + new Vector3(0f, 0f, rot);
            }

            if (shakeCamera != null && elapsed <= camDuration)
            {
                float ox = (Mathf.PerlinNoise(noiseSeed + 10f, 0f) - 0.5f) * 2f * cameraPositionIntensity * falloff;
                float oy = (Mathf.PerlinNoise(0f, noiseSeed + 10f) - 0.5f) * 2f * cameraPositionIntensity * falloff;
                float cr = (Mathf.PerlinNoise(noiseSeed + 20f, noiseSeed) - 0.5f) * 2f * cameraRotationIntensity * falloff;

                shakeCamera.transform.localPosition = cameraBaseLocalPosition + new Vector3(ox, oy, 0f);
                shakeCamera.transform.localEulerAngles = cameraBaseLocalEuler + new Vector3(0f, 0f, cr);
            }

            yield return null;
        }

        ResetTransforms();
    }

    private void CacheBaseTransforms()
    {
        if (uiShakeRoot != null)
        {
            uiBaseAnchoredPosition = uiShakeRoot.anchoredPosition;
            uiBaseEulerAngles = uiShakeRoot.localEulerAngles;
        }

        if (shakeCamera != null)
        {
            cameraBaseLocalPosition = shakeCamera.transform.localPosition;
            cameraBaseLocalEuler = shakeCamera.transform.localEulerAngles;
        }
    }

    private void ResetTransforms()
    {
        if (uiShakeRoot != null)
        {
            uiShakeRoot.anchoredPosition = uiBaseAnchoredPosition;
            uiShakeRoot.localEulerAngles = uiBaseEulerAngles;
        }

        if (shakeCamera != null)
        {
            shakeCamera.transform.localPosition = cameraBaseLocalPosition;
            shakeCamera.transform.localEulerAngles = cameraBaseLocalEuler;
        }
    }
}
