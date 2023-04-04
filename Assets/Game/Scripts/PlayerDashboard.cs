using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Guild;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using PlayFab.Json;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
public class PlayerDashboard : MonoBehaviour
{
    public Text uniqueTokenCount; public Text[] moneyTexts;
    public Text timeElapsed, tradeCompleted, dollarVolume, collusionExecuted, averageTeamSize;
    [Space(5)]
    [Header("Panels")]
    public GameObject[] panels;
    private GuildController guildController;
    /// <summary>
    /// title_player_account and master_player_account
    /// </summary>
    public static string[] defaultPlayerTitleID = { "1CEF8B074D4740BC", "655DC02EC6E815DE" };
    string[] defaultPlayer= { "1CEF8B074D4740BC" };


    void Start()
    {
        guildController = new GuildController();

        for (int i = 0; i < moneyTexts.Length; i++)
        {
            moneyTexts[i].text = "$" + UserAccountManager.Instance.MONEY.ToString();
        }
        UpdatePlayerTitleData();
        UpdateTokensCount();
        UpdateTitleData();
    }

    void UpdateTokensCount()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest()
        , result =>
        {
            UserAccountManager.Instance.TOKENCOUNT = result.Inventory.Count;
            uniqueTokenCount.text = UserAccountManager.Instance.TOKENCOUNT.ToString() + "/100";
            //for (int i = 0; i < result.Inventory.Count; i++)
            //{
            //    Debug.Log("You have " + result.Inventory[i].ItemId);
            //}
        },
        error =>
        {
            Debug.Log("Error Getting Tokens");
        });
    }
    string timeSinceGameStart_server, tradeCompleted_server, dollarVolume_server, collusionExecuted_server, averageTeamSize_server;

    void UpdateTitleData()
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
                    timeSinceGameStart_server = result.Data["TimeSinceGameStart"];
                    averageTeamSize_server = result.Data["AverageTeamSize"];
                    collusionExecuted_server = result.Data["CollusionExecuted"];
                    dollarVolume_server = result.Data["DollarVolume"];
                    tradeCompleted_server = result.Data["TradeCompleted"];

                    SetTitleData();
                }
            },
            error =>
            {
                Debug.Log("Got error getting titleData:");
                Debug.Log(error.GenerateErrorReport());
            });
    }

    void SetTitleData()
    {
        if (timeSinceGameStart_server != "")
        {
            CultureInfo culture = new CultureInfo("en-US");
            DateTime startTime = Convert.ToDateTime(timeSinceGameStart_server, culture);
            TimeSpan timeSinceGameStarts = DateTime.Now - startTime;
            if (timeSinceGameStarts.Days < 1)
            {
                timeElapsed.text = timeSinceGameStarts.Hours + " Hours";
            }
            else if (timeSinceGameStarts.Days < 30)
            {
                timeElapsed.text = timeSinceGameStarts.Days + " Days";
            }
            else
            {
                int months = (int)timeSinceGameStarts.TotalDays / 30;
                int days = months * 30 - (int)timeSinceGameStarts.TotalDays;
                if (months < 2)
                {
                    timeElapsed.text = months + "Month & " + days + " Days";
                }
                else
                {
                    timeElapsed.text = months + "Months & " + days + " Days";
                }
            }
        }
        tradeCompleted.text = tradeCompleted_server;
        collusionExecuted.text = collusionExecuted_server;
        averageTeamSize.text = averageTeamSize_server;
        dollarVolume.text = dollarVolume_server;
    }

    void UpdatePlayerTitleData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = UserAccountManager.Instance.PlayFabID
        }, response =>
        {
            UserAccountManager.Instance.IsTeamMember = bool.Parse(response.Data["isTeamMember"].Value);
            UserAccountManager.Instance.GroupID = response.Data["GroupID"].Value;
            UserAccountManager.Instance.GroupEntity.Id = response.Data["GroupID"].Value;
            UserAccountManager.Instance.GroupEntity.Type = "group";

            UserAccountManager.Instance.ProfilePhotoIndex = int.Parse(response.Data["ProfilePhoto"].Value);
            if (UserAccountManager.Instance.IsTeamMember)
            {
                makeTeamButton.gameObject.SetActive(false);
                seeAllTeams.gameObject.SetActive(true);
                leaveTeam.gameObject.SetActive(true);
                GetGroupDetail();
            }
        }, error => { Debug.Log("Player User Data not found"); });
    }

    public Root teamMemberInventoryDetails;
    /// <summary>
    /// Get all the inventory data of team member to merge wallets
    /// </summary>
    void GetTeamMemberInventory()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetInventory",
            FunctionParameter = new { ID = UserAccountManager.Instance.PlayFabID },
        },
    result =>
    {
        teamMemberInventoryDetails = JsonUtility.FromJson<Root>(result.FunctionResult.ToString());

    },
    error =>
    {
        Debug.Log("Error");
    });
    }

    public void OpenPanel(int index)
    {
        panels[index].SetActive(true);
    }

    public void ClosePanel(int index)
    {
        panels[index].SetActive(false);
    }

    public void OnTeamNameInput(string _teamName)
    {
        teamName = _teamName;
    }

    [Space(5)]
    [Header("Team")]
    public Button makeTeamButton;
    public Image makeTeamButtonLoading;
    private string teamName;
    public GroupDetail groupDetailPrefab;
    public Transform groupDetailParent;

    public GroupDetail[] groupDetails;
    public TeamMemberDetails teamMemberPrefab;
    public TeamMemberDetails[] team;
    public Transform teamDetailParent;
    public Button seeAllTeams, leaveTeam;

    public void MakeTeam()
    {
        if (teamName != "")
        {
            makeTeamButton.interactable = false;
            makeTeamButtonLoading.gameObject.SetActive(true);
            UserAccountManager.OnGroupCreationResult.AddListener(OnGroupCreatedResult);
            PlayFab.ClientModels.EntityKey entity = UserAccountManager.Instance.EntityToken.Entity;
            PlayFab.GroupsModels.EntityKey Entity = new PlayFab.GroupsModels.EntityKey { Id = entity.Id, Type = entity.Type };
            guildController.CreateGroup(teamName, Entity);
        }
        else
        {
            NotificationManager.Instance.CreateNotification("Team Name Can't be Empty", NotificationType.Error);
        }
    }
    public void GetGroupDetail()
    {
        UserAccountManager.OnListGroupMembersResult.AddListener(OnListGroupMembers);
        guildController.ListGroupsMembers(GuildController.EntityKeyMaker(UserAccountManager.Instance.GroupID));
    }
    List<EntityMemberRole> entities;
    private void OnListGroupMembers(string result)
    {
        if (result == "Success")
        {
            entities = (UserAccountManager.Instance.ListGroupMembers.Members);

            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = 0; j < entities[i].Members.Count; j++)
                {
                    //Debug.Log("ID: " + entities[i].Members[j].Key.Id + " Type: " + entities[i].Members[j].Key.Type);
                    PlayFab.AdminModels.GetPlayerProfileRequest request = new PlayFab.AdminModels.GetPlayerProfileRequest();
                    request.PlayFabId = entities[i].Members[j].Key.Id;


                    PlayFab.ProfilesModels.GetEntityProfileRequest req = new PlayFab.ProfilesModels.GetEntityProfileRequest();
                    req.Entity = new PlayFab.ProfilesModels.EntityKey { Id = entities[i].Members[j].Key.Id, Type = "title_player_account" };
                    req.DataAsObject = true;
                    PlayFabProfilesAPI.GetProfile(req, response =>
                    {
                        if (response.Profile.Entity.Id != PlayerDashboard.defaultPlayerTitleID[0])
                        {
                            TeamMemberDetails teamMember = Instantiate(teamMemberPrefab, teamDetailParent);
                            teamMember.SetData(response);
                        }
                    }, error =>
                    {
                        Debug.Log("Error " + error.Error);
                    }
                    );
                }
            }
        }
        else if (result == "Failed")
        {

        }
        UserAccountManager.OnListGroupMembersResult.RemoveListener(OnListGroupMembers);

    }

    void OnGroupCreatedResult(string result)
    {
        if (result == "Success")
        {
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string>()
            {
                { "isTeamMember", "true" },
                { "GroupID", UserAccountManager.Instance.GroupEntity.Id },
            }
            },
                result =>
                {
                    NotificationManager.Instance.CreateNotification("You are now owner of " + UserAccountManager.Instance.GroupName, NotificationType.Normal);
                    Debug.Log("Successfully updated user data");
                    PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                    {
                        FunctionName = "addMembers",
                        FunctionParameter = new { GroupId = UserAccountManager.Instance.GroupEntity.Id, MemberIDs = defaultPlayer },
                        GeneratePlayStreamEvent = true
                    },
            response =>
            {
                for (int i = 0; i < response.Logs.Count; i++)
                {
                    Debug.Log(response.Logs[i].Message);
                }

                //                Debug.Log("result " + response.FunctionResult.ToString());
                if (response.FunctionResult.ToString() == "True")
                {
                    Debug.Log("DefaultPlayerAdded");
                }
                else
                {
                    Debug.Log("DefaultPlayer not Added ");
                }
            },
            error =>
            {
                Debug.Log("Error: " + error.Error + " DefaultPlayer not Added");
            }); ;
                },
                error =>
                {
                    NotificationManager.Instance.CreateNotification("Sorry, Team Data Can't be updated, Error: " + error.Error, NotificationType.Normal);
                    Debug.Log(error.GenerateErrorReport());
                });

        }
        else if (result == "Failed")
        {
            NotificationManager.Instance.CreateNotification("Team can't be created, Try Again", NotificationType.Normal);
        }
        makeTeamButton.interactable = true;
        makeTeamButtonLoading.gameObject.SetActive(false);

        GetGroupDetail();

        UserAccountManager.OnGroupCreationResult.RemoveListener(OnGroupCreatedResult);
    }
    public void OpenJoinGroup()
    {
        ListAllGroups();
    }

    public void ListAllGroups()
    {
        UserAccountManager.OnListGroupResult.AddListener(SetupGroupData);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "ListTitleMembership",
            FunctionParameter = new { playFabId = defaultPlayerTitleID[0] },
        },
    result =>
    {
        //Debug.Log(result.FunctionResult.ToString());
        string s = result.FunctionResult.ToString();
        string substring = s.Substring(10);
//        Debug.Log(substring);
        substring = substring.Substring(0, substring.Length - 1);
        //Debug.Log(substring);

        UserAccountManager.Instance.AllGroups = JsonConvert.DeserializeObject<List<GroupWithRoles>>(substring);
        UserAccountManager.OnListGroupResult.Invoke("Success");
    },
    error =>
    {
        Debug.Log("Error");
    });


    }
    public void CloseJoinGroup()
    {
        for (int i = 0; i < AllGroupDetails.Count; i++)
        {
            Destroy(AllGroupDetails[i].gameObject);
        }

    }
    List<GroupDetail> AllGroupDetails;
    void SetupGroupData(string result)
    {
        if (result == "Success")
        {
            GetGroupRequest getGroupRequest = new GetGroupRequest();
            AllGroupDetails = new List<GroupDetail>();
            for (int i = 0; i < UserAccountManager.Instance.AllGroups.Count; i++)
            {
                GroupDetail group = Instantiate(groupDetailPrefab, groupDetailParent);
                AllGroupDetails.Add(group);
                getGroupRequest.Group = UserAccountManager.Instance.AllGroups[i].Group;
                PlayFabGroupsAPI.GetGroup(getGroupRequest, respose =>
                {
                    group.groupData = respose;
                    group.SetData();
                }, error =>
                {
                    Debug.Log("Error :" + error.Error);
                });
                //ListGroupMembersRequest request=  new ListGroupMembersRequest();
                //request.Group = UserAccountManager.Instance.AllGroups[i].Group;
                //PlayFabGroupsAPI.ListGroupMembers(request, respose =>
                //{
                //    group.groupMembers = respose;
                //    group.SetData();
                //}, error =>
                //{
                //    Debug.Log("Error :" + error.Error);
                //});
            }
        }
        else if (result == "Failed")
        {
            Debug.Log("Failed to list groups");

        }
        UserAccountManager.OnListGroupResult.RemoveListener(SetupGroupData);
    }

    public void SearchTeams()
    {
        PlayFab.GroupsModels.EntityKey entityKey = GuildController.EntityKeyMaker(UserAccountManager.Instance.PlayFabID);
        guildController.ListGroups(entityKey);
    }

    public void JoinTeam(string _teamName)
    {
        UserAccountManager.OnGroupJoinResult.AddListener(OnGroupJoinedResult);
        PlayFab.ClientModels.EntityKey entity = UserAccountManager.Instance.EntityToken.Entity;
        PlayFab.GroupsModels.EntityKey Entity = new PlayFab.GroupsModels.EntityKey { Id = entity.Id, Type = entity.Type };
        guildController.ApplyToGroup(_teamName, Entity);
    }

    void OnGroupJoinedResult(string result)
    {
        if (result == "Success")
        {
            NotificationManager.Instance.CreateNotification("Group Joining Requested", NotificationType.Normal);
        }
        else if (result == "Failed")
        {
            NotificationManager.Instance.CreateNotification("Group Request Failed", NotificationType.Error);
        }
        UserAccountManager.OnGroupJoinResult.RemoveListener(OnGroupJoinedResult);
    }

}
