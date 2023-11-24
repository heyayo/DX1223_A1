using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

[CreateAssetMenu(menuName = "Static Data/Player Data", fileName = "Player Data")]
public class PlayerData : ScriptableObject
{
    private static readonly string _defaultShipID = "ship_default";
    private static readonly string _hoursPlayedKey = "stat_hours";
    private static readonly string _minutesPlayedKey = "stat_minutes";
    private static readonly string _matchesPlayedKey = "stat_matches";
    
    [Header("All Ships In The Game")]
    [SerializeField] private List<string> allShipIDs = new List<string>();
    [SerializeField] private List<GameObject> allShipPrefabs = new List<GameObject>();
    public Dictionary<string, GameObject> allShipsInGame = new Dictionary<string, GameObject>(); // All The Ships in Game tied by ItemID
    
    [Header("Default Ship")]
    [SerializeField] private GameObject defaultShip;
    
    [Header("Player's Inventory")]
    public Dictionary<string, GameObject> shipsOwnedIndex = new Dictionary<string, GameObject>(); // All Ships player owned index-able by ItemID
    public List<GameObject> shipsOwned = new List<GameObject>(); // Raw List of ships owned

    [Header("Player's General Statistics")]
    public int hoursPlayed;
    public int minutesPlayed;
    public int matchesPlayed;
    
    public void Initialize()
    {
        // Debug if Statement
        if (allShipIDs.Count != allShipPrefabs.Count)
        {
            Debug.LogError("Mismatched Number of Ship IDs to Prefabs");
            Debug.Break();
        }

        allShipsInGame.Clear();
        allShipsInGame.Add(_defaultShipID,defaultShip);
        for (int i = 0; i < allShipPrefabs.Count; ++i)
            allShipsInGame.Add(allShipIDs[i],allShipPrefabs[i]);
    }
    
    public void FetchInventory(Action action)
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            success =>
            {
                // Clear Existing Ships
                shipsOwnedIndex.Clear();
                shipsOwned.Clear();
                
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
            failure =>
            {
                Debug.Log(failure.GenerateErrorReport());
            }
            );
    }

    public void UploadPlayerData()
    {
        Dictionary<string, string> skill = new Dictionary<string, string>();
        
        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {_hoursPlayedKey,hoursPlayed.ToString()},
                    {_minutesPlayedKey,minutesPlayed.ToString()},
                    {_matchesPlayedKey,matchesPlayed.ToString()}
                }
            },
            success =>
            { Debug.Log("Uploaded Player Data"); },
            failure =>
            { Debug.Log(failure.GenerateErrorReport()); }
            );
    }

    public void FetchPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            success =>
            {
                hoursPlayed = Int32.Parse(success.Data[_hoursPlayedKey].Value);
                minutesPlayed = Int32.Parse(success.Data[_minutesPlayedKey].Value);
                matchesPlayed = Int32.Parse(success.Data[_matchesPlayedKey].Value);
            },
            failure =>
            { Debug.Log(failure.GenerateErrorReport()); }
            );
    }
}
