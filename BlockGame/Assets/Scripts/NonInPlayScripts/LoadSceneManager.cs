using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneManager : MonoBehaviour
{
    [Header("UI things")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private string sceneToLoad = "MainMenu";
    private void Start()
    {
        StartCoroutine(LoadingSceneAsync());
    }
    IEnumerator LoadingSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            if (progress >= 0.9f)
            {
                progressBar.value = 1;
                yield return new WaitForSeconds(1f);
                operation.allowSceneActivation = true;
            }
        }
        yield return null;
    }
}
