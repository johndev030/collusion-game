using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using PlayFab.ServerModels;
using UnityEngine;
using UnityEngine.Events;



public class UserAccountManager : MonoBehaviour
{
    public string UserName, DisplayName;
    public int MONEY;
    public int TOKENCOUNT;
    public string timeSinceGameStart;

    public string PlayFabID;
    public int ProfilePhotoIndex;

    public static UserAccountManager Instance;
    public static UnityEvent<string> OnLoginSuccess = new UnityEvent<string>();
    public static UnityEvent<string> OnLoginFailed = new UnityEvent<string>();
    public static UnityEvent<string> OnRegistrationSuccessfull = new UnityEvent<string>();
    public static UnityEvent<string> OnRegistrationFailed = new UnityEvent<string>();

    void Awake()
    {
        //PlayerPrefs.DeleteAll();

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

    public void CreateUser(string userName, string email, string password, string displayName)
    {
        PlayFabClientAPI.RegisterPlayFabUser(
            new RegisterPlayFabUserRequest()
            {
                Email = email,
                Password = password,
                Username = userName,
                DisplayName = displayName,
                RequireBothUsernameAndEmail = true

            },
            response =>
            {
                Debug.Log($"Successfully account created: ,{userName},{email}");
                OnRegistrationSuccessfull.Invoke("Account Registered Successfully, Logging In");
                DisplayName = displayName;
                UserName = userName;
                GetTitleData();
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
                //SetTitleData();

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
            new PlayFab.ClientModels.SubtractUserVirtualCurrencyRequest()
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

    public void SetTitleData()
    {
        //string tk = JsonUtility.ToJson(tokens);
        PlayFabServerAPI.SetTitleData(
            new PlayFab.ServerModels.SetTitleDataRequest
            {
                Key = "TimeSinceGameStart",
                Value = DateTime.Today.ToString()
            },
            result =>
            {
                //for (int i = 0; i < uniqueTokens.Count; i++)
                //{
                Debug.Log("TimeSinceGameStart: " + DateTime.Today.ToString());
                //}
                //GetTokensCount();
            },
            error =>
            {
                Debug.Log("Got error setting titleData:");
                Debug.Log(error.GenerateErrorReport());
            }
        ); ;
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
                for (int i = 0; i < uniqueTokens.Count; i++)
                {
                    Debug.Log("TOKEN_" + uniqueTokens[i] + " is ASSIGNED");
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
    public void GetTitleData()
    {
        PlayFabServerAPI.GetTitleData(new PlayFab.ServerModels.GetTitleDataRequest(),
            result =>
            {
                if (result.Data == null)
                {
                    Debug.Log("No Tokens Data " + result.Data);
                }
                else
                {
                    //Debug.Log("Tokens: " + result.Data["Tokens"]);
                    tokens = JsonUtility.FromJson<Tokens>(result.Data["Tokens"]);
                    timeSinceGameStart = result.Data["TimeSinceGameStart"];
                    AssignRandomTokens();
                }
            },
            error =>
            {
                Debug.Log("Got error getting titleData:");
                Debug.Log(error.GenerateErrorReport());
                GetTitleData();
            });
    }
    public List<int> uniqueTokens = new List<int>();
    void AssignRandomTokens()
    {
        do
        {
            int temp = UnityEngine.Random.Range(1, 101);
            if (tokens.TokenCount[temp] < 5000)
            {
                uniqueTokens.Add(temp);
                tokens.TokenCount[temp] += 1;

            }
        } while (uniqueTokens.Count != 5);

        SetTitleData(tokens);
        SetCatalogItem();
    }
    internal bool uniqueTokensGranted = false;

    ItemOwnerDetail itemOwnerDetail;
    void SetCatalogItem()
    {
        List<string> itemIds = new List<string>();
        for (int i = 0; i < uniqueTokens.Count; i++)
        {
            itemIds.Add("tk_" + uniqueTokens[i]);
        }

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "grantToken",
            FunctionParameter = new { ITEMS = itemIds },
        },
        OnCloudAddItem01,
        OnErrorShared);

        /*
        for (int i = 0; i < itemGrants.Count; i++)
        {
            PurchaseItemRequest request = new PurchaseItemRequest();

            request.ItemId = itemGrants[i];
            request.Price = 0;
            request.VirtualCurrency = "CL";
            request.CatalogVersion = "Tokens";
            PlayFabClientAPI.PurchaseItem(request,
             result =>
             {
                 for (int i = 0; i < result.Items.Count; i++)
                 {
                     Debug.Log("Purchased: " + result.Items[i]);
                     if (result.Items[i].CustomData != null)
                     {
                         itemOwnerDetail = JsonUtility.FromJson<ItemOwnerDetail>(result.Items[i].CustomData["Owners"]);
                         itemOwnerDetail.ownerPlayfabId.Add(PlayFabID);
                         itemOwnerDetail.ownerName.Add(DisplayName);
                         UpdateItemData();
                     }
                     else
                     {
                         itemOwnerDetail = new ItemOwnerDetail();
                         itemOwnerDetail.ownerName.Add(name);
                         itemOwnerDetail.ownerPlayfabId.Add(PlayFabID);
                         UpdateItemData();

                     }
                 }
             },
             error => { Debug.Log("Error: " + error.Error); }
            );
        }*/
    }

    void UpdateItemData()
    {
        UpdateUserInventoryItemDataRequest update = new UpdateUserInventoryItemDataRequest();
        string jsonData = JsonUtility.ToJson(itemOwnerDetail);
        //Dictionary<string, string> keyValuePairs = jsonData;
        update.Data.Add("Owners", jsonData);
        PlayFabServerAPI.UpdateUserInventoryItemCustomData(update,
            result =>
            {
                Debug.Log("OwnerName Updated");
            },
            error =>
            {
                Debug.Log("OwnerName Not Updated");
            });
    }

    public void GrantTokens(List<string> itemGrants)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "grantToken",
            FunctionParameter = new { itemGrants },
        },
        OnCloudAddItem01,
        OnErrorShared);
    }
    private static void OnCloudAddItem01(PlayFab.ClientModels.ExecuteCloudScriptResult result)
    {
        Debug.Log(PlayFabSimpleJson.SerializeObject(result.FunctionResult));
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object messageValue; jsonResult.TryGetValue("messageValue", out messageValue);
        Debug.Log((string)messageValue);
    }
    private static void OnErrorShared(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }
    #endregion
}
