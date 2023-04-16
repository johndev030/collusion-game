using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using PlayFab.AdminModels;
using PlayFab.ClientModels;

using PlayFab;
using Newtonsoft.Json;

public class TeamMemberDetails : MonoBehaviour
{
    public Text displayName;
    public Image profilePhoto;
    public int profilePhotoIndex;
    [SerializeField]
    public PlayFab.ProfilesModels.GetEntityProfileResponse entityProfile;
    [SerializeField]
    public GetPlayerProfileResult profile;
    public VirtualCurrency currency;
    public GetUserInventoryResult inventory;

    public ProfileData profileDataPrefab;
    internal TeamDetails teamDetails;
    // Start is called before the first frame update
    public void OpenUserProfile()
    {
        if (profile != null && currency!= null && inventory != null)
        {
            ProfileData profileData = Instantiate(profileDataPrefab, MainMenuUIController.Instance.popUpsParent);
            profileData.SetProfileData(profile, currency, inventory, profilePhotoIndex);
        }
        else {
            NotificationManager.Instance.CreateNotification("Please wait, Data fetching is in progress", NotificationType.Error);
        }
    }

    internal void SetData(PlayFab.ProfilesModels.GetEntityProfileResponse respone)
    {
        entityProfile = respone;
        displayName.text = respone.Profile.DisplayName;

        GetPlayerProfileRequest request2 = new GetPlayerProfileRequest();
        request2.PlayFabId = entityProfile.Profile.Lineage.MasterPlayerAccountId;
        PlayFabClientAPI.GetPlayerProfile(request2, respone =>
        {
            profile = respone;
            displayName.text = respone.PlayerProfile.DisplayName;
            GetTeamMemberInventory(profile.PlayerProfile.PlayerId);

            //Debug.Log("G Haji sb: " + respone.PlayerProfile.DisplayName);
        }, error =>
        {
            //Debug.Log("Ni Haji sb: " + error.Error);
        });

 

        UserAccountManager.Instance.GetPlayerData(entityProfile.Profile.Lineage.TitlePlayerAccountId, response => {
            PlayerData playerData = response;
            profilePhotoIndex = playerData.ProfilePhoto;
            profilePhoto.sprite = MainMenuUIController.Instance.profileImageSprites[profilePhotoIndex];
        }, error => { });

        //GetUserDataRequest request = new GetUserDataRequest();
        //request.PlayFabId = entityProfile.Profile.Lineage.MasterPlayerAccountId;
        //PlayFabClientAPI.GetUserData(request, respone =>
        //{
        //    //            Debug.Log(request.PlayFabId);

        //    profilePhotoIndex = int.Parse(respone.Data["ProfilePhoto"].Value);
        //    profilePhoto.sprite = MainMenuUIController.Instance.profileImageSprites[profilePhotoIndex];

        //}, error =>
        //{
        //    Debug.Log(request.PlayFabId + " Error: " + error.Error);
        //});

        UpdateCurrency();
    }

    void UpdateCurrency()
    {
//        Debug.Log("UpdateCurrency");

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetCurrencyBalance",
            FunctionParameter = new { ID = entityProfile.Profile.Lineage.MasterPlayerAccountId },
        },
        result =>
        {

            currency = JsonConvert.DeserializeObject<VirtualCurrency>(result.FunctionResult.ToString());
           // Debug.Log("Currency Fetched");

            if (teamDetails)
            {
               // Debug.Log("Adding Currency");

                teamDetails.TeamTotalCash += currency.CL;
                teamDetails.teamWallet.UpdateBalance();
            }
        },
        error =>
        {
            Debug.Log("Error");
        });
    }
    public static int inventoryFetched;
    void GetTeamMemberInventory(string playfabId)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetInventory",
            FunctionParameter = new { ID = playfabId },
        },
        result =>
        {
            inventory = JsonConvert.DeserializeObject<GetUserInventoryResult>(result.FunctionResult.ToString());
            if (teamDetails)
            {
                inventoryFetched++;
//                Debug.Log("inventoryFetched " + inventoryFetched);
            }

            if (teamDetails && inventoryFetched == teamDetails.teamMembersList.Count)
            {
                teamDetails.teamWallet.SetupTokens();
            }
        },
    error =>
    {
        Debug.Log("Error");
    });
    }
}
