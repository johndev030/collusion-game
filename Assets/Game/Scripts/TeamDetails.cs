using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Guild;
using PlayFab;
using PlayFab.GroupsModels;
using UnityEngine;
using UnityEngine.UI;

public class TeamDetails : MonoBehaviour
{
    public Transform admins, members;

    public Text teamName;
    public Text totalCash;
    public Text teamTerms;
    public GameObject teamTermsParent;
    public GameObject applicationParent;
    public GameObject loading;
    int teamTotalCash;
    public int TeamTotalCash
    {
        get { return teamTotalCash; }
        set
        {
            teamTotalCash = value;
            totalCash.text = teamTotalCash.ToString();
        }
    }

    public string terms;

    public TeamMemberDetails teamMemberPrefab;
    public List<TeamMemberDetails> teamMembersList;
    public List<TeamMemberDetails> teamMembersListPlayerDashboard;

    public TeamWallet teamWallet;
    [Space(5)]
    [Header("Application for joining Team")]
    public ApplicantDetails applicantDetailsPrefab;
    public Transform applicantDetailsParent;
    public List<ApplicantDetails> applicantDetails;
    public Transform applicantsCountParent;
    public Text applicantsCount;

    GuildController guildController = new GuildController();
    // Start is called before the first frame update
    void Awake()
    {
        //TeamTotalCash = 0;
//        print("TeamTotalCash");
        //GetGroupDetails();
    }

    List<EntityMemberRole> entities;
    internal static bool isTeamMemberDetailRetrieved = false;
    public void GetGroupDetails()
    {
        PlayerDashboard.Instance.refreshButton.interactable = false;
        PlayerDashboard.Instance.refreshButton.GetComponent<DOTweenAnimation>().DOPlay();
        loading.SetActive(true);
        teamName.text = UserAccountManager.Instance.GroupName;
        teamTotalCash = 0;

        entities = (UserAccountManager.Instance.ListGroupMembers.Members);

        if (teamMembersList.Count > 0)
        {
            foreach (var item in teamMembersList)
            {
                Destroy(item.gameObject);
            }
        }
        if (teamMembersListPlayerDashboard.Count > 0)
        {
            foreach (var item in teamMembersListPlayerDashboard)
            {
                Destroy(item.gameObject);
            }
        }

        for (int j = 0; j < entities[0].Members.Count; j++)
        {
            PlayFab.ProfilesModels.GetEntityProfileRequest req = new PlayFab.ProfilesModels.GetEntityProfileRequest();
            req.Entity = new PlayFab.ProfilesModels.EntityKey { Id = entities[0].Members[j].Key.Id, Type = "title_player_account" };
            req.DataAsObject = true;
//            print("Getting Admins");

            PlayFabProfilesAPI.GetProfile(req, response =>
            {
                if (response.Profile.Entity.Id != PlayerDashboard.defaultPlayerTitleID[0])
                {
                    TeamMemberDetails teamMemberDashboard = Instantiate(teamMemberPrefab, PlayerDashboard.Instance.teamDetailParent);
                    teamMemberDashboard.SetData(response);
                    teamMembersListPlayerDashboard.Add(teamMemberDashboard);
                    TeamMemberDetails teamMember = Instantiate(teamMemberPrefab, admins);
                    teamMember.teamDetails = this;
                    teamMembersList.Add(teamMember);
                    teamMember.SetData(response);
                    if (teamMembersList.Count == entities[0].Members.Count-1 && entities[1].Members.Count ==0)
                    {
                        isTeamMemberDetailRetrieved = true;
                        loading.SetActive(false);
                        print("loading.SetActive(false)");
                        PlayerDashboard.Instance.refreshButton.interactable = true;
                        PlayerDashboard.Instance.refreshButton.GetComponent<DOTweenAnimation>().DOPause();
                    }
                }
            }, error =>
            {
                Debug.Log("Error " + error.Error);
            }
            );
        }
        bool isMemberPlayer = false;
        for (int j = 0; j < entities[1].Members.Count; j++)
        {
            PlayFab.AdminModels.GetPlayerProfileRequest request = new PlayFab.AdminModels.GetPlayerProfileRequest();
            request.PlayFabId = entities[1].Members[j].Key.Id;

//            Debug.Log(UserAccountManager.Instance.EntityToken.Entity.Id + " ~ "+ entities[1].Members[j].Key.Id);

            if (UserAccountManager.Instance.EntityToken.Entity.Id == entities[1].Members[j].Key.Id)
            {
                //Debug.Log("You are a Member, not Admin");
                isMemberPlayer = true;
            }

            PlayFab.ProfilesModels.GetEntityProfileRequest req = new PlayFab.ProfilesModels.GetEntityProfileRequest() {
            
            };
            req.Entity = new PlayFab.ProfilesModels.EntityKey { Id = entities[1].Members[j].Key.Id, Type = "title_player_account"  };
            req.DataAsObject = true;
            //print("Getting Memebers");

            PlayFabProfilesAPI.GetProfile(req, response =>
            {
                if (response.Profile.Entity.Id != PlayerDashboard.defaultPlayerTitleID[0])
                {

                    TeamMemberDetails teamMemberDashboard = Instantiate(teamMemberPrefab, PlayerDashboard.Instance.teamDetailParent);
                    teamMemberDashboard.SetData(response);
                    teamMembersListPlayerDashboard.Add(teamMemberDashboard);

                    TeamMemberDetails teamMember = Instantiate(teamMemberPrefab, members);
                    teamMember.teamDetails = this;
                    teamMembersList.Add(teamMember);
                    teamMember.SetData(response);
                    if (teamMembersList.Count == (entities[0].Members.Count + entities[1].Members.Count -1))
                    {
                        isTeamMemberDetailRetrieved = true;
                        loading.SetActive(false);
                        print("loading.SetActive(false)");

                        PlayerDashboard.Instance.refreshButton.interactable = true;
                        PlayerDashboard.Instance.refreshButton.GetComponent<DOTweenAnimation>().DOPause();
                    }
                }
            }, error =>
            {
                Debug.Log("Error " + error.Error);
            }
            );
        }

        //Getting team pending applications

        //Debug.Log("GroupEntity ID " + UserAccountManager.Instance.GroupEntity.Id);
        //Debug.Log("GroupEntity TYPE " + UserAccountManager.Instance.GroupEntity.Type);

        if (isMemberPlayer == false)
        {
            UserAccountManager.OnListMembershipOpportunities.AddListener(OnListMembershipOpportunitiesResult);
            guildController.ListGroupApplications(UserAccountManager.Instance.GroupEntity);
        }
    }

    private void OnListMembershipOpportunitiesResult(string response)
    {
        if(response == "Success")
        {
            applicantsCount.text = UserAccountManager.Instance.groupApplications.Count.ToString();
            applicantsCountParent.gameObject.SetActive(true);
            for (int i = 0; i < UserAccountManager.Instance.groupApplications.Count; i++)
            {
                ApplicantDetails applicant = Instantiate(applicantDetailsPrefab, applicantDetailsParent);
                applicant.groupApplication = UserAccountManager.Instance.groupApplications[i];
                applicant.SetDatails();
                applicantDetails.Add(applicant);
            }
        }
        else if (response == "Failed")
        {

        }
    }
}
