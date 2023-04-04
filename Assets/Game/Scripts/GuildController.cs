using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guild
{
    /// <summary>
    /// Assumptions for this controller:
    /// + Entities can be in multiple groups
    ///   - This is game specific, many games would only allow 1 group, meaning you'd have to perform some additional checks to validate this.
    /// </summary>
    [Serializable]
    public class GuildController
    {
        // A local cache of some bits of PlayFab data
        // This cache pretty much only serves this example , and assumes that entities are uniquely identifiable by EntityId alone, which isn't technically true. Your data cache will have to be better.
        public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
        public readonly Dictionary<string, string> GroupNameById = new Dictionary<string, string>();

        public static EntityKey EntityKeyMaker(string entityId)
        {
            return new EntityKey { Id = entityId };
        }

        private void OnSharedError(PlayFab.PlayFabError error)
        {
            if (UserAccountManager.OnGroupCreationResult != null)
            {
                UserAccountManager.OnGroupCreationResult.Invoke("Failed");
            }
            if (UserAccountManager.OnGroupJoinResult != null)
            {
                UserAccountManager.OnGroupJoinResult.Invoke("Failed");
            }
            if (UserAccountManager.OnListGroupMembersResult != null)
            {
                UserAccountManager.OnListGroupMembersResult.Invoke("Failed");
            }
            if (UserAccountManager.OnListGroupResult != null)
            {
                UserAccountManager.OnListGroupResult.Invoke("Failed");
            }
            if (UserAccountManager.OnListMembershipOpportunities != null)
            {
                UserAccountManager.OnListMembershipOpportunities.Invoke("Failed");
            }

            NotificationManager.Instance.CreateNotification(error.GenerateErrorReport(), NotificationType.Error);
            Debug.LogError(error.GenerateErrorReport());
        }

        public void ListGroupsMembers(EntityKey entityKey)
        {
            var request = new ListGroupMembersRequest { Group = entityKey };
            PlayFabGroupsAPI.ListGroupMembers(request, OnListGroupsMembers, OnSharedError);
        }

        private void OnListGroupsMembers(ListGroupMembersResponse response)
        {
            UserAccountManager.Instance.ListGroupMembers = response;
            UserAccountManager.OnListGroupMembersResult.Invoke("Success");
        }

        public void ListGroups(EntityKey entityKey)
        {
            Debug.Log("entityKey " + entityKey.Id + " entityType " + entityKey.Type);
            var request = new ListMembershipRequest { Entity = entityKey };
            PlayFabGroupsAPI.ListMembership(request, OnListGroups, OnSharedError);
        }



        private void OnListGroups(ListMembershipResponse response)
        {
            var prevRequest = (ListMembershipRequest)response.Request;
            UserAccountManager.Instance.AllGroups = response.Groups;
            UserAccountManager.OnListGroupResult.Invoke("Success");

            foreach (var pair in response.Groups)
            {
                GroupNameById[pair.Group.Id] = pair.GroupName;
                EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, pair.Group.Id));
            }
        }
        public void ListGroupApplications(EntityKey entityKey)
        {
            var request = new ListGroupApplicationsRequest { Group = entityKey };
            PlayFabGroupsAPI.ListGroupApplications(request, OnListGroupApplications, OnSharedError);
        }

        private void OnListGroupApplications(ListGroupApplicationsResponse response)
        {
            if (response.Applications != null)
            {
                UserAccountManager.Instance.groupApplications = response.Applications;
                UserAccountManager.OnListMembershipOpportunities.Invoke("Success");
            }
            else
            {
                Debug.Log("No Invitations, Bugger Off..!");
            }
        }

        public void ListMembershipOpportunities(EntityKey entityKey)
        {
            var request = new ListMembershipOpportunitiesRequest{ Entity = entityKey };
            PlayFabGroupsAPI.ListMembershipOpportunities(request, OnListMembershipOpportunities, OnSharedError);
        }

        private void OnListMembershipOpportunities(ListMembershipOpportunitiesResponse response)
        {
            if (response.Invitations != null)
            {
                UserAccountManager.Instance.groupInvitations = response.Invitations;
                UserAccountManager.Instance.groupApplications = response.Applications;
                UserAccountManager.OnListMembershipOpportunities.Invoke("Success");
            }
            else
            {
                Debug.Log("No Invitations, Bugger Off..!");
            }
        }

        public void CreateGroup(string groupName, EntityKey entityKey = null)
        {
            // A player-controlled entity creates a new group
            var request = new CreateGroupRequest { GroupName = groupName, Entity = entityKey};
            PlayFabGroupsAPI.CreateGroup(request, OnCreateGroup, OnSharedError);
        }
        private void OnCreateGroup(CreateGroupResponse response)
        {
            Debug.Log("Group Created: " + response.GroupName + " - " + response.Group.Id);

            var prevRequest = (CreateGroupRequest)response.Request;
            EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, response.Group.Id));
            GroupNameById[response.Group.Id] = response.GroupName;

            UserAccountManager.Instance.GroupEntity = response.Group;
            UserAccountManager.Instance.GroupName = response.GroupName;
            UserAccountManager.Instance.GroupID = response.Group.Id;
            UserAccountManager.Instance.IsTeamMember = true;
            UserAccountManager.OnGroupCreationResult.Invoke("Success");

            NotificationManager.Instance.CreateNotification(response.GroupName + " Team Created", NotificationType.Normal);
        }
        public void DeleteGroup(string groupId)
        {
            // A title, or player-controlled entity with authority to do so, decides to destroy an existing group
            var request = new DeleteGroupRequest { Group = EntityKeyMaker(groupId) };
            PlayFabGroupsAPI.DeleteGroup(request, OnDeleteGroup, OnSharedError);
        }
        private void OnDeleteGroup(EmptyResponse response)
        {
            var prevRequest = (DeleteGroupRequest)response.Request;
            Debug.Log("Group Deleted: " + prevRequest.Group.Id);

            var temp = new HashSet<KeyValuePair<string, string>>();
            foreach (var each in EntityGroupPairs)
                if (each.Value != prevRequest.Group.Id)
                    temp.Add(each);
            EntityGroupPairs.IntersectWith(temp);
            GroupNameById.Remove(prevRequest.Group.Id);
        }

        public void InviteToGroup(string groupId, EntityKey entityKey)
        {
            // A player-controlled entity invites another player-controlled entity to an existing group
            var request = new InviteToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey };
            PlayFabGroupsAPI.InviteToGroup(request, OnInvite, OnSharedError);
        }
        public void OnInvite(InviteToGroupResponse response)
        {
            var prevRequest = (InviteToGroupRequest)response.Request;

            // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
            var request = new AcceptGroupInvitationRequest { Group = EntityKeyMaker(prevRequest.Group.Id), Entity = prevRequest.Entity };
            PlayFabGroupsAPI.AcceptGroupInvitation(request, OnAcceptInvite, OnSharedError);
        }
        public void OnAcceptInvite(EmptyResponse response)
        {
            var prevRequest = (AcceptGroupInvitationRequest)response.Request;
            Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
            EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, prevRequest.Group.Id));
        }

        public void ApplyToGroup(string groupId, EntityKey entityKey)
        {
            // A player-controlled entity applies to join an existing group (of which they are not already a member)
            var request = new ApplyToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey };
            PlayFabGroupsAPI.ApplyToGroup(request, OnApply, OnSharedError);
        }
        public void OnApply(ApplyToGroupResponse response)
        {
            var prevRequest = (ApplyToGroupRequest)response.Request;

            // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
            UserAccountManager.OnGroupJoinResult.Invoke("Success");
        }
        public void OnAcceptApplication(EmptyResponse response)
        {
            var prevRequest = (AcceptGroupApplicationRequest)response.Request;
            Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
        }
        public void KickMember(string groupId, EntityKey entityKey)
        {
            var request = new RemoveMembersRequest { Group = EntityKeyMaker(groupId), Members = new List<EntityKey> { entityKey } };
            PlayFabGroupsAPI.RemoveMembers(request, OnKickMembers, OnSharedError);
        }
        private void OnKickMembers(EmptyResponse response)
        {
            var prevRequest = (RemoveMembersRequest)response.Request;

            Debug.Log("Entity kicked from Group: " + prevRequest.Members[0].Id + " to " + prevRequest.Group.Id);
            EntityGroupPairs.Remove(new KeyValuePair<string, string>(prevRequest.Members[0].Id, prevRequest.Group.Id));
        }
    }
}