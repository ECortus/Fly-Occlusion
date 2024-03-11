using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaxConsent : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text m_messageText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private GameObject menu;

    [Space]
    public string TermsOfService_URL;
    public string PrivacyPolicy_URL;

    private bool Accepted
    {
        get => PlayerPrefs.GetInt("MaxSDK_Consent_Accepted", 0) != 0;
        set
        {
            PlayerPrefs.SetInt("MaxSDK_Consent_Accepted", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => 
        {
            // AppLovin SDK is initialized, start loading ads
        };

        MaxSdk.SetSdkKey("5AAhiuFzwRBZXL6NRkfMQIFE9TpJ-fX4qinXb1VVTh4_1ANSv1qJJ3TSWLnV_Jaq1LLcMr7rXCqTMC0FDqZXu6");
        // MaxSdk.SetUserId("45erdr45hhy5kioi09utmc5wqsopiu563");
        MaxSdk.InitializeSdk();
        
        if (!Accepted)
        {
            menu.SetActive(true);
            acceptButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Accept();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 mousePos = new Vector3(eventData.position.x, eventData.position.y, 0);
        int linkId = TMP_TextUtilities.FindIntersectingLink(m_messageText, mousePos, eventData.pressEventCamera);
        if (linkId !=-1)
        {
            TMP_LinkInfo link = m_messageText.textInfo.linkInfo[linkId];
            if (link.GetLinkID() == "TOS")
            { 
                OpenLink(TermsOfService_URL);
            }
            else if (link.GetLinkID() == "PP")
            {
                OpenLink(PrivacyPolicy_URL);
            }
        }
    }

    private void OpenLink(string link)
    {
        Application.OpenURL(link);
    }

    private void OnButtonClick()
    {
        AppsFlyerEventsSuite.AF_COMPLETE_REGISTRATION("MaxSDK-Accept-Consent");
        Accept();
    }

    private void Accept()
    {
        MaxSdk.SetHasUserConsent(true);
        MaxSdk.SetIsAgeRestrictedUser(false);
        MaxSdk.SetDoNotSell(false);

        Accepted = true;
        
        AppsFlyerEventsSuite.AF_LOGIN();
        
        Destroy(gameObject);
    }
}
