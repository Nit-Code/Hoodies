using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SharedScripts.DataId;

public class LoginSceneUIManager : MonoBehaviour
{
    private enum LoginCanvasId
    {
        INVALID,
        LOGIN,
        NONE
    }

    [SerializeField] private LoginCanvasId myBaseCanvas;
    private LoginCanvasId myCurrentCanvas;
    private Dictionary<LoginCanvasId, GameObject> myCanvasMap;

    // canvas
    [SerializeField] private LoginCanvasUIManager myLoginCanvasReference;

    // TODO: If possible these UI specific members should exist on LoginCanvasUIManager
    [Header("To Relocate")]
    #region ToReloacte
    // panel
    [SerializeField] private GameObject myMenuPanel;

    // area
    [SerializeField] private GameObject myUnauthenticatedArea;
    [SerializeField] private GameObject myConfirmEmailArea;

    // input
    private List<TMP_InputField> myInputFields;
    [SerializeField] private TMP_InputField myLoginEmailInputField;
    [SerializeField] private TMP_InputField myLoginPasswordInputField;
    [SerializeField] private TMP_InputField mySignUpEmailInputField;
    [SerializeField] private TMP_InputField mySignUpUsernameInputField;
    [SerializeField] private TMP_InputField mySignUpPasswordInputField;
    #endregion

    private AuthenticationManager myAuthenticationManagerReference;
    private SceneController mySceneControllerReference;

    private void Awake()
    {
        LoadCanvasMap();
        myInputFields = new List<TMP_InputField> { myLoginEmailInputField, myLoginPasswordInputField, mySignUpEmailInputField, mySignUpUsernameInputField, mySignUpPasswordInputField };
        DeactivateAllCanvas();
        myCurrentCanvas = LoginCanvasId.NONE;
        SwitchToBaseCanvas();
        
        myMenuPanel.SetActive(false);
        myUnauthenticatedArea.SetActive(false);
    }

    private void Start()
    {
        Debug.Log("[HOOD][LOGIN] - LoginSceneUIManager/Start");
        myAuthenticationManagerReference = FindObjectOfType<AuthenticationManager>();
        mySceneControllerReference = FindObjectOfType<SceneController>();
        RefreshTokenAndGoToMenu();
    }

    private void LoadCanvasMap()
    {
        myCanvasMap = new Dictionary<LoginCanvasId, GameObject>();
        myCanvasMap.Add(LoginCanvasId.LOGIN, myLoginCanvasReference.gameObject);
    }

    private void SwitchCanvas(LoginCanvasId aTargetCanvas)
    {
        if (aTargetCanvas == myCurrentCanvas || aTargetCanvas == LoginCanvasId.INVALID)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - SwitchCanvas()");
            return;
        }

