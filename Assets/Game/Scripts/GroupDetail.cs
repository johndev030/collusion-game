using System.Collections;
using System.Collections.Generic;
using Guild;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using UnityEngine;
using UnityEngine.UI;


public class GroupDetail : MonoBehaviour
{
    public Text groupName;
    public Text groupOwner;
    public Text groupCount;
    public Text groupCreationDate;
    public List<EntityMemberRole> members;
    [SerializeField]
    public GetGroupResponse groupData;
    public ListGroupMembersResponse groupMembers;
    public GetPlayerProfileResult profileResult;

    public void SetData()
    {
        groupName.text = groupData.GroupName;

        groupCreationDate.text = groupData.Created.ToLocalTime().ToString();

        ListGroupMembersRequest request = new ListGroupMembersRequest();
        request.Group = groupData.Group;
        PlayFabGroupsAPI.ListGroupMembers(request, respone => {
            groupCount.text = respone.Members.Count.ToString();
            members = respone.Members;

            foreach (var member in respone.Members)
            {
            
                foreach (var item in member.Members)
                {
                    
                    foreach (var item2 in item.Lineage)
                    {
                        if (member.RoleId == "admins" && item2.Value.Id != PlayerDashboard.defaultPlayerTitleID[1]) {
                            //Debug.Log("Key "+ item2.Key);
                            Debug.Log("Value " + item2.Value.Id);
                            Debug.Log("Value " + item2.Value.Type);
                            GetPlayerProfileRequest profileRequest = new GetPlayerProfileRequest();
                            profileRequest.PlayFabId = item2.Value.Id;
                            PlayFabClientAPI.GetPlayerProfile(profileRequest, response => {
                                profileResult = response;
                                groupOwner.text = response.PlayerProfile.DisplayName;
                            }, error => { });
                        }
                    }
                }
            }

            //for (int i = 0; i < members.Count; i++)
            //{
            //    Debug.Log("Name " + members[i].Members + " ID " + members[i].RoleId);
            //    if(members[i].RoleId == "admins")
            //    {
            //        groupOwner.text = members[i].RoleId;
            //    }
            //}
        }, error => { });
    }

    // Update is called once per frame
    public void RequestToJoin()
    {
        GuildController guildController = new GuildController();
        UserAccountManager.OnGroupJoinResult.AddListener(OnGroupJoinedResult);
        PlayFab.ClientModels.EntityKey entity = UserAccountManager.Instance.EntityToken.Entity;
        PlayFab.GroupsModels.EntityKey Entity = new PlayFab.GroupsModels.EntityKey { Id = entity.Id, Type = entity.Type };
        guildController.ApplyToGroup(groupData.Group.Id, Entity);
    }

    void OnGroupJoinedResult(string result)
    {
        if (result == "Success")
        {
            NotificationManager.Instance.CreateNotification("Group Joining Requested", NotificationType.Normal);
        }
        else if (result == "Failed")
        {
            NotificationManager.Instance.CreateNotification("Group Request Failed", NotificationType.Error);
        }
        UserAccountManager.OnGroupJoinResult.RemoveListener(OnGroupJoinedResult);
    }
}
