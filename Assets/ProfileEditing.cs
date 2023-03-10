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
    }
    public void SetUserData()
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {


            { "ProfilePhoto", profilePhotoIndex.ToString() },
        }
        },
        result => { Debug.Log("Successfully updated user data");
                MainMenuUIController.Instance.profilePhoto.overrideSprite = profileImageSprites[profilePhotoIndex];
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
            Debug.Log("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("ProfilePhoto")) Debug.Log("No such key is present");
            else
            {
                profilePhotoIndex = int.Parse(result.Data["ProfilePhoto"].Value);
                MainMenuUIController.Instance.profilePhoto.overrideSprite = profileImageSprites[int.Parse(result.Data["ProfilePhoto"].Value)];
                Debug.Log("profile: " + result.Data["ProfilePhoto"].Value);
            }
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }
    void UploadProfilePhoto(int index)
    {
        //PlayFabClientAPI.up
    }
}
