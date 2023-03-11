using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;



public class ProfileEditing : MonoBehaviour
{
    public Button[] profileImageButton;
    public Sprite[] profileImageSprites;
    public InputField inputFieldName;
    private int profilePhotoIndex;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < profileImageButton.Length; i++)
        {
            int tempIndex = i;
            profileImageButton[i].onClick.AddListener(() =>
            {
                SelectPhoto(tempIndex);
            });
        }
    }

    private void SelectPhoto(int index)
    {
        profilePhotoIndex = index;
        //profileImageButton[profilePhotoIndex].gameObject.
        Debug.Log("profilePhotoIndex : " + profilePhotoIndex);
    }

    private void OnEnable()
    {
        Setup();
    }
    public void SelectProfilePhoto(int index)
    {
        profilePhotoIndex = index;
    }
    void Setup()
    {
        for (int i = 0; i < profileImageButton.Length; i++)
        {
            profileImageButton[i].image.overrideSprite = profileImageSprites[i];
        }
        GetUserData();
        GetPlayerProfile(UserAccountManager.Instance.PlayFabID);
    }
    public void SetUserData()
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {


            { "ProfilePhoto", profilePhotoIndex.ToString() },
        }
        },
        result =>
        {
            Debug.Log("Successfully updated user data");

            for (int i = 0; i < MainMenuUIController.Instance.profilePhoto.Length; i++)
            {
                MainMenuUIController.Instance.profilePhoto[i].overrideSprite = profileImageSprites[profilePhotoIndex];
            }
        },
        error =>
        {
            Debug.Log("Profile photo not updated");
            Debug.Log(error.GenerateErrorReport());
        });
    }
    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = UserAccountManager.Instance.PlayFabID, //THIS LİNE ISN'T WORKİNG//
            Keys = null
        }, result =>
        {
            //Debug.Log("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("ProfilePhoto")) Debug.Log("No such key is present");
            else
            {
                profilePhotoIndex = int.Parse(result.Data["ProfilePhoto"].Value);
                for (int i = 0; i < MainMenuUIController.Instance.profilePhoto.Length; i++)
                {
                    MainMenuUIController.Instance.profilePhoto[i].overrideSprite = profileImageSprites[int.Parse(result.Data["ProfilePhoto"].Value)];
                }
                //Debug.Log("profile: " + result.Data["ProfilePhoto"].Value);
            }
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }
    public void UpdateDisplayName()
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = inputFieldName.text
        }, result =>
        {
            Debug.Log("The player's display name is now: " + result.DisplayName);
            MainMenuUIController.Instance.nameText.text = result.DisplayName;

        }, error => Debug.LogError(error.GenerateErrorReport())); ;
    }
    void GetPlayerProfile(string playFabId)
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true
            }
        },
        result =>
        {
            Debug.Log("The player's DisplayName profile data is: " + result.PlayerProfile.DisplayName);
            MainMenuUIController.Instance.nameText.text = result.PlayerProfile.DisplayName;
            inputFieldName.text = result.PlayerProfile.DisplayName;
        },
        error => Debug.LogError(error.GenerateErrorReport()));
    }
}
