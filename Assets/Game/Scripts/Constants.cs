using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants 
{
    public static string USERNAME
    {
        get
        {
            return PlayerPrefs.GetString("USERNAME", "");
        }
        set
        {
            PlayerPrefs.SetString("USERNAME", value);
        }
    }
    public static string PASSWORD
    {
        get
        {
            return PlayerPrefs.GetString("PASSWORD", "");
        }
        set
        {
            PlayerPrefs.SetString("PASSWORD", value);
        }
    }
    private static string remember_me;
    public static string REMEMBER_ME
    {
        get
        {
            return PlayerPrefs.GetString("REMEMBER_ME", "False");
        }
        set
        {
            remember_me = value;
            PlayerPrefs.SetString("REMEMBER_ME", remember_me);
            Debug.Log("remember_me" + remember_me);
        }
    }

    public static string PROFILE_SETUP
    {
        get
        {
            return PlayerPrefs.GetString("PROFILE_SETUP", "False");
        }
        set
        {
            PlayerPrefs.SetString("PROFILE_SETUP", value);
        }
    }
}
