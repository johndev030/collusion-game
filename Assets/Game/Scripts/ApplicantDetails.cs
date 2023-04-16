using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.GroupsModels;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using Guild;
using Newtonsoft.Json;
using PlayFab.AdminModels;

public class ApplicantDetails : MonoBehaviour
{
    public GroupApplication groupApplication;
    public Text displayName, expiryDate;
    public Image profilePhoto;
    GuildController guildController = new GuildController();
    [SerializeField]
    public EntityWithLineage entityWithLineage;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetDatails()
    {
        PlayFab.GroupsModels.EntityKey entity = new PlayFab.GroupsModels.EntityKey();
        foreach (var values in groupApplication.Entity.Lineage.Values)
        {
            entity = values;
            Debug.Log("item:" + values.Id);
            Debug.Log("item:" + values.Type);
        }


        PlayFab.ClientModels.GetPlayerProfileRequest request = new PlayFab.ClientModels.GetPlayerProfileRequest();
        request.PlayFabId = entity.Id;
        PlayFabClientAPI.GetPlayerProfile(request, response =>
        {
            displayName.text = response.PlayerProfile.DisplayName;
            Debug.Log("Name " + response.PlayerProfile.DisplayName);
        }, error =>
        {
            Debug.Log("GetPlayerProfileRequest error " + error.Error);
        });

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "getPlayerProfile",
            FunctionParameter = new { playFabId = entity.Id },
        },
        response =>
        {
            //GetPlayerProfileResult playerProfile;
            Debug.Log(response.FunctionResult.ToString());
            //GetPlayerProfileResult playerProfile = JsonConvert.DeserializeObject<GetPlayerProfileResult>(response.FunctionResult.ToString());
            //displayName.text = playerProfile.PlayerProfile.DisplayName;
            //Debug.Log("Name " + playerProfile.PlayerProfile.DisplayName);
        },
        error =>
        {
            Debug.Log("GetPlayerProfileRequest error " + error.Error);

        });

        PlayFab.ClientModels.GetUserDataRequest request1 = new PlayFab.ClientModels.GetUserDataRequest();
        request1.PlayFabId = entity.Id;
        PlayFabClientAPI.GetUserData(request1, response =>
        {
//            Debug.Log(response.Data);
            if (response.Data != null)
            {
                profilePhoto.sprite = MainMenuUIController.Instance.profileImageSprites[int.Parse(response.Data["ProfilePhoto"].Value)];
            }
        }, error =>
        {
            Debug.Log("ProfileDate could not be loaded");
        });

        expiryDate.text = groupApplication.Expires.ToShortDateString();
    }

    // Update is called once per frame
    public void AcceptApplication()
    {
        Dictionary<string, string> Data = new Dictionary<string, string> {
               { "isTeamMember", "true" },
               { "GroupID",UserAccountManager.Instance.GroupEntity.Id }
            };
        Debug.Log("Key" + groupApplication.Entity.Key.Id);


        PlayerData playerData = new PlayerData();

        playerData.isTeamMember = true;
        playerData.GroupID = UserAccountManager.Instance.GroupEntity.Id;

        PlayFab.DataModels.SetObject setObject = new PlayFab.DataModels.SetObject();
        setObject.ObjectName = "PlayerData";
        setObject.DataObject = playerData;
        List<PlayFab.DataModels.SetObject> objects = new List<PlayFab.DataModels.SetObject>();
        objects.Add(setObject);

        PlayFabDataAPI.SetObjects(new PlayFab.DataModels.SetObjectsRequest
        {
            Entity = UserAccountManager.EntityKeyMaker(groupApplication.Entity.Key.Id, "title_player_account"),
            Objects = objects
        },
        response => {
            Debug.Log("Successfully Player Data of Applicant updated");
            //resultCallback.Invoke("Success");
        },
        error => {
            Debug.Log(error.GenerateErrorReport());
            //errorCallback.Invoke("Failed");
        }
        );

        //JsonUtility.ToJson(Data);
        //PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        //{
        //    FunctionName = "updateUserData",
        //    FunctionParameter = new { playFabId = groupApplication.Entity.Key.Id, data = Data },
        //},
        //response =>
        //{
        //    for (int i = 0; i < response.Logs.Count; i++)
        //    {
        //        Debug.Log(response.Logs[i].Message);
        //    }
        //    Debug.Log(response.Error.Message);
        //    Debug.Log(response.Error.Error);
        //    Debug.Log(response.Error.StackTrace);
        //    Debug.Log(response.FunctionResult.ToString());
        //},
        //error =>
        //{

        //});

        //PlayFab.AdminModels.UpdateUserDataRequest updateUserDataRequest = new PlayFab.AdminModels.UpdateUserDataRequest();
        //updateUserDataRequest.PlayFabId = groupApplication.Entity.Key.Id;
        //updateUserDataRequest.Data = Data;
        //updateUserDataRequest.Permission = PlayFab.AdminModels.UserDataPermission.Public;

        //PlayFabAdminAPI.UpdateUserData(new PlayFab.AdminModels.UpdateUserDataRequest
        //{
        //    Data = new Dictionary<string, string>() {
        //        { "isTeamMember", "true" },
        //        { "GroupID", UserAccountManager.Instance.GroupEntity.Id }
        //        },
        //    Permission = PlayFab.AdminModels.UserDataPermission.Public,
        //    PlayFabId = groupApplication.Entity.Key.Id
        //},
        //response =>
        //{
        //    Debug.Log("Updated");
        //},
        //error =>
        //{
        //    Debug.Log(error.GenerateErrorReport());
        //    //Debug.Log("Error" + error.Error);
        //    //Debug.Log("Error" + error.ErrorMessage);
        //    //Debug.Log("Error" + error.ErrorDetails);

        //});

        var request = new AcceptGroupApplicationRequest { Group = UserAccountManager.Instance.GroupEntity, Entity = groupApplication.Entity.Key };
        PlayFabGroupsAPI.AcceptGroupApplication(request, response =>
        {
            NotificationManager.Instance.CreateNotification(displayName.text + " is added to your team", NotificationType.Normal);

            PlayerDashboard.Instance.GetGroupDetail();

            Destroy(this.gameObject);
        }, error =>
        {
            NotificationManager.Instance.CreateNotification("Sorry, Could not add this member in your team", NotificationType.Error);
            Debug.Log("could not add this member in your team");
        });
    }


    public void RejectApplication()
    {
        var request = new RemoveGroupApplicationRequest { Group = UserAccountManager.Instance.GroupEntity, Entity = groupApplication.Entity.Key };
        PlayFabGroupsAPI.RemoveGroupApplication(request, response =>
        {
            NotificationManager.Instance.CreateNotification(displayName.text + " is removed", NotificationType.Normal);
            Destroy(this.gameObject);
        }, error =>
        {
            NotificationManager.Instance.CreateNotification("Sorry, Could not remove", NotificationType.Error);
            Debug.Log("could not remove this member application");
        });
    }


}
