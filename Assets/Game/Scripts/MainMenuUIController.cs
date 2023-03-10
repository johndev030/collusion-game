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
    public Text userName;
    public Image profilePhoto;
    public Sprite[] profilePhotos;
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
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupProfile();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void SetupProfile()
    {
        userName.text = UserAccountManager.Instance.UserName;
        PlayFabClientAPI.GetPlayerProfile(
            new GetPlayerProfileRequest
            {
                
            },response =>
            {
                Debug.Log($"Successfully account created: ,{userName}");

            },
            error =>
            {
                Debug.Log($"Error: ,{error.Error},{error.ErrorMessage},{error.ErrorDetails}");
            }

        );
    }

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
    }
}
