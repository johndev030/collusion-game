using UnityEngine;
using UnityEngine.UI;

public class TokenOwner : MonoBehaviour
{
    public Text displayName;
    public Text memberSince;
    public Text country;
    public Text amountOfToken;
    public Text totalTokenCount;

    public void SetData(string _displayName, string _memberSince, string _country, string _amountOfToken, string _totalTokenCount = "??")
    {
        displayName.text = _displayName;
        memberSince.text = _memberSince;
        country.text = _country;
        amountOfToken.text = _amountOfToken;
        totalTokenCount.text = _totalTokenCount;
    }

}
