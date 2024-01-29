using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EconomyModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraderPager : MonoBehaviour
{
    [SerializeField] private TMP_Text output;
    [SerializeField] private TMP_Text tradeText;

    [Header("Inputs")]
    // [SerializeField] private TMP_Dropdown trades;
    [SerializeField] private TMP_Dropdown friends;
    [SerializeField] private TMP_Dropdown items;

    [Header("External")]
    [SerializeField] private FriendsListPanel _friends;

    private List<string> _currentTrade;
    private Dictionary<string, string> _trades;
    private Dictionary<string, string> _itemIndex;

    private PlayerData _data;

    private void Awake()
    {
        output.text = "";
        tradeText.text = "";
        gameObject.SetActive(false);
    }

    public void ButtonCallback()
    {
        _data = PlayerData.RetrieveData();

        _currentTrade = new List<string>();
        _trades = new Dictionary<string, string>();
        _itemIndex = new Dictionary<string, string>();

        // trades.options.Clear();
        friends.options.Clear();
        items.options.Clear();

        FetchInventory();

        var flist = _friends.Friends;
        foreach (var f in flist)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
            data.text = f.Key;
            friends.options.Add(data);
        }

        // FetchActiveTrades();
    }

    public void AddButton()
    {
        if (items.options.Count <= 0) return;
        AddItemToTrade(items.options[items.value].text);
        items.options.RemoveAt(items.value);
    }
    // public void AcceptTradeButton() { AcceptTrade(); }
    public void OpenTradeButton()
    { SendTrade(); }

    private void FetchActiveTrades()
    {
        var req = new GetPlayerTradesRequest{};

        Debug.Log("Fetching Trades");

        PlayFabClientAPI.GetPlayerTrades(req, response =>
        {
            _trades.Clear();
            foreach (var t in response.OpenedTrades)
            {
                if (t.Status != TradeStatus.Open) continue;
                TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
                data.text = t.OfferingPlayerId;
                // trades.options.Add(data);
                _trades.Add(t.OfferingPlayerId,t.TradeId);
                LogM("Fetched Trades");
            }
        }, common_error);
    }

    private void FetchInventory()
    {
        var req = new GetUserInventoryRequest { };

        PlayFabClientAPI.GetUserInventory(req, result =>
        {
            items.options.Clear();
            foreach (var item in result.Inventory)
            {
                TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
                data.text = item.ItemId;
                _itemIndex.Add(item.ItemId,item.ItemInstanceId);
                items.options.Add(data);
            }
        }, common_error);
    }

    private void AddItemToTrade(string key)
    {
        if (_currentTrade.Contains(key))
            return;
        _currentTrade.Add(_itemIndex[key]);
        tradeText.text += key;
    }

    private void SendTrade()
    {
        if (_currentTrade.Count <= 0)
        {
            LogM("Nothing To Trade");
            return;
        }
        if (friends.options.Count <= 0)
        {
            LogM("No Friends To Trade With");
            return;
        }
        var req = new OpenTradeRequest { OfferedInventoryInstanceIds = _currentTrade, AllowedPlayerIds = null, RequestedCatalogItemIds = null };
        foreach (var item in _currentTrade)
        { Debug.Log(item); }

        PlayFabClientAPI.OpenTrade(req, response =>
        {
            LogM("Opened Trade");
            var req = new AcceptTradeRequest { OfferingPlayerId = response.Trade.OfferingPlayerId, TradeId = response.Trade.TradeId};

            PlayFabClientAPI.AcceptTrade(req, response =>
            {
                LogM("Accepted Trade | " + response.Trade.TradeId);
                FetchActiveTrades();
            }, common_error);

        }, common_error);
    }

    // private void AcceptTrade()
    // {
    //     if (trades.options.Count <= 0)
    //     {
    //         LogM("No Trade Selected");
    //         return;
    //     }
    //
    //     string offerer = trades.options[trades.value].text;
    //
    //     var req = new AcceptTradeRequest { OfferingPlayerId = offerer, TradeId = _trades[offerer]};
    //
    //     PlayFabClientAPI.AcceptTrade(req, response =>
    //     {
    //         LogM("Accepted Trade | " + _trades[offerer]);
    //         FetchActiveTrades();
    //     }, common_error);
    // }

    private void FillTradeText()
    {
        tradeText.text = "";
        for (int i = 0; i < _currentTrade.Count; ++i)
        {
            tradeText.text += _currentTrade[i] + '\n';
        }
    }

    private void LogM(string msg)
    {
        Debug.Log(msg);
        output.text = msg;
    }

    private void common_error(PlayFabError error)
    { Debug.LogError(error.GenerateErrorReport()); }
}
