using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Token : MonoBehaviour
{

    public Text nameText;
    public Text countText;
    public Image tokenImage;
    public Transform countParent;

    internal string tokenName;
    internal int tokenCount =0 ;


    public void EnableToken()
    {
        
        nameText.text = tokenName;
        countText.text = tokenCount.ToString();

        if (tokenCount > 1)
        {
            countParent.gameObject.SetActive(true);
        }
    }

}
