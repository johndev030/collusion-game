using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Dictionary<string, string> Data = new Dictionary<string, string> {
               { "isTeamMember", "true" },
               { "GroupID",UserAccountManager.Instance.GroupEntity.Id }
            };
        //Debug.Log("Key" + groupApplication.Entity.Key);
        //JsonUtility.ToJson(Data);
        //PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        //{
        //    FunctionName = "updateUserData",
        //    FunctionParameter = new { playFabId = groupApplication.Entity.Key, data = Data },
        //},
        //response =>
        //{
        //    for (int i = 0; i < response.Logs.Count; i++)
        //    {
        //        Debug.Log(response.Logs[i].Message);
        //    }
        //    Debug.Log(response.Error);
        //    Debug.Log(response.FunctionResult.ToString());
        //},
        //error =>
        //{

        //});
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
