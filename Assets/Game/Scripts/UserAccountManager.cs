using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;



public class UserAccountManager : MonoBehaviour
{
    public string UserName;
    public int MONEY;

    public string PlayFabID;
    public int ProfilePhotoIndex;

    public static UserAccountManager Instance;
    public static UnityEvent<string> OnLoginSuccess = new UnityEvent<string>();
    public static UnityEvent<string> OnLoginFailed = new UnityEvent<string>();
    public static UnityEvent<string> OnRegistrationSuccessfull = new UnityEvent<string>();
    public static UnityEvent<string> OnRegistrationFailed = new UnityEvent<string>();


    // Start is called before the first frame update
    void Awake()
    {
        PlayerPrefs.DeleteAll();

        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        if (Constants.REMEMBER_ME == "True")
        {
            if (Constants.USERNAME != "" && Constants.PASSWORD != "")
            {
                FindObjectOfType<UserAccountUI>().loadingScreen.SetActive(true);
                Login(Constants.USERNAME, Constants.PASSWORD);
            }
        }
    }

    public void CreateUser(string userName, string email, string password)

    {
        PlayFabClientAPI.RegisterPlayFabUser(
            new RegisterPlayFabUserRequest()
            {
                Email = email,
                Password = password,
                Username = userName,
                RequireBothUsernameAndEmail = true

            },
            response =>
            {
                Debug.Log($"Successfully account created: ,{userName},{email}");
                OnRegistrationSuccessfull.Invoke("Account Registered Successfully, Logging In");
                GetTokensCount();
                FindObjectOfType<UserAccountUI>().loadingScreen.SetActive(true);
                Login(userName, password);
            },
            error =>
            {
                Debug.Log($"Error: ,{error.Error},{error.ErrorMessage},{error.ErrorDetails}");
                OnRegistrationFailed.Invoke("Account Registeration Failed/n" + error.Error);
            }

            );

    }
    public void Login(string userName, string password)

    {
        PlayFabClientAPI.LoginWithPlayFab(
            new LoginWithPlayFabRequest()
            {
                Password = password,
                Username = userName,
            },
            response =>
            {
                //Debug.Log($"Successfully account created: ,{userName}");
                UserName = userName;
                PlayFabID = response.PlayFabId;

                if (Constants.REMEMBER_ME == "True")
                {
                    Constants.USERNAME = userName;
                    Constants.PASSWORD = password;
                    //Debug.Log("Login Credentials Saved");
                }
                OnLoginSuccess.Invoke("Account Login Successful");
                //SetTitleData(tokens);


            },
            error =>
            {
                Debug.Log($"Error: ,{error.Error},{error.ErrorMessage},{error.ErrorDetails}");
                OnLoginFailed.Invoke("Account Login Failed/n" + error.Error);
            }

            );

    }


    public void BuyToken(int price)
    {
        PlayFabClientAPI.SubtractUserVirtualCurrency(
            new SubtractUserVirtualCurrencyRequest()
            {
                VirtualCurrency = "CL",
                Amount = price

            },
            response =>
            {
                Debug.Log("Item Purchased, Remaining Balance: " + response.Balance);
            },
            error =>
            {
                Debug.Log("Item not Purchased, Details: " + error.ErrorDetails);
            }
            );
    }


    #region Initial Token Assigning
    public void SetTitleData(Tokens tokens)
    {
        string tk = JsonUtility.ToJson(tokens);
        PlayFabServerAPI.SetTitleData(
            new PlayFab.ServerModels.SetTitleDataRequest
            {
                Key = "Tokens",
                Value = tk

            },
            result =>
            {
                for (int i = 0; i < userTokens.Count; i++)
                {
                    Debug.Log("TOKEN_" +userTokens[i]+" is ASSIGNED");
                }
                //GetTokensCount();
            },
            error =>
            {
                Debug.Log("Got error setting titleData:");
                Debug.Log(error.GenerateErrorReport());
            }
        );
    }
    public Tokens tokens;
    public void GetTokensCount()
    {
        PlayFabServerAPI.GetTitleData(new PlayFab.ServerModels.GetTitleDataRequest(),
            result =>
            {
                if (result.Data == null || !result.Data.ContainsKey("Tokens"))
                {
                    Debug.Log("No Tokens Data " + result.Data);

                }
                else { Debug.Log("Tokens: " + result.Data["Tokens"]);
                    tokens = JsonUtility.FromJson<Tokens>(result.Data["Tokens"]);
                    AssignRandomTokens();
                }
            },
            error =>
            {
                Debug.Log("Got error getting titleData:");
                Debug.Log(error.GenerateErrorReport());
                GetTokensCount();
            });
    }
    List<int> userTokens = new List<int>();
    void AssignRandomTokens()
    {
        
        do
        {
            int temp = UnityEngine.Random.Range(1, 101);
            if(tokens.TokenCount[temp] < 5000)
            {
                userTokens.Add(temp);
                tokens.TokenCount[temp] += 1;
            }
        } while (userTokens.Count != 5);

        SetTitleData(tokens);
    }
    #endregion
}
