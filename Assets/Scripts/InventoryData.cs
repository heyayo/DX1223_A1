using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

[CreateAssetMenu(menuName = "Static Data/Inventory", fileName = "Inventory")]
public class InventoryData : ScriptableObject
{
    [Header("All Ships In The Game")]
    [SerializeField] private List<string> allShipIDs;
    [SerializeField] private List<GameObject> allShipPrefabs;
    public Dictionary<string, GameObject> allShipsInGame; // All The Ships in Game tied by ItemID
    
    [Header("Player's Inventory")]
    public Dictionary<string, GameObject> shipsOwnedIndex; // All Ships player owned index-able by ItemID
    public List<GameObject> shipsOwned; // Raw List of ships owned
    
    public void Initialize()
    {
        // Debug if Statement
        if (allShipIDs.Count != allShipPrefabs.Count)
        {
            Debug.LogError("Mismatched Number of Ship IDs to Prefabs");
            Debug.Break();
        }

        allShipsInGame.Clear();
        for (int i = 0; i < allShipPrefabs.Count; ++i)
            allShipsInGame.Add(allShipIDs[i],allShipPrefabs[i]);
    }
    
    public void FetchInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            success =>
            {
                // Clear Existing Ships
                shipsOwnedIndex.Clear();
                shipsOwned.Clear();
                
                var inv = success.Inventory;
                foreach (var item in inv)
                {
                    // Add to Dictionary and List
                    shipsOwnedIndex.Add(item.ItemId,allShipsInGame[item.ItemId]);
                    shipsOwned.Add(allShipsInGame[item.ItemId]);
                }
            },
            failure =>
            {
                Debug.Log(failure.GenerateErrorReport());
            }
            );
    }
}
