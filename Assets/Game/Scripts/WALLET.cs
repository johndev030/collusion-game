using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WALLET : MonoBehaviour
{
    public Token[] tokens;
    public Color activeColor = Color.green;
    public Color passiveColor = Color.gray;

    void OnEnable()
    {
        SetupTokens();
    }

    void SetupTokens()
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i].name = (i + 1).ToString();
            tokens[i].tokenName = (i + 1);
            tokens[i].tokenCount = 0;
            tokens[i].EnableToken();
        }
        for (int i = 0; i < UserAccountManager.Instance.uniqueTokens.Count; i++)
        {

            tokens[UserAccountManager.Instance.uniqueTokens[i]-1].tokenImage.color = activeColor;
            tokens[UserAccountManager.Instance.uniqueTokens[i]-1].tokenCount+=1;
            tokens[UserAccountManager.Instance.uniqueTokens[i]-1].EnableToken();

        }
    }
}
