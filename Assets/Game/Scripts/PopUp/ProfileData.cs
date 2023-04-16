using System.Collections;
using System.Collections.Generic;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;


public class ProfileData : MonoBehaviour
{
    public Image profilePhoto;
    public Text displayName;
    public Text memberSince;
    public Text country;
    public Text playMoney;
    public Transform TokensParent;
    public List<Token> tokens;
    [Space(6)]
    [Header("TOKEN OWNED")]
    public List<Token> tokensOwned;



    // Start is called before the first frame update
    public void SetProfileData(GetPlayerProfileResult profile, VirtualCurrency currency, GetUserInventoryResult inventory, int profilePhotoIndex)
    {
        displayName.text = profile.PlayerProfile.DisplayName;

        if(profile.PlayerProfile.Created.ToString() != "")
        {
            memberSince.text = profile.PlayerProfile.Created.ToString();
        }
        else
        {
            memberSince.text = "00/00/0000";
        }

        if (profile.PlayerProfile.Locations != null)
        {
            country.text = profile.PlayerProfile.Locations[0].ToString();
        }
        else
        {
            country.text = "Unavailable";
        }
        playMoney.text = "$" + currency.CL;


        profilePhoto.sprite = MainMenuUIController.Instance.profileImageSprites[profilePhotoIndex];

        for (int i = 0; i < tokens.Count; i++)
        {
            tokens[i].name = (i + 1).ToString();
            tokens[i].tokenName = (i + 1);
            tokens[i].tokenCount = 0;
            tokens[i].EnableToken();
        }

        for (int i = 0; i < inventory.Inventory.Count; i++)
        {
            int token = int.Parse(inventory.Inventory[i].ItemId.Substring(3));
//            print("token" + token);
            tokens[token - 1].AddToken();
            tokens[token - 1].tokenOwner.Add(profile);
            tokens[token - 1].tokenOwnerInventory.Add(inventory);

            tokens[token - 1].ownerTotalTokensCount = inventory.Inventory.Count;
            tokensOwned.Add(tokens[token - 1]);
        }

    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
