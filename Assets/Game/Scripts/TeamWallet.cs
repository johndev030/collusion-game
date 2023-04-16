using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class TeamWallet : MonoBehaviour
{
    public Token[] tokens;
    public Color activeColor = Color.green;
    public Color passiveColor = Color.gray;
    public Text[] teamCash;
    public Text[] teamTotalTokens;

    public TeamDetails teamDetails;
    List<int> uniqueTokens = new List<int>();

    void OnEnable()
    {
        //SetupTokens();
        //for (int i = 0; i < teamCash.Length; i++)
        //{
        //    teamCash[i].text = "$" + teamDetails.TeamTotalCash.ToString();
        //}
    }

    public void SetupTokens()
    {
        //Debug.Log("Setting Tokens for teams");
        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i].name = (i + 1).ToString();
            tokens[i].tokenName = (i + 1);
            tokens[i].tokenCount = 0;
            tokens[i].ResetToken();
            tokens[i].EnableToken();
        }


        for (int i = 0; i < teamDetails.teamMembersList.Count; i++)
        {
            List<ItemInstance> inventory = teamDetails.teamMembersList[i].inventory.Inventory;
            for (int j = 0; j < inventory.Count; j++)
            {
                int tokenNumber = int.Parse(inventory[j].ItemId.Substring(3)) - 1;

                if (!uniqueTokens.Contains(tokenNumber))
                {
                    uniqueTokens.Add(tokenNumber);
                }

                //print("tokenNumber" + tokenNumber);
                tokens[tokenNumber].tokenImage.color = activeColor;
                tokens[tokenNumber].tokenCount += 1;
                tokens[tokenNumber].tokenOwner.Add(teamDetails.teamMembersList[i].profile);
                tokens[tokenNumber].tokenOwnerInventory.Add(teamDetails.teamMembersList[i].inventory);

                tokens[tokenNumber].ownerTotalTokensCount = inventory.Count;
                tokens[tokenNumber].EnableToken();
            }
        }

        UpdateTokens();
    }

    public void UpdateBalance()
    {
        for (int i = 0; i < teamCash.Length; i++)
        {
            teamCash[i].text = "$" + teamDetails.TeamTotalCash.ToString();
        }
    }

    public void UpdateTokens()
    {
        for (int i = 0; i < teamTotalTokens.Length; i++)
        {
            teamTotalTokens[i].text = uniqueTokens.Count + "/100";
        }
    }


}
