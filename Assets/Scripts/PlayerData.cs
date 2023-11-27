using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

[Serializable]
public struct PlayStatistics
{
    public float stat_minutesPlayed;
    public int stat_matchesPlayed;
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
    private static readonly string _playStatisticsKey = "playtime";
    private static readonly string _achievementsKey = "achievements";
    
    [Header("All Ships In The Game")]
    [SerializeField] private List<Product> allShipsInGameList = new List<Product>();
    public Dictionary<string, Product> allShipsInGame = new Dictionary<string, Product>(); // All The Ships in Game tied by ItemID
    
    [Header("Default Ship")]
    [SerializeField] private Product defaultShip;
    
    [Header("Player's Inventory")]
    public Dictionary<string, Product> shipsOwnedIndex = new Dictionary<string, Product>(); // All Ships player owned index-able by ItemID
    public List<Product> shipsOwned = new List<Product>(); // Raw List of ships owned
    public uint selectedShipIndex = 0;

    [Header("Player's General Statistics")]
    public PlayStatistics playStatistics = new PlayStatistics();
    public Achievements achievements = new Achievements();

    public static PlayerData RetrieveData()
    { return Resources.Load<PlayerData>("Inventory"); }
    
    public void Initialize()
    {
        allShipsInGame.Clear();
        allShipsInGame.Add(_defaultShipID,defaultShip);
        for (int i = 0; i < allShipsInGameList.Count; ++i)
            allShipsInGame.Add(allShipsInGameList[i].productID,allShipsInGameList[i]);
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
                action();
            },
            FailCode
            );
    }

    public void UploadStats()
    {
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

    public void FetchStats()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            success =>
            {
                Debug.Log("Fetched Player Data");
                playStatistics = JsonUtility.FromJson<PlayStatistics>(success.Data[_playStatisticsKey].Value);
                achievements = JsonUtility.FromJson<Achievements>(success.Data[_achievementsKey].Value);
            },
            FailCode
            );
    }

    private void FailCode(PlayFabError error)
    { Debug.LogError(error.GenerateErrorReport()); }
}
