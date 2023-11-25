using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductPanelPopulator : MonoBehaviour
{
    [SerializeField] private PlayerData data;
    
    [Header("Presets")]
    [SerializeField] private Transform origin;
    [SerializeField] private ProductPanel productPanel;

    private void Awake()
    { data = PlayerData.RetrieveData(); }
    
    private void Start()
    {
        int ownedShipsDifference = data.allShipsInGame.Count - data.shipsOwned.Count;
        Product[] unownedShips = new Product[ownedShipsDifference];
        int unownedShipsIterator = 0;
        foreach (var ship in data.allShipsInGame)
        {
            if (data.shipsOwnedIndex.ContainsKey(ship.Key)) // Player Owns the ship
                continue;
            
            // Don't own the ship, Create Product Page for it
            unownedShips[unownedShipsIterator] = ship.Value.GetComponent<Product>();
            ++unownedShipsIterator;
        }

        for (int i = 0; i < unownedShips.Length; ++i)
        {
            int yValue = i % 4;
            var panel = Instantiate(productPanel, origin.position + new Vector3(i * 200,yValue * 200,0), origin.rotation);
            panel.product = unownedShips[i];
            panel.Create();
        }
    }
}
