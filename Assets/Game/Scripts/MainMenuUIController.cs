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
    public Text[] moneyTexts;
    public Text[] uniqueTokensTexts;
    public GameObject profileEditingPanel;
    public Sprite[] profileImageSprites;


    public GameObject[] panels;
    public Button[] panelButton;
    public Transform popUpsParent;

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

        for (int i = 0; i < moneyTexts.Length; i++)
        {
            moneyTexts[i].text = "$"+UserAccountManager.Instance.MONEY.ToString();
        }
        //for (int i = 0; i < uniqueTokensTexts.Length; i++)
        //{
        //    uniqueTokensTexts[i].text = UserAccountManager.Instance.uniqueTokens.Count.ToString()+"/100";
        //}
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

    public void OpenPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if(i == index)
            {
                panels[i].SetActive(true);
                panelButton[i].image.color = Color.white;
            }
            else
            {
                panelButton[i].image.color = Color.grey/2;
                panels[i].SetActive(false);
            }
        }
    }

}