        // Deactivate current canvas if any
        if (myCurrentCanvas == LoginCanvasId.NONE || SetCanvasActiveValue(myCurrentCanvas, false))
        {
            // Activate target canvas if any
            if (SetCanvasActiveValue(aTargetCanvas, true))
            {
                myCurrentCanvas = aTargetCanvas;
            }
        }
    }

    private bool SetCanvasActiveValue(LoginCanvasId anId, bool aValue)
    {
        if (anId == LoginCanvasId.INVALID)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - SetCanvasActiveValue()");
            return false;
        }

        if (myCanvasMap.TryGetValue(anId, out GameObject canvasGameObject))
        {
            if (canvasGameObject != null)
            {
                canvasGameObject.SetActive(aValue);
                return true;
            }
        }

        Debug.LogError("[HOOD][CLIENT][SCENE] - SetCanvasActiveValue()");
        return false;
    }

    private void DeactivateAllCanvas()
    {
        SetAllCanvasActiveValue(false);
        myCurrentCanvas = LoginCanvasId.NONE;
    }

    private void SetAllCanvasActiveValue(bool aValue)
    {
        foreach (KeyValuePair<LoginCanvasId, GameObject> canvas in myCanvasMap)
        {
            if (canvas.Key == LoginCanvasId.INVALID)
            {
                Debug.LogError("[HOOD][CLIENT][SCENE] - SetAllCanvasActiveValue()");
                continue;
            }

            SetCanvasActiveValue(canvas.Key, aValue);
        }
    }

    public void SwitchToBaseCanvas()
    {
        if (myBaseCanvas == LoginCanvasId.INVALID)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - BackToBaseCanvas()");
            return;
        }

        SwitchCanvas(myBaseCanvas);
    }

    public void BackFromConfirmEmailArea() 
    {
        ShowLoadingText();
        RefreshTokenAndGoToMenu();
    }

    public async void RefreshTokenAndGoToMenu()
    {
        bool successfulRefresh = await myAuthenticationManagerReference.RefreshSession();
        myMenuPanel.SetActive(true);
        LoadMenu(successfulRefresh);
    }

    private void LoadMenu(bool anIsSuccessfulAuthentication)
    {
        ClearInputFields();
        myLoginCanvasReference.HideLoadingPopup();
        myLoginCanvasReference.HideErrorPopup();

        if (anIsSuccessfulAuthentication)
        {
            Debug.Log("[HOOD][LOGIN] - Session token refresh success.");

            UnloadUnauthenticatedArea();
            UnloadConfirmEmailMenu();
            if (mySceneControllerReference != null)
            {
                Debug.Log("[HOOD][LOGIN] - Session token refresh success.");
                mySceneControllerReference.LoadScene(SceneId.MENU);
            }
        }
        else
        {
            Debug.Log("[HOOD][LOGIN] - Session token refresh failed.");
            UnloadConfirmEmailMenu();
            LoadUnauthenticatedArea();
        }
    }

    private void LoadConfirmEmailMenu()
    {
        myLoginCanvasReference.HideErrorPopup();

        if (myConfirmEmailArea != null)
        {
            myConfirmEmailArea.SetActive(true);
        }
    }

    private void LoadUnauthenticatedArea()
    {
        if (myUnauthenticatedArea != null)
        {
            myUnauthenticatedArea.SetActive(true);
        }
    }

    private void UnloadConfirmEmailMenu()
    {
        if (myConfirmEmailArea != null)
        {
            myConfirmEmailArea.SetActive(false);
        }
    }

    private void UnloadUnauthenticatedArea()
    {
        if (myUnauthenticatedArea != null)
        {
            myUnauthenticatedArea.SetActive(false);
        }
    }

    public void StartLogin() 
    {
        ShowLoadingText();
        TryLogin();
    }

    public async void TryLogin()
    {
        bool successfulLogin = await myAuthenticationManagerReference.Login(myLoginEmailInputField.text, myLoginPasswordInputField.text);        
        LoadMenu(successfulLogin);
        myLoginCanvasReference.ShowErrorPopup();
    }

    public void StartSignUp() 
    {
        ShowLoadingText();
        TrySignUp();
    }

    public async void TrySignUp()
    {
        bool successfulSignup = await myAuthenticationManagerReference.Signup(mySignUpUsernameInputField.text, mySignUpEmailInputField.text, mySignUpPasswordInputField.text);

        if (successfulSignup)
        {
            UnloadUnauthenticatedArea();
            LoadConfirmEmailMenu();

            // copy over the new credentials to make the process smoother
            mySignUpEmailInputField.text = myLoginEmailInputField.text;
            mySignUpPasswordInputField.text = myLoginEmailInputField.text;
        }
        else
        {
            myLoginCanvasReference.ShowErrorPopup();
        }

        myLoginCanvasReference.HideLoadingPopup();
    }

    public void StartGame()
    {
        mySceneControllerReference.LoadScene(SceneId.MENU);
        myLoginCanvasReference.HideLoadingPopup();
    }

    private void ShowLoadingText()
    {
        myUnauthenticatedArea.SetActive(false);
        myConfirmEmailArea.SetActive(false);

        myLoginCanvasReference.ShowLoadingPopup();
    }

    private void ClearInputFields()
    {
        foreach (TMP_InputField inputField in myInputFields)
        {
            inputField.text = "";
        }
    }
}
