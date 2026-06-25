using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fullscreen educational transition: reassurance scores → Drop-Cover-Hold graphic → After scene.
/// </summary>
public class TransitionScreenManager : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject transitionRoot;

    [Header("Phase A — Black Reassurance Screen")]
    [SerializeField] private GameObject phaseAPanel;
    [SerializeField] private TMP_Text coreScoreText;
    [SerializeField] private TMP_Text extraScoreText;
    [SerializeField] private TMP_Text reassuranceMessageText;
    [SerializeField] private float phaseADuration = 9f;
    [SerializeField] private string reassuranceMessage =
        "Great job! Remember, this is just a game to help us learn. In real life, we should only pack exactly what is necessary for our emergency bags so they aren't too heavy. The next part of our journey is also a safe virtual simulation to show us what happens after an earthquake. Stay calm and let's see what we've learned!";

    [Header("Phase B — Drop, Cover, Hold On")]
    [SerializeField] private GameObject phaseBPanel;
    [SerializeField] private Image dropCoverHoldImage;
    [SerializeField] private TMP_Text dropCoverHoldCaption;
    [SerializeField] private float phaseBDuration = 3.5f;
    [SerializeField] private string dropCoverHoldCaptionText = "Çök — Kapan — Tutun  (Drop, Cover, and Hold On)";

    [Header("Phase C — Scene Load")]
    [SerializeField] private string nextSceneName = "After";
    [SerializeField] private float fadeBetweenPhases = 0.5f;

    private bool isRunning;

    public event Action OnTransitionStarted;
    public event Action OnPhaseACompleted;
    public event Action OnPhaseBCompleted;
    public event Action OnSceneLoadStarted;

    private void Awake()
    {
        if (transitionRoot != null)
            transitionRoot.SetActive(false);

        if (phaseAPanel != null)
            phaseAPanel.SetActive(false);

        if (phaseBPanel != null)
            phaseBPanel.SetActive(false);
    }

    public void BeginTransition(int coreScore, int extraScore)
    {
        if (isRunning)
            return;

        if (reassuranceMessageText != null)
            reassuranceMessageText.text = reassuranceMessage;

        if (coreScoreText != null)
            coreScoreText.text = $"Temel Simulasyon Puanı: {coreScore}";

        if (extraScoreText != null)
            extraScoreText.text = $"Extra Hazırlılık Puanı: {extraScore}";

        if (dropCoverHoldCaption != null)
            dropCoverHoldCaption.text = dropCoverHoldCaptionText;

        StartCoroutine(RunTransitionSequence());
    }

    private IEnumerator RunTransitionSequence()
    {
        isRunning = true;
        OnTransitionStarted?.Invoke();

        if (transitionRoot != null)
            transitionRoot.SetActive(true);

        yield return ShowPhase(phaseAPanel, phaseADuration);
        OnPhaseACompleted?.Invoke();

        if (fadeBetweenPhases > 0f)
            yield return new WaitForSecondsRealtime(fadeBetweenPhases);

        yield return ShowPhase(phaseBPanel, phaseBDuration);
        OnPhaseBCompleted?.Invoke();

        if (!IsSceneInBuildSettings(nextSceneName))
        {
            Debug.LogError($"Scene \"{nextSceneName}\" is not in Build Settings.");
            isRunning = false;
            yield break;
        }

        OnSceneLoadStarted?.Invoke();

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(nextSceneName);
        if (loadOperation == null)
        {
            isRunning = false;
            yield break;
        }

        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < 0.9f)
            yield return null;

        loadOperation.allowSceneActivation = true;

        while (!loadOperation.isDone)
            yield return null;
    }

    private IEnumerator ShowPhase(GameObject panel, float duration)
    {
        if (phaseAPanel != null && panel != phaseAPanel)
            phaseAPanel.SetActive(false);

        if (phaseBPanel != null && panel != phaseBPanel)
            phaseBPanel.SetActive(false);

        if (panel != null)
            panel.SetActive(true);

        yield return new WaitForSecondsRealtime(duration);

        if (panel != null)
            panel.SetActive(false);
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
}
