using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public NotificationDetails notificationDetails;
    public AudioClip notificationSound;
    public Canvas parent;
    public float notificationLifeTime;
    public bool isSoundEnable;

    public static NotificationManager Instance;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void CreateNotification()
    {

    }

}
