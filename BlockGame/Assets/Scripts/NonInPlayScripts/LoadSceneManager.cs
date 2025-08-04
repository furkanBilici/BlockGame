using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.UI;

public class LoadSceneManager : MonoBehaviour
{
    [Header("UI things")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private string sceneToLoad = "MainMenu";

   
    private void Start()
    {
        CheckLanguage();
        if (PlayerPrefs.GetInt("termsofusage", 0) == 1 && PlayerPrefs.GetInt("privacypolicy", 0) == 1)
        {
            StartCoroutine(LoadingSceneAsync());
            return;
        }     
        
    }
    public IEnumerator LoadingSceneAsync()
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
  
    [Header("Language")]
    [SerializeField] private GameObject trFlag;
    [SerializeField] private Locale english;
    [SerializeField] private Locale turkish;
    public void ChangeLanguage()
    {
        if (PlayerPrefs.GetString("Locale", english.Identifier.Code) == english.Identifier.Code)
        {
            LocalizationSettings.SelectedLocale = turkish;
            trFlag.SetActive(true);
            PlayerPrefs.SetString("Locale", turkish.Identifier.Code);
            PlayerPrefs.Save();
        }
        else if (PlayerPrefs.GetString("Locale", english.Identifier.Code) == turkish.Identifier.Code)
        {
            LocalizationSettings.SelectedLocale = english;
            trFlag.SetActive(false);
            PlayerPrefs.SetString("Locale", english.Identifier.Code);
            PlayerPrefs.Save();
        }
    }
    void CheckLanguage()
    {
        if (PlayerPrefs.GetString("Locale", english.Identifier.Code) == turkish.Identifier.Code)
        {
            LocalizationSettings.SelectedLocale = turkish;
            trFlag.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetString("Locale", english.Identifier.Code);
            LocalizationSettings.SelectedLocale = english;
            trFlag.SetActive(false);
        }
    }
}

