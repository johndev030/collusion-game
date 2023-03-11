using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class MainMenuUIController : MonoBehaviour
{

    [Header("Profile")]
    [Space(5)]
    public Text userNameText;
    public Text nameText;
    public Image[] profilePhoto;
    public GameObject profileEditingPanel;
    [Space(10)]
    [Header("PlayerDashboard")]
    public Text timeElapsed;
    public static MainMenuUIController Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        if(Constants.PROFILE_SETUP == "False")
        {
            profileEditingPanel.SetActive(true);
            Constants.PROFILE_SETUP = "True";
        }
        else
        {
            profileEditingPanel.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupProfile();
    }

    void SetupProfile()
    {
        userNameText.text = "@"+UserAccountManager.Instance.UserName;
    }

    /*
    void UpdateDisplayName(string displayName)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        }, result => {
            Debug.Log("The player's display name is now: " + result.DisplayName);
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
        result => Debug.Log("The player's DisplayName profile data is: " + result.PlayerProfile.DisplayName),
        error => Debug.LogError(error.GenerateErrorReport()));
    }*/
}
