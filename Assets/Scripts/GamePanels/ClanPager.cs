using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.GroupsModels;
using UnityEngine;
using TMPro;

public class ClanPager : MonoBehaviour
{
    private PlayerData _data;

    [Header("Texts")]
    [SerializeField] private TMP_Text clanName;
    [SerializeField] private TMP_Text clanMembers;
    [SerializeField] private TMP_Text outputText;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField joinField;
    [SerializeField] private TMP_InputField createField;

    private void Awake()
    {
        _data = PlayerData.RetrieveData();
        gameObject.SetActive(false);
    }

    public void ButtonCallback()
    { FetchClanData(); }
    public void JoinClanCall()
    { JoinClan(joinField.text); }
    public void CreateClanCall()
    { CreateClan(createField.text); }
    public void LeaveClanCall()
    { LeaveClan(); }

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
                clanMembers.text += response.Members[i].RoleName;
                for (int j = 0; j < response.Members[i].Members.Count; ++j)
                {
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
            clanMembers.text = _data.entityID;
            outputText.text = "Created Clan, " + clan;
        }, common_error);
    }

    private void LeaveClan()
    {
        var req = new RemoveMembersRequest
            { Group = _data.currentGroup, Members = new List<EntityKey> { _data.entityKey } };

        PlayFabGroupsAPI.RemoveMembers(req, response =>
        {
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

        var req = new ApplyToGroupRequest { };

        PlayFabGroupsAPI.ApplyToGroup(req, response =>
        {
            outputText.text = "Applied To Clan, " + clan;
            Debug.Log("Applied to Group");
        }, common_error);
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
