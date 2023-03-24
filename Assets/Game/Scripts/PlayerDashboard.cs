using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Globalization;
using PlayFab.Json;
using Guild;
public class PlayerDashboard : MonoBehaviour
{
    public Text uniqueTokenCount; public Text[] moneyTexts;
    public Text timeElapsed, tradeCompleted, dollarVolume, collusionExecuted, averageTeamSize;
    [Space(5)]
    [Header("Panels")]
    public GameObject[] panels;
    private GuildController guildController;
    void Start()
    {
        guildController = new GuildController();

        for (int i = 0; i < moneyTexts.Length; i++)
        {
            moneyTexts[i].text = "$" + UserAccountManager.Instance.MONEY.ToString();
        }
        UpdateTokensCount();
        UpdateTitleData();
    }

    void UpdateTokensCount()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest()
        , result =>
        {
            UserAccountManager.Instance.TOKENCOUNT = result.Inventory.Count;
            uniqueTokenCount.text = UserAccountManager.Instance.TOKENCOUNT.ToString() + "/100";
            //for (int i = 0; i < result.Inventory.Count; i++)
            //{
            //    Debug.Log("You have " + result.Inventory[i].ItemId);
            //}
        },
        error =>
        {
            Debug.Log("Error Getting Tokens");
        });
    }
    string timeSinceGameStart_server, tradeCompleted_server, dollarVolume_server, collusionExecuted_server, averageTeamSize_server;
    void UpdateTitleData()
    {
        PlayFabServerAPI.GetTitleData(new PlayFab.ServerModels.GetTitleDataRequest(),
            result =>
            {
                if (result.Data == null)
                {
                    Debug.Log("No Tokens Data " + result.Data);
                }
                else
                {

                    timeSinceGameStart_server = result.Data["TimeSinceGameStart"];
                    averageTeamSize_server = result.Data["AverageTeamSize"];
                    collusionExecuted_server = result.Data["CollusionExecuted"];
                    dollarVolume_server = result.Data["DollarVolume"];
                    tradeCompleted_server = result.Data["TradeCompleted"];

                    SetTitleData();
                }
            },
            error =>
            {
                Debug.Log("Got error getting titleData:");
                Debug.Log(error.GenerateErrorReport());
            });
    }

    void SetTitleData()
    {
        if (timeSinceGameStart_server != "")
        {
            CultureInfo culture = new CultureInfo("en-US");
            DateTime startTime = Convert.ToDateTime(timeSinceGameStart_server, culture);
            TimeSpan timeSinceGameStarts = DateTime.Now - startTime;
            if (timeSinceGameStarts.Days < 1)
            {
                timeElapsed.text = timeSinceGameStarts.Hours + " Hours";
            }
            else if (timeSinceGameStarts.Days < 30)
            {
                timeElapsed.text = timeSinceGameStarts.Days + " Days";
            }
            else
            {
                int months = (int)timeSinceGameStarts.TotalDays / 30;
                int days = months * 30 - (int)timeSinceGameStarts.TotalDays;
                if (months < 2)
                {
                    timeElapsed.text = months + "Month & " + days + " Days";
                }
                else
                {
                    timeElapsed.text = months + "Months & " + days + " Days";
                }
            }
        }
        tradeCompleted.text = tradeCompleted_server;
        collusionExecuted.text = collusionExecuted_server;
        averageTeamSize.text = averageTeamSize_server;
        dollarVolume.text = dollarVolume_server;

        GetTeamMemberInventory();
    }
    public Root teamMemberInventoryDetails;
    void GetTeamMemberInventory()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GetOthersInventory",
            FunctionParameter = new { ID = "6FE73911052ECAA9" },
        },
    result =>
    {
        teamMemberInventoryDetails = JsonUtility.FromJson<Root>(result.FunctionResult.ToString());
        //Debug.Log("Result: " + result.FunctionResult);
        //Debug.Log(PlayFabSimpleJson.SerializeObject(result.FunctionResult));
    },
    error =>
    {
        Debug.Log("Error");
    });
    }

    public void OpenPanel(int index)
    {
        panels[index].SetActive(true);
    }

    public void ClosePanel(int index)
    {
        panels[index].SetActive(false);
    }

    public void OnTeamNameInput(string _teamName)
    {
        teamName = _teamName;
    }
    [Space(5)]
    [Header("Team")]
    private string teamName;
    public void MakeTeam()
    {
        if (teamName != "")
        {
            PlayFab.GroupsModels.EntityKey entityKey = GuildController.EntityKeyMaker(UserAccountManager.Instance.PlayFabID);
            GuildController guildController = new GuildController();
            guildController.CreateGroup(teamName, entityKey);
        }
        else
        {

        }
    }

    public void SearchTeams()
    {
        PlayFab.GroupsModels.EntityKey entityKey = GuildController.EntityKeyMaker(UserAccountManager.Instance.PlayFabID);
        guildController.ListGroups(entityKey);
    }

    public void JoinTeam()
    {
        //PlayFab.GroupsModels.EntityKey entityKey = GuildController.EntityKeyMaker(UserAccountManager.Instance.PlayFabID);
        //GuildController guildController = new GuildController();
        //guildController.ListGroups(entityKey);
    }

}
