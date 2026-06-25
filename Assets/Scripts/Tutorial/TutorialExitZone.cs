using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Final tutorial trigger: loads the next scene when the player enters and all tutorial steps are done.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialExitZone : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Before";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float minimumLoadTime = 0.5f;
    [SerializeField] private GameObject loadingOverlay;

    private bool isLoading;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !other.CompareTag(playerTag))
            return;

        if (TutorialManager.Instance == null || !TutorialManager.Instance.IsTutorialComplete)
        {
            Debug.Log("Complete all training objectives before leaving the simulation.");
            return;
        }

        if (!IsSceneInBuildSettings(nextSceneName))
        {
            Debug.LogError($"Scene \"{nextSceneName}\" is not in Build Settings.");
            return;
        }

        StartCoroutine(LoadNextSceneAsync());
    }

    private IEnumerator LoadNextSceneAsync()
    {
        isLoading = true;

        if (loadingOverlay != null)
            loadingOverlay.SetActive(true);

        float start = Time.unscaledTime;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        if (op == null)
        {
            isLoading = false;
            yield break;
        }

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        while (Time.unscaledTime - start < minimumLoadTime)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;
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
