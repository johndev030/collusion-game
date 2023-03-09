using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.Events;

public class UserAccountManager : MonoBehaviour
{
    public static UserAccountManager Instance;
    public static UnityEvent<string> OnLoginSuccess = new UnityEvent<string>();
    public static UnityEvent<string> OnLoginFailed = new UnityEvent<string>();
    public static UnityEvent<string> OnRegistrationSuccessfull = new UnityEvent<string>();
    public static UnityEvent<string> OnRegistrationFailed = new UnityEvent<string>();


    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

     public void CreateUser(string userName, string email, string password)

    {
        PlayFabClientAPI.RegisterPlayFabUser(
            new RegisterPlayFabUserRequest()
            {
                Email = email,
                Password = password,
                Username = userName,
                RequireBothUsernameAndEmail = true
                
            },
            response =>
            {
                Debug.Log($"Successfully account created: ,{userName},{email}");
                OnRegistrationSuccessfull.Invoke("Account Registered Successfully");
            },
            error =>
            {
                Debug.Log($"Error: ,{error.Error},{error.ErrorMessage},{error.ErrorDetails}");
                OnRegistrationFailed.Invoke("Account Registeration Failed/n"+ error.Error);
            }
            
            );   

    }
    public void Login(string userName, string password)

    {
        PlayFabClientAPI.LoginWithPlayFab(
            new LoginWithPlayFabRequest()
            {
                Password = password,
                Username = userName,
            },
            response =>
            {
                Debug.Log($"Successfully account created: ,{userName}");
                OnLoginSuccess.Invoke("Account Login Successful");

            },
            error =>
            {
                Debug.Log($"Error: ,{error.Error},{error.ErrorMessage},{error.ErrorDetails}");
                OnLoginFailed.Invoke("Account Login Failed/n" + error.Error);
            }

            );

    }

}
