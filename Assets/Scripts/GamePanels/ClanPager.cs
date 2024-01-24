using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.GroupsModels;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class ClanPager : MonoBehaviour
{
    private PlayerData _data;

    [SerializeField] private TMP_Text clanName;
    [SerializeField] private TMP_Text clanMembers;

    private void Awake()
    {
        _data = PlayerData.RetrieveData();
        gameObject.SetActive(false);
    }

    public void ButtonCallback()
    { FetchClanData(); }

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
        }, error);
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
        }, error);
    }

    private void CreateClan(string name)
    {
        var req = new CreateGroupRequest { GroupName = name };

        PlayFabGroupsAPI.CreateGroup(req, response =>
        {
            _data.currentGroup = response.Group;
            clanName.text = name;
            clanMembers.text = "";
            clanMembers.text = _data.entityID;
        }, error);
    }

    private void LeaveClan()
    {
        var req = new RemoveMembersRequest
            { Group = _data.currentGroup, Members = new List<EntityKey> { _data.entityKey } };

        PlayFabGroupsAPI.RemoveMembers(req, response =>
        {
            FetchClanData();
            Debug.Log("Left Clan");
        },error);
    }

    private void JoinClan(string clan)
    {
        if (_data.currentGroup != null)
        {
            Debug.Log("Already in a Clan");
            return;
        }

        var req = new ApplyToGroupRequest { };
    }

    private void GetGroupID(string name, Action<GetGroupResponse> callback)
    {
        var req = new GetGroupRequest { GroupName = name };

        PlayFabGroupsAPI.GetGroup(req, callback,error);
    }

    private void error(PlayFabError error)
    { Debug.LogError(error.GenerateErrorReport()); }
}
