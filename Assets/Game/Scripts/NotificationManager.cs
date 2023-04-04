using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NotificationType
{
    Normal,
    Error
}

public class NotificationManager : MonoBehaviour
{
    public NotificationDetails prefab;
    public AudioClip notificationSound;
    public Canvas parent;
    public float notificationLifeTime =2;
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
    /// <summary>
    ///  Pop up a notification on UI at bottom side of screen
    /// </summary>
    /// <param name="notificationText"></param>
    /// <param name="notificationType"></param>
    public void CreateNotification(string notificationText, NotificationType notificationType)
    {
        NotificationDetails notification = Instantiate(prefab, parent.transform);
        Color color = notificationType == NotificationType.Normal ? Color.yellow : Color.red;
        notification.SetText(notificationText, notificationLifeTime, color);
    }

}
