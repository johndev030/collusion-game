using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.GroupsModels;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using Guild;
using Newtonsoft.Json;

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


        GetPlayerProfileRequest request = new GetPlayerProfileRequest();
        request.PlayFabId = entity.Id;
        PlayFabClientAPI.GetPlayerProfile(request, response =>
        {
            displayName.text = response.PlayerProfile.DisplayName;
            Debug.Log("Name " + response.PlayerProfile.DisplayName);
        }, error => {
            Debug.Log("GetPlayerProfileRequest error " +error.Error);
        });

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "getPlayerProfile",
            FunctionParameter = new { playFabId = entity.Id },
        },
        response => {
            //GetPlayerProfileResult playerProfile;
            Debug.Log(response.FunctionResult.ToString());
            //GetPlayerProfileResult playerProfile = JsonConvert.DeserializeObject<GetPlayerProfileResult>(response.FunctionResult.ToString());
            //displayName.text = playerProfile.PlayerProfile.DisplayName;
            //Debug.Log("Name " + playerProfile.PlayerProfile.DisplayName);
        },
        error=>{
            Debug.Log("GetPlayerProfileRequest error " + error.Error);

        });

        GetUserDataRequest request1 =  new GetUserDataRequest();
        request1.PlayFabId = entity.Id;
        PlayFabClientAPI.GetUserData(request1, response =>
        {
            Debug.Log(response.Data);
            if (response.Data != null)
            {
                profilePhoto.sprite = MainMenuUIController.Instance.profileImageSprites[int.Parse(response.Data["ProfilePhoto"].Value)];
            }
        }, error => {
            Debug.Log("ProfileDate could not be loaded");
        });

        expiryDate.text = groupApplication.Expires.ToShortDateString();
    }

    // Update is called once per frame
    public void AcceptApplication()
    {
        var request = new AcceptGroupApplicationRequest { Group = UserAccountManager.Instance.GroupEntity, Entity = groupApplication.Entity.Key };
        PlayFabGroupsAPI.AcceptGroupApplication(request, response=> {
            NotificationManager.Instance.CreateNotification(displayName.text + " is added to your team", NotificationType.Normal);
            Destroy(this.gameObject);
        }, error=> {
            NotificationManager.Instance.CreateNotification("Sorry, Could not add this member in your team", NotificationType.Error);
            Debug.Log("could not add this member in your team");
        });
    }


    public void RejectApplication()
    {
        var request = new RemoveGroupApplicationRequest { Group = UserAccountManager.Instance.GroupEntity, Entity = groupApplication.Entity.Key };
        PlayFabGroupsAPI.RemoveGroupApplication(request, response => {
            NotificationManager.Instance.CreateNotification(displayName.text + " is removed", NotificationType.Normal);
            Destroy(this.gameObject);
        }, error => {
            NotificationManager.Instance.CreateNotification("Sorry, Could not remove", NotificationType.Error);
            Debug.Log("could not remove this member application");
        });
    }
}
