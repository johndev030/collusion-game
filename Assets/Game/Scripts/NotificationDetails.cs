using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationDetails : MonoBehaviour
{

    public Text details;
    public Image image;

    private void Awake()
    {
        //image = GetComponent<Image>();
    }

    public void SetText(string detail, float lifeTime, Color color)
    {
        details.text = detail;
        image.color = color;
        Destroy(this.gameObject, lifeTime);
    }
}
