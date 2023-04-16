using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class TokenData : MonoBehaviour
{
    public Text tokenNumber;
    public Transform TokensParent;
    public List<TokenOwner> tokenOwner;
    public TokenOwner tokenOwnerPrefab;
    public List<GetUserInventoryResult> inv;
    public List<GetUserInventoryResult> tokenOwnerInventory;

    List<GetPlayerProfileResult> profiles = new List<GetPlayerProfileResult>();
    public void SetData(int _tokenNumber, List<GetPlayerProfileResult> profile, List<GetUserInventoryResult> inventory,  int tokenCount, int totalTokenCount =0)
    {
        inv = inventory;
//        Debug.Log("inventory " + inventory.Count);
        int thisTokenCount =0;
        tokenNumber.text = "TOKEN "+_tokenNumber;

        /// Checking duplicate tokens for each inventory
        for (int i = 0; i < profile.Count; i++)
        {
            if (profiles.Contains(profile[i]))
            {
                //print("Already Present, F*** Yeahh..");
                break;
            }
            else
            {
                profiles.Add(profile[i]);
            }
            thisTokenCount = 0;
            for (int j = 0; j < inventory[i].Inventory.Count; j++)
            {
                if (int.Parse(inventory[i].Inventory[j].ItemId.Substring(3)) == _tokenNumber)
                {
                    thisTokenCount++;
                }
            }
            TokenOwner tokenOwner = Instantiate(tokenOwnerPrefab, TokensParent);
            tokenOwner.SetData(profile[i].PlayerProfile.DisplayName, profile[i].PlayerProfile.Created.ToString(), "", thisTokenCount.ToString(), totalTokenCount.ToString());
        }
    }
 
    public void Close()
    {
        Destroy(gameObject);
    }
}
