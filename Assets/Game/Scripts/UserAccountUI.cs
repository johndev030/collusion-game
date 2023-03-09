using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UserAccountUI : MonoBehaviour
{
    public Text msg;
    public GameObject loadingScreen;

    string email_register, password_register, userName_register;

    private void OnEnable()
    {
        UserAccountManager.OnLoginSuccess.AddListener(DisplayMessage);
        UserAccountManager.OnLoginFailed.AddListener(DisplayMessage);
        UserAccountManager.OnRegistrationSuccessfull.AddListener(DisplayMessage);
        UserAccountManager.OnRegistrationFailed.AddListener(DisplayMessage);
    }
    private void OnDisable()
    {
        UserAccountManager.OnLoginSuccess.RemoveListener(DisplayMessage);
        UserAccountManager.OnLoginFailed.RemoveListener(DisplayMessage);
        UserAccountManager.OnRegistrationSuccessfull.RemoveListener(DisplayMessage);
        UserAccountManager.OnRegistrationFailed.RemoveListener(DisplayMessage);
    }

    void DisplayMessage(string message)
    {
        loadingScreen.SetActive(false);
        msg.text = message;
        msg.gameObject.SetActive(true);
    }
    public void DisableDisplayMessage()
    {
        msg.gameObject.SetActive(false);
    }

    public void UpdateUserName(string _userName)
    {
        userName_register = _userName;
    }

    public void UpdateEmail(string _email)
    {
        email_register = _email;
    }

    public void UpdatePassword(string _password)
    {
        password_register = _password;
    }

    public void Register()
    {
        loadingScreen.SetActive(true);
        UserAccountManager.Instance.CreateUser(userName_register, email_register, password_register);
    }

    string password_login, userName_login;

    public void UpdateUserNameLogin(string _userName)
    {
        userName_login = _userName;
    }

    public void UpdatePasswordLogin(string _password)
    {
        password_login = _password;
    }

    public void Login()
    {
        loadingScreen.SetActive(true);
        UserAccountManager.Instance.Login(userName_login, password_login);
    }
}
