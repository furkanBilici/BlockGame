using LLlibs.ZeroDepJson;
using UnityEngine;
using UnityEngine.UI;

public class EulaPpManager : MonoBehaviour
{
    [Header("Eula & Privacy-Policy")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject termsOfUsagePanel;
    [SerializeField] private GameObject privacyPolicyPanel;
    [SerializeField] private Toggle privacePolicy;
    [SerializeField] private Toggle termsOfUsage;
    [SerializeField] private Button nextButton;

    [SerializeField]private LoadSceneManager loadSceneManager;
    void Start()
    {
        if (PlayerPrefs.GetInt("termsofusage", 0) == 1 && PlayerPrefs.GetInt("privacypolicy", 0) == 1)
        {
            Debug.Log("Accepted");
            nextButton.interactable = true;
        }
        else
        {
            OpenEulaPanel();
            nextButton.interactable = false;
        }
        
    }
    void OpenEulaPanel()
    {
        panel.SetActive(true);
        privacePolicy.isOn = PlayerPrefs.GetInt("privacypolicy", 0) == 1;
        termsOfUsage.isOn = PlayerPrefs.GetInt("termsofusage", 0) == 1;
    }

    public void PrivacyPolicyToggle()
    {
        PlayerPrefs.SetInt("privacypolicy",privacePolicy.isOn ? 1 : 0);
        UpdateNextButtonState();
    }

    public void TermsOfUsageToggle()
    {
        PlayerPrefs.SetInt("termsofusage",termsOfUsage.isOn ? 1 : 0);
        UpdateNextButtonState();
    }

    public void NextButton()
    {
        CloseEulaPanel();
    }
    void CloseEulaPanel()
    {
        panel.SetActive(false);
        if(loadSceneManager!=null) StartCoroutine(loadSceneManager.LoadingSceneAsync());

    }
    public void PrivacyPolicyPanelButton()
    {
        if (privacyPolicyPanel.activeSelf)
        {
            privacyPolicyPanel.SetActive(false);
            return;
        }
        privacyPolicyPanel.SetActive(true); 
    }
    public void EulaPanelButton()
    {
        if (termsOfUsagePanel.activeSelf)
        {
            termsOfUsagePanel.SetActive(false);
            return;
        }
       termsOfUsagePanel.SetActive(true);
    }
    public void TermsOfUsageAndPrivacyPolicyButton()
    {
        OpenEulaPanel();
    }
    private void UpdateNextButtonState()
    {
        bool accepted = PlayerPrefs.GetInt("termsofusage", 0) == 1 && PlayerPrefs.GetInt("privacypolicy", 0) == 1;
        nextButton.interactable = accepted;
        PlayerPrefs.Save();
    }
}
