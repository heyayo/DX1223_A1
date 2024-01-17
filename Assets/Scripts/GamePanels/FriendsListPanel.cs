using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class FriendsListPanel : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text friendsListText;
    [SerializeField] private TMP_InputField addField;
    [SerializeField] private TMP_InputField removeField;

    private Dictionary<string, FriendInfo> _registry;

    private void Start()
    {
        _registry = new Dictionary<string, FriendInfo>();
        gameObject.SetActive(false);
        RequestFriendsList();
    }

    private void EmptyTexts()
    { friendsListText.text = ""; }
    private void InsertNameIntoList(string username)
    { friendsListText.text += username + '\n'; }

    private void PopulateFriendsList()
    {
        foreach (var x in _registry)
        { InsertNameIntoList(x.Value.Username); }
    }

    private void RequestFriendsList()
    {
        var req = new GetFriendsListRequest {};

        PlayFabClientAPI.GetFriendsList(req, result =>
        {
            Debug.Log("Got Friends List");
            if (result == null)
            {
                Debug.Log("Friends List is NULL");
                return;
            }
            var list = result.Friends;
            EmptyTexts();
            _registry.Clear();
            foreach (var t in list)
            {
                InsertNameIntoList(t.Username);
                _registry.Add(t.Username,t);
            }
        }, error =>
        {
            Debug.Log("Failed To Get Friends List");
            EmptyTexts();
            InsertNameIntoList("Failed To Fetch Friends");
        });
    }

    private void AddFriend(string username)
    {
        var req = new AddFriendRequest
        { FriendUsername = username };

        PlayFabClientAPI.AddFriend(req, result =>
        {
            Debug.Log("Sent Added Friend");
            InsertNameIntoList(username);
        },error => Debug.LogError("Failed To Add Friend"));
    }

    private void RemoveFriend(string username)
    {
        var req = new RemoveFriendRequest
        { FriendPlayFabId = _registry[username].FriendPlayFabId };

        PlayFabClientAPI.RemoveFriend(req, result =>
            {
                Debug.Log("Removed Friend");
                _registry.Remove(username);
                PopulateFriendsList();
            }, error =>
        { Debug.LogError("Failed To Remove Friend"); });
    }

    public void AddFriendButtonCall()
    {
        AddFriend(addField.text);
        addField.text = "";
    }

    public void RemoveFriendButtonCall()
    {
        RemoveFriend(removeField.text);
        removeField.text = "";
    }
}
