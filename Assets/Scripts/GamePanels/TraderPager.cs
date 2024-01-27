using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

public class TraderPager : MonoBehaviour
{
    [SerializeField] private TMP_Text output;

    [Header("Inputs")]
    [SerializeField] private TMP_Dropdown trades;
    [SerializeField] private TMP_Dropdown friends;
    [SerializeField] private TMP_Dropdown items;

    [Header("External")]
    [SerializeField] private FriendsListPanel _friends;

    private List<string> _currentTrade;
    private Dictionary<string, string> _trades;

    private PlayerData _data;

    public void ButtonCallback()
    {
        _data = PlayerData.RetrieveData();

        _currentTrade = new List<string>();
        _trades = new Dictionary<string, string>();

        trades.options.Clear();
        friends.options.Clear();
        items.options.Clear();

        foreach (var ship in _data.shipsOwnedIndex)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
            data.text = ship.Key;
            items.options.Add(data);
        }

        var flist = _friends.Friends;
        foreach (var f in flist)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
            data.text = f.Key;
            friends.options.Add(data);
        }

        FetchActiveTrades();
    }

    private void FetchActiveTrades()
    {
        var req = new GetPlayerTradesRequest{};

        PlayFabClientAPI.GetPlayerTrades(req, response =>
        {
            _trades.Clear();
            foreach (var t in response.OpenedTrades)
            {
                TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
                data.text = t.OfferingPlayerId;
                trades.options.Add(data);
                _trades.Add(t.OfferingPlayerId,t.TradeId);
            }
        }, common_error);
    }

    private void AddItemToTrade(string key)
    {
        if (_currentTrade.Contains(key))
            return;
        _currentTrade.Add(key);
    }

    private void SendTrade()
    {
        if (_currentTrade.Count <= 0)
            LogM("Nothing To Trade");
        List<string> a = new List<string>();
        a.Add(friends.options[friends.value].text);
        var req = new OpenTradeRequest { OfferedInventoryInstanceIds = _currentTrade, AllowedPlayerIds = a };

        PlayFabClientAPI.OpenTrade(req, response =>
        {
            LogM("Opened Trade");
        }, common_error);
    }

    private void LogM(string msg)
    {
        Debug.Log(msg);
        output.text = msg;
    }

    private void common_error(PlayFabError error)
    { Debug.LogError(error.GenerateErrorReport()); }
}
