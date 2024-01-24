using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using EntityKey = PlayFab.GroupsModels.EntityKey;

[Serializable]
public struct PlayStatistics
{
    public double stat_minutesPlayed;
    public int stat_matchesPlayed;
    public int stat_xp;
    public int stat_level;
    public int stat_prestige;
}

[Serializable]
public struct Achievements
{
    public bool achievement_afk;
    public bool achievement_wave30;
    public bool achievement_allShips;
    public bool achievement_maxLevel;
    public bool achievement_prestiged;
    public bool achievement_breenseerayyang; // Secret Achievement
}

[CreateAssetMenu(menuName = "Static Data/Player Data", fileName = "Player Data")]
public class PlayerData : ScriptableObject
{
    private static readonly string _defaultShipID = "ship_default";
    private static readonly string _playStatisticsKey = "statistics";
    private static readonly string _achievementsKey = "achievements";
    private static readonly string _xrCreditsKey = "XD";
    
    [Header("All Ships In The Game")]
    public Dictionary<string, Product> allShipsInGame = new Dictionary<string, Product>(); // All The Ships in Game tied by ItemID
    public Dictionary<string, Product> allPurchasableItems = new Dictionary<string, Product>();
    
    [Header("Default Ship")]
    [SerializeField] private Product defaultShip;
    
    [Header("Player's Inventory")]
    public Dictionary<string, Product> shipsOwnedIndex = new Dictionary<string, Product>(); // All Ships player owned index-able by ItemID
    public List<Product> shipsOwned = new List<Product>(); // Raw List of ships owned
    public int xr_credits = 0;
    public uint selectedShipIndex = 0;
    public Product selectedShip;
    public string displayName;
    public DateTime timeCreated;

    [Header("Player's General Statistics")]
    public PlayStatistics playStatistics = new PlayStatistics();
    public Achievements achievements = new Achievements();
    public bool isGuest = false;
    public string token;
    public string entityID;
    public EntityKey currentGroup;
    public EntityKey entityKey;

    public static PlayerData RetrieveData()
    { return Resources.Load<PlayerData>("Inventory"); }
    
    public void Initialize(Action action)
    {
        allShipsInGame.Clear();
        allPurchasableItems.Clear();
        allShipsInGame.Add(_defaultShipID,defaultShip);

        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(),
            success =>
            {
                foreach (var item in success.Catalog)
                {
                    Product product = Resources.Load<Product>("Products/" + item.ItemId);
                    product.productName = item.DisplayName;
                    product.productPrice = item.VirtualCurrencyPrices["XD"];
                    allPurchasableItems.Add(item.ItemId,product);
                    if (item.ItemClass == "ship")
                    {
                        allShipsInGame.Add(item.ItemId,product);
                        product.productTag = Product.PRODUCT_TAG.SHIP;
                    }
                }
                action();
            },
            failure =>
            { Debug.LogError(failure.GenerateErrorReport()); }
            );
        
        xr_credits = 0;
        
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            success =>
            {
                if (success.AccountInfo.TitleInfo.DisplayName != null)
                    displayName = success.AccountInfo.TitleInfo.DisplayName;
                timeCreated = success.AccountInfo.Created;
                entityID = success.AccountInfo.PlayFabId;
            },
            FailCode
            );
    }

    public void AddShip(string shipID)
    {
        if (allShipsInGame.ContainsKey(shipID))
            shipsOwnedIndex.Add(shipID,allShipsInGame[shipID]);
        else
            Debug.LogError("Attempting to Add Invalid Ship");
    }
    
    public void FetchInventory(Action action)
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            success =>
            {
                // Clear Existing Ships
                shipsOwnedIndex.Clear();
                shipsOwned.Clear();
                
                // Give Player Default Ship
                shipsOwnedIndex.Add(_defaultShipID,defaultShip);
                shipsOwned.Add(defaultShip);
                
                var inv = success.Inventory;
                foreach (var item in inv)
                {
                    // Add to Dictionary and List
                    shipsOwnedIndex.Add(item.ItemId,allShipsInGame[item.ItemId]);
                    shipsOwned.Add(allShipsInGame[item.ItemId]);
                }

                xr_credits = success.VirtualCurrency["XD"];
                
                // Check All Ships for Achievement
                achievements.achievement_allShips = true;
                foreach (var ship in allShipsInGame)
                {
                    if (!shipsOwnedIndex.ContainsKey(ship.Key))
                        achievements.achievement_allShips = false;
                }
                if (achievements.achievement_allShips) Debug.Log("ALL SHIPS ACHIEVEMENT");
                action();
            },
            FailCode
            );
    }

    public void UploadStats()
    {
        if (playStatistics.stat_level > 50)
            playStatistics.stat_level = 50;
        string pStats = JsonUtility.ToJson(playStatistics);
        string ach = JsonUtility.ToJson(achievements);
        
        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {_playStatisticsKey,pStats},
                    {_achievementsKey,ach}
                }
            },
            success =>
            { Debug.Log("Uploaded Player Data"); },
            FailCode
            );
    }

    public void FetchStats(Action action)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            success =>
            {
                Debug.Log("Fetched Player Data");
                if (success.Data.ContainsKey(_playStatisticsKey))
                    playStatistics = JsonUtility.FromJson<PlayStatistics>(success.Data[_playStatisticsKey].Value);
                else
                {
                    playStatistics.stat_matchesPlayed = 0;
                    playStatistics.stat_minutesPlayed = 0;
                    playStatistics.stat_level = 0;
                    playStatistics.stat_xp = 0;
                    playStatistics.stat_prestige = 0;
                }
                if (success.Data.ContainsKey(_achievementsKey))
                    achievements = JsonUtility.FromJson<Achievements>(success.Data[_achievementsKey].Value);
                else
                {
                    achievements.achievement_afk = false;
                    achievements.achievement_wave30 = false;
                    achievements.achievement_allShips = false;
                    achievements.achievement_maxLevel = false;
                    achievements.achievement_prestiged = false;
                    achievements.achievement_breenseerayyang = false;
                }

                action();
            },
            FailCode
            );
    }

    public void GiveCurrency(int amount)
    {
        PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = _xrCreditsKey,
            Amount = amount
        },
            success =>
            {
                Debug.Log("New Balance: " + success.Balance + success.VirtualCurrency);
                xr_credits = success.Balance;
            },
        failure => { Debug.LogError(failure.GenerateErrorReport());});
    }

    public void TakeCurrency(int amount)
    {
        PlayFabClientAPI.SubtractUserVirtualCurrency(new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = _xrCreditsKey,
            Amount = amount
        },
        success =>
        {
            Debug.Log("New Balance: " + success.Balance + success.VirtualCurrency);
            xr_credits = success.Balance;
        },
        failure => { Debug.LogError(failure.GenerateErrorReport());});
    }

    private void FailCode(PlayFabError error)
    { Debug.LogError(error.GenerateErrorReport()); }
}
