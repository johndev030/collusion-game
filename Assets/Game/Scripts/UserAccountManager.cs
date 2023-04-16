using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using PlayFab.Json;
using PlayFab.ServerModels;
using UnityEngine;
using UnityEngine.Events;



public class UserAccountManager : MonoBehaviour
{
    public bool deletePlayerPrefs;
    [Space(5)]
    public string UserName, DisplayName;
    public int MONEY;
    public int TOKENCOUNT;
    public string timeSinceGameStart;
    public PlayFab.ClientModels.EntityTokenResponse EntityToken;
    public string PlayFabID;
    public int ProfilePhotoIndex;
    public bool IsTeamMember;
    public string TeamName;
    public string GroupName;
    public string GroupID;
    public PlayFab.GroupsModels.EntityKey GroupEntity;
    public List<GroupWithRoles> AllGroups;
    public ListGroupMembersResponse ListGroupMembers;
    public List<GroupInvitation> groupInvitations;
    public List<GroupApplication> groupApplications;

    public PlayerData playerData;

    public static UserAccountManager Instance;
    public static UnityEvent<string> OnLoginSuccess = new UnityEvent<string>();
    public static UnityEvent<string> OnLoginFailed = new UnityEvent<string>();
    public static UnityEvent<string> OnRegistrationSuccessfull = new UnityEvent<string>();
    public static UnityEvent<string> OnRegistrationFailed = new UnityEvent<string>();


    //PlayerDashboard
    public static UnityEvent<string> OnGroupCreationResult = new UnityEvent<string>();
    public static UnityEvent<string> OnListGroupMembersResult = new UnityEvent<string>();

    public static UnityEvent<string> OnGroupJoinResult = new UnityEvent<string>();
    public static UnityEvent<string> OnListGroupResult = new UnityEvent<string>();
    public static UnityEvent<string> OnListMembershipOpportunities = new UnityEvent<string>();



    void Awake()
    {
        if (deletePlayerPrefs)
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
        playerData = new PlayerData();
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
                InitPlayerData();
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
                EntityToken = response.EntityToken;
                if (Constants.REMEMBER_ME == "True")
                {
                    Constants.USERNAME = userName;
                    Constants.PASSWORD = password;
                    //Debug.Log("Login Credentials Saved");
                }
                GetPlayerData(EntityToken.Entity.Id, response=> {
                    Debug.Log("PlayerData Retrieved "+ response);
                    playerData = response;
                    //playerData.
                    OnLoginSuccess.Invoke("Account Login Successful");

                }, error=> {
                    Debug.Log("PlayerData could not Retrieved");
                });
                //GetPlayerData("B4FF4383B48ACDBD");

                //SetTitleData(tokens);
                //SetTitleData();

//                Debug.Log(response.SessionTicket);

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

    #region PlayerData
    public static PlayFab.DataModels.EntityKey EntityKeyMaker(string entityId, string entityType)
    {
        return new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType };
    }
    public void InitPlayerData()
    {
        playerData = new PlayerData();
        playerData.isTeamMember = false;
        playerData.GroupID = "0x000000000";
        playerData.ProfilePhoto = 0;

        playerData.MemberType = "";
        playerData.TeamSize = "";
        playerData.TeamSystem = "";
        playerData.Responsiveness = "";

        PlayFab.DataModels.SetObject setObject = new PlayFab.DataModels.SetObject();
        setObject.ObjectName = "PlayerData";
        setObject.DataObject = playerData;
        List<PlayFab.DataModels.SetObject> objects = new List<PlayFab.DataModels.SetObject>();
        objects.Add(setObject);

        PlayFabDataAPI.SetObjects(new PlayFab.DataModels.SetObjectsRequest
        {
            Entity = EntityKeyMaker(EntityToken.Entity.Id, "title_player_account"),
            Objects = objects
        },
        response => { Debug.Log(response.SetResults.Count);
            //GetPlayerData(EntityToken.Entity.Id);
        },
        error => { Debug.Log(error.GenerateErrorReport()); }
        );
    }
    /// <summary>
    /// Update Object Data of user 
    /// </summary>
    /// <param name="PlayFabID"></param>
    public void UpdatePlayerData(string PlayFabID, Action<string> resultCallback, Action<string> errorCallback)
    {
        PlayFab.DataModels.SetObject setObject = new PlayFab.DataModels.SetObject();
        setObject.ObjectName = "PlayerData";
        setObject.DataObject = playerData;
        List<PlayFab.DataModels.SetObject> objects = new List<PlayFab.DataModels.SetObject>();
        objects.Add(setObject);

        PlayFabDataAPI.SetObjects(new PlayFab.DataModels.SetObjectsRequest
        {
            Entity = EntityKeyMaker(PlayFabID, "title_player_account"),
            Objects = objects
        },
        response => {
            //Debug.Log(response.SetResults.Count);
            resultCallback.Invoke("Success");
        },
        error => {
            Debug.Log(error.GenerateErrorReport());
            errorCallback.Invoke("Failed");
        }
        );
    }
    /// <summary>
    /// Get Object Data of user 
    /// </summary>
    /// <param name="PlayFabID"></param>
    public void GetPlayerData(string PlayFabID, Action<PlayerData> resultCallback, Action<string> errorCallback)
    {
        PlayFab.DataModels.SetObject setObject = new PlayFab.DataModels.SetObject();
        setObject.ObjectName = "PlayerData";
        setObject.DataObject = playerData;
        List<PlayFab.DataModels.SetObject> objects = new List<PlayFab.DataModels.SetObject>();
        objects.Add(setObject);

        PlayFabDataAPI.GetObjects(new PlayFab.DataModels.GetObjectsRequest
        {
            Entity = EntityKeyMaker(PlayFabID, "title_player_account"),
        },
        response => {
            //Debug.Log(response.Objects.Count);
            PlayerData playerData = new PlayerData();
            foreach (var item in response.Objects)
            {
                playerData =  JsonConvert.DeserializeObject<PlayerData>(item.Value.DataObject.ToString());
                Debug.Log("isTeamMember " + playerData.isTeamMember);
            }

            resultCallback.Invoke(playerData);
        },
        error => {
            Debug.Log(error.GenerateErrorReport());
            errorCallback.Invoke("Failed");
        }
        );
    }

    #endregion
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
            int temp = UnityEngine.Random.Range(0, 100);
            if (tokens.TokenCount[temp] < 5000)
            {
                uniqueTokens.Add(temp+1);
                tokens.TokenCount[temp] += 1;
//                Debug.Log("uniqueTokens " + uniqueTokens.Count);
            }
        } while (uniqueTokens.Count <5);

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

        //PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        //{
        //    FunctionName = "grantToken",
        //    FunctionParameter = new { ITEMS = itemIds },
        //},
        //OnCloudAddItem01,
        //OnErrorShared);

        
        for (int i = 0; i < itemIds.Count; i++)
        {
            PurchaseItemRequest request = new PurchaseItemRequest();

            request.ItemId = itemIds[i];
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
                         //UpdateItemData();
                     }
                 }
             },
             error => { Debug.Log("Error: " + error.Error); }
            );
        }
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
