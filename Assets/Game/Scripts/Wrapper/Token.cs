using System.Collections;
using System.Collections.Generic;
using PlayFab.ClientModels;
using PlayFab.ProfilesModels;
using UnityEngine;
using UnityEngine.UI;



public class Token : MonoBehaviour
{

    public Text nameText;
    public Text countText;
    public Image tokenImage;
    public Transform countParent;
    public List<GetPlayerProfileResult> tokenOwner;
    public List<GetUserInventoryResult> tokenOwnerInventory;

    public int ownerTotalTokensCount;

    internal int tokenName;
    internal int tokenCount =0 ;

    public Color activeColor = Color.green;
    public Color passiveColor = Color.gray;

    public TokenData tokenDataPrefab;

    public void EnableToken()
    {
        nameText.text = tokenName.ToString();
        countText.text = tokenCount.ToString();
        if (tokenCount>0)
        {
            tokenImage.color = activeColor;
        }
        else
        {
            tokenImage.color = passiveColor;
        }

        if (tokenCount > 1)
        {
            countParent.gameObject.SetActive(true);
        }
        else
        {
            countParent.gameObject.SetActive(false);
        }
    }

    public void AddToken()
    {
        tokenCount++;
        EnableToken();
    }

    public void RemoveToken()
    {
        tokenCount--;
        EnableToken();
    }

    public void OpenTokenDetail()
    {
        TokenData tokenData = Instantiate(tokenDataPrefab, MainMenuUIController.Instance.popUpsParent);
        tokenData.SetData(tokenName, tokenOwner, tokenOwnerInventory, tokenCount, ownerTotalTokensCount);
    }

    public void ResetToken()
    {
        tokenOwner.Clear();
        tokenOwnerInventory.Clear();
    }


}
