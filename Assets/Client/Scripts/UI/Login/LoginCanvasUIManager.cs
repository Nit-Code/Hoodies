using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedScripts.DataId;

public class LoginCanvasUIManager : MonoBehaviour
{
    // reference
    private AudioManager myAudioManagerReference;
    [SerializeField] private LoginSceneUIManager myLoginSceneUIManagerReference;

    [SerializeField] GameObject myErrorPopup;
    [SerializeField] GameObject myLoadingPopup;

    private void Start()
    {
        myAudioManagerReference = FindObjectOfType<AudioManager>();
    }

    public void PlayClickSound()
    {
        if (myAudioManagerReference != null) 
        {
            myAudioManagerReference.PlaySound(AudioId.SOUND_MENU_CLICK);
        }
        else
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - PlayClickSound()");
        }
    }

    public void OnConfirmEmailAreaBackButton()
    {
        PlayClickSound();
        myLoginSceneUIManagerReference.BackFromConfirmEmailArea();
    }

    public void OnLoginButton() 
    {
        PlayClickSound();
        myLoginSceneUIManagerReference.StartLogin();
    }

    public void OnSignUpButton() 
    {
        PlayClickSound();
        myLoginSceneUIManagerReference.StartSignUp();
    }

    // Popups
    public void ShowLoadingPopup()
    {
        myLoadingPopup.SetActive(true);
    }

    public void HideLoadingPopup()
    {
        myLoadingPopup.SetActive(false);
    }

    public void ShowErrorPopup()
    {
        myErrorPopup.SetActive(true);
    }

    public void HideErrorPopup()
    {
        myErrorPopup.SetActive(false);
    }
}
