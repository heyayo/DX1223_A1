using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.GroupsModels;
using UnityEngine;
using TMPro;
using EntityKey = PlayFab.GroupsModels.EntityKey;

public class ClanPager : MonoBehaviour
{
    private PlayerData _data;
    private List<GroupApplication> _applicants;

    [Header("Texts")]
    [SerializeField] private TMP_Text clanName;
    [SerializeField] private TMP_Text clanMembers;
    [SerializeField] private TMP_Text outputText;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField joinField;
    [SerializeField] private TMP_InputField createField;
    [SerializeField] private TMP_Dropdown applicantsList;

    private void Awake()
    {
        _data = PlayerData.RetrieveData();
        _applicants = new List<GroupApplication>();
        outputText.text = "";
        _data.currentGroup = null;
        gameObject.SetActive(false);
    }

    public void ButtonCallback()
    {
        FetchClanData(); // Fetch data about the group
        FetchApplicants(); // See who wants to join the group
    }
    public void JoinClanCall()
    { JoinClan(joinField.text); }
    public void CreateClanCall()
    { CreateClan(createField.text); }
    public void LeaveClanCall()
    { LeaveClan(); }
    public void AcceptApplicationCall()
    { AcceptClanApplication(_applicants[applicantsList.value].Entity.Key); }

    private void FetchClanData()
    {
        var req = new ListMembershipRequest();

        PlayFabGroupsAPI.ListMembership(req, response =>
        {
            if (response.Groups.Count <= 0)
            {
                clanName.text = "You are not in a clan";
                clanMembers.text = "";
                return;
            }

            clanName.text = response.Groups[0].GroupName;
            _data.currentGroup = response.Groups[0].Group;
            Debug.Log("In Clan: " + _data.currentGroup.Id);
            FetchClanMembers();
        }, common_error);
    }

    private void FetchClanMembers()
    {
        var req = new ListGroupMembersRequest{ Group = _data.currentGroup };

        PlayFabGroupsAPI.ListGroupMembers(req, response =>
        {
            clanMembers.text = "";
            for (int i = 0; i < response.Members.Count; ++i)
            {
                for (int j = 0; j < response.Members[i].Members.Count; ++j)
                {
                    clanMembers.text += response.Members[i].RoleName + " | ";
                    clanMembers.text += response.Members[i].Members[j].Key.Id + '\n';
                }
            }
            clanMembers.text += "Members: " + response.Members.Count;
        }, common_error);
    }

    private void CreateClan(string clan)
    {
        var req = new CreateGroupRequest { GroupName = clan };

        PlayFabGroupsAPI.CreateGroup(req, response =>
        {
            _data.currentGroup = response.Group;
            clanName.text = clan;
            clanMembers.text = "";
            FetchClanData();
            outputText.text = "Created Clan, " + clan;
        }, common_error);
    }

    private void LeaveClan()
    {
        var req = new RemoveMembersRequest
            { Group = _data.currentGroup, Members = new List<EntityKey> { _data.entityKey } };

        PlayFabGroupsAPI.RemoveMembers(req, response =>
        {
            _data.currentGroup = null;
            FetchClanData();
            outputText.text = "Left Clan";
            Debug.Log("Left Clan");
        },common_error);
    }

    private void JoinClan(string clan)
    {
        if (_data.currentGroup != null)
        {
            outputText.text = "Already in a Clan";
            Debug.Log("Already in a Clan");
            return;
        }

        GetGroupID(clan, response =>
        {
            var req = new ApplyToGroupRequest { Group = response.Group };

            PlayFabGroupsAPI.ApplyToGroup(req, response =>
            {
                outputText.text = "Applied To Clan, " + clan;
                Debug.Log("Applied to Group");
            }, common_error);
        });
    }

    private void AcceptClanApplication(EntityKey applicant)
    {
        var req = new AcceptGroupApplicationRequest { Group = _data.currentGroup, Entity =  applicant };

        PlayFabGroupsAPI.AcceptGroupApplication(req, response =>
        {
            Debug.Log("Accepted Group Application");
            outputText.text = "Accepted Group Application";
            FetchClanMembers();
        },common_error);
    }

    private void FetchApplicants()
    {
        applicantsList.options.Clear(); // Empty out options
        if (_data.currentGroup == null)
        {
            Debug.Log("Unable to fetch Applicants | Not in a clan");
            return;
        }
        Debug.Log("Fetching Applicants");
        var req = new ListGroupApplicationsRequest { Group = _data.currentGroup };

        PlayFabGroupsAPI.ListGroupApplications(req, response =>
        {
            _applicants = new List<GroupApplication>(response.Applications);
            Debug.Log("Fetched Group Applicants");
            foreach (var applicant in _applicants)
            {
                // Populate Dropdown Menu
                applicantsList.options.Add(new TMP_Dropdown.OptionData{text = applicant.Entity.Key.Id});
            }
        },common_error);
    }

    private void GetGroupID(string group_name, Action<GetGroupResponse> callback)
    {
        var req = new GetGroupRequest { GroupName = group_name };

        PlayFabGroupsAPI.GetGroup(req, callback,common_error);
    }

    private void common_error(PlayFabError error)
    {
        outputText.text = "Error";
        Debug.LogError(error.GenerateErrorReport());
    }
}
