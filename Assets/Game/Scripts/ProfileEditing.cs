using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;



public class ProfileEditing : MonoBehaviour
{
    public GameObject ProfileEditingPanel;
    public Button[] profileImageButton;
    public InputField inputFieldName;
    private int profilePhotoIndex;
    Sprite[] profileImageSprites;

    [Space(10)]
    [Header("Questionier Data")]
    public GameObject QuestionierPanel;
    public Dropdown inputFieldTeamSize, inputFieldTeamSystem, inputFieldResponsiveness, inputFieldTeamMemberType;
    public Button saveQuestionerButton;


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
        profileImageSprites = MainMenuUIController.Instance.profileImageSprites;
        Setup();
    }

    private void SelectPhoto(int index)
    {
        profilePhotoIndex = index;
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
        for (int i = 0; i < MainMenuUIController.Instance.profilePhoto.Length; i++)
        {
            MainMenuUIController.Instance.profilePhoto[i].overrideSprite = profileImageSprites[UserAccountManager.Instance.ProfilePhotoIndex];
        }
        GetPlayerProfile(UserAccountManager.Instance.PlayFabID);
    }
    public void SetProfilePhoto()
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
            PlayFabId = UserAccountManager.Instance.PlayFabID, 
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

            if (Constants.QUESTIONIER_ASKED == "False")
            {
                ProfileEditingPanel.SetActive(false);
                QuestionierPanel.SetActive(true);
            }

        }, error => Debug.LogError(error.GenerateErrorReport()));
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
//          Debug.Log("The player's DisplayName profile data is: " + result.PlayerProfile.DisplayName);
            MainMenuUIController.Instance.nameText.text = result.PlayerProfile.DisplayName;
            UserAccountManager.Instance.DisplayName = result.PlayerProfile.DisplayName;
            inputFieldName.text = result.PlayerProfile.DisplayName;
        },
        error => Debug.LogError(error.GenerateErrorReport()));
    }

    public void SaveQuestionier()
    {
        saveQuestionerButton.interactable = false;
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
            { "TeamSize", inputFieldTeamSize.options[inputFieldTeamSize.value].text.ToString() },
            { "TeamSystem", inputFieldTeamSystem.options[inputFieldTeamSystem.value].text.ToString() },
            { "Responsiveness", inputFieldResponsiveness.options[inputFieldResponsiveness.value].text.ToString() },
            { "MemberType", inputFieldTeamMemberType.options[inputFieldTeamMemberType.value].text.ToString() }
            }

        },
        result =>
        {
            Debug.Log("Successfully updated Questionier data");
            saveQuestionerButton.interactable = true;
            Constants.QUESTIONIER_ASKED = "True";
        },
        error =>
        {
            Debug.Log("Questionier not updated");
            Debug.Log(error.GenerateErrorReport());
            saveQuestionerButton.interactable = true;

        });
    }
}
