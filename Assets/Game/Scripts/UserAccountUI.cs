using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using JetBrains.Annotations;

public class UserAccountUI : MonoBehaviour
{
    public Text msg;
    public GameObject loadingScreen;
    public GameObject loadingBarParent;
    public Image loadingBar;


    private void Awake()
    {
        if (Constants.REMEMBER_ME == "True")
        {
            toggle.isOn = true;
        }
        else
        {
            toggle.isOn = false;
        }
    }
    string email_register, password_register, userName_register, displayName;

    private void OnEnable()
    {
        UserAccountManager.OnLoginSuccess.AddListener(DisplayMessage);
        UserAccountManager.OnLoginSuccess.AddListener(GoToMainMenu);

        UserAccountManager.OnLoginFailed.AddListener(DisplayMessage);
        UserAccountManager.OnRegistrationSuccessfull.AddListener(DisplayMessage);
        UserAccountManager.OnRegistrationFailed.AddListener(DisplayMessage);
    }
    private void OnDisable()
    {
        UserAccountManager.OnLoginSuccess.RemoveListener(DisplayMessage);
        UserAccountManager.OnLoginFailed.RemoveListener(DisplayMessage);
        UserAccountManager.OnRegistrationSuccessfull.RemoveListener(DisplayMessage);
        UserAccountManager.OnRegistrationFailed.RemoveListener(DisplayMessage);
    }

    void DisplayMessage(string message)
    {
        loadingScreen.SetActive(false);
        msg.text = message;
        msg.gameObject.SetActive(true);
    }

    IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(0.8f);
        loadingBarParent.SetActive(true);
        loadingScreen.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main");

        asyncLoad.allowSceneActivation = false;
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            loadingBar.fillAmount = asyncLoad.progress;
            //print("asyncLoad.progress" + asyncLoad.progress);

            // Check if the load has finished
            if (asyncLoad.progress >= 0.9f && setupMoney && fetchTokensData)
            {
                loadingBar.fillAmount = 1;
                //Change the Text to show the Scene is ready
                //m_Text.text = "Press the space bar to continue";
                //Wait to you press the space key to activate the Scene
                //if (Input.GetKeyDown(KeyCode.Space))
                //Activate the Scene
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
    public Toggle toggle;
    public void RememberMe()
    {
        Constants.REMEMBER_ME = toggle.isOn.ToString();
//        print("toggle " + toggle.isOn.ToString());
    }
    void GoToMainMenu(string message)
    {
        //GetUserData();
        GetVirtualCurrency();
        GetTokenInventory();
        StartCoroutine(LoadMainMenu());
    }
    public void DisableDisplayMessage()
    {
        msg.gameObject.SetActive(false);
    }

    public void UpdateUserName(string _userName)
    {
        userName_register = _userName;
    }

    public void UpdateEmail(string _email)
    {
        email_register = _email;
    }

    public void UpdateDisplayName(string _displayName)
    {
        displayName = _displayName;
    }
    public void UpdatePassword(string _password)
    {
        password_register = _password;
    }

    public void Register()
    {
        loadingScreen.SetActive(true);
        UserAccountManager.Instance.CreateUser(userName_register, email_register, password_register, displayName);
    }

    string password_login, userName_login;

    public void UpdateUserNameLogin(string _userName)
    {
        userName_login = _userName;
    }

    public void UpdatePasswordLogin(string _password)
    {
        password_login = _password;
    }

    public void Login()
    {
        loadingScreen.SetActive(true);
        //Debug.Log("userName_login " + userName_login);

        //Debug.Log("password " + password_login);

        UserAccountManager.Instance.Login(userName_login, password_login);
    }


    #region Default Values
    bool setProfilePhoto = false;
    /*
    public void SetTitleData()
    {
        UpdateUserDataRequest updateUserDataRequest = new UpdateUserDataRequest {

            Data = new Dictionary<string, string> {
                { "ProfilePhoto", "0" },
                { "isTeamMember", "false" },
                { "GroupID", "0000000000" }
            },
            Permission = UserDataPermission.Public
        };

      
        //updateUserDataRequest.Data["ProfilePhoto"] = "0";
        //updateUserDataRequest.Data["isTeamMember"] = "false";
        //updateUserDataRequest.Data["GroupID"] = "0000000000";
        //updateUserDataRequest.Permission = UserDataPermission.Public;

        PlayFabClientAPI.UpdateUserData(updateUserDataRequest,
        result =>
        {
            Debug.Log("Successfully updated user data");
            setProfilePhoto = true;
        },
        error =>
        {
            Debug.Log("Profile photo not updated");
            Debug.Log(error.GenerateErrorReport());
            SetTitleData();
        });
    }

    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = UserAccountManager.Instance.PlayFabID
            
        }, result =>
        {
            if (result.Data == null || !result.Data.ContainsKey("ProfilePhoto") || !result.Data.ContainsKey("isTeamMember") || !result.Data.ContainsKey("GroupID"))
            {
                SetTitleData();
            }
            else
            {
                UserAccountManager.Instance.ProfilePhotoIndex = int.Parse(result.Data["ProfilePhoto"].Value);
                UserAccountManager.Instance.IsTeamMember = bool.Parse(result.Data["isTeamMember"].Value);
                setProfilePhoto = true;
            }
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }
    */
    int collusionMoney;
    bool setupMoney = false;

    void GetVirtualCurrency()
    {
        GetUserInventoryRequest requestInventory = new GetUserInventoryRequest();

        PlayFabClientAPI.GetUserInventory(requestInventory, result =>
        {
            result.VirtualCurrency.TryGetValue("CL", out collusionMoney);
            UserAccountManager.Instance.MONEY = collusionMoney;
            //Debug.Log("MONEY : " + collusionMoney);
            setupMoney = true;
        }, error =>

        {
            GetVirtualCurrency();
            Debug.Log(error.ErrorMessage);
        });
    }

    bool fetchTokensData = false;
    void GetTokenInventory()
    {
        UserAccountManager.Instance.uniqueTokens.Clear();
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
           result =>
           {
               if (result.Inventory == null)
               {
                   Debug.Log("No Tokens Data ");
               }
               else
               {
                   //Debug.Log("Tokens: " + result.Data["Tokens"]);
                   List<ItemInstance> items = result.Inventory;
                   for (int i = 0; i < items.Count; i++)
                   {
                       string itemId = items[i].ItemId;
                       //                       Debug.Log("tokenID = " + itemId);
                       int id = int.Parse((itemId.Substring(3, itemId.Length - 3)));
                       //                     Debug.Log("tokenID = " + itemId + " Id =" + id);
                       UserAccountManager.Instance.uniqueTokens.Add(id);
                   }
                   fetchTokensData = true;
               }
           },
           error =>
           {
               Debug.Log("Got error getting titleData:");
               Debug.Log(error.GenerateErrorReport());
               GetTokenInventory();
           });
    }
    #endregion
}
