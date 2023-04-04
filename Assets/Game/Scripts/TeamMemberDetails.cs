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
    // Start is called before the first frame update
    void Start()
    {

    }

    internal void SetData(PlayFab.ProfilesModels.GetEntityProfileResponse respone)
    {
        entityProfile = respone;
        displayName.text = respone.Profile.DisplayName;
     
        GetPlayerProfileRequest request2 = new GetPlayerProfileRequest();
        request2.PlayFabId = entityProfile.Profile.Lineage.MasterPlayerAccountId;
        PlayFabClientAPI.GetPlayerProfile(request2, respone =>
        {
            displayName.text = respone.PlayerProfile.DisplayName;
            //Debug.Log("G Haji sb: " + respone.PlayerProfile.DisplayName);
        }, error =>
        {
            //Debug.Log("Ni Haji sb: " + error.Error);
        });

        GetUserDataRequest request = new GetUserDataRequest();
        request.PlayFabId = entityProfile.Profile.Lineage.MasterPlayerAccountId;
        PlayFabClientAPI.GetUserData(request, respone =>
        {
            profilePhotoIndex = int.Parse(respone.Data["ProfilePhoto"].Value);

            profilePhoto.sprite = MainMenuUIController.Instance.profileImageSprites[profilePhotoIndex];

        }, error =>
        {
            Debug.Log("Error: " + error.Error);
        });

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetCurrencyBalance",
            FunctionParameter = new { ID = entityProfile.Profile.Lineage.MasterPlayerAccountId },
        },
     result =>
     {
         currency = JsonConvert.DeserializeObject<VirtualCurrency>(result.FunctionResult.ToString());
         Debug.Log(respone.Profile.DisplayName + " " + currency.CL);
     },
     error =>
     {
         Debug.Log("Error");
     });
    }
}
