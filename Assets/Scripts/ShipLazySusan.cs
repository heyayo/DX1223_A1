using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class ShipLazySusan : MonoBehaviour
{
    public static ShipLazySusan Instance;
    
    [FormerlySerializedAs("playerData")]
    [Header("Configuration")]
    [SerializeField] private PlayerData data;
    [SerializeField] private Transform anchor;
    [SerializeField] private Product[] selection;
    [SerializeField] private TMP_Text shipTitle;
    [SerializeField] private TMP_Text insufficientFundsWarning;
    [SerializeField] private TMP_Text currencyValue;
    
    [Header("Buttons To Delay")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button purchaseButton;

    private void Awake()
    { Instance = this; }
    
    // TODO Fix Purchase Button And Play Button Swapping Out
    public void Start()
    {
        shopButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        purchaseButton.gameObject.SetActive(false);
        insufficientFundsWarning.gameObject.SetActive(false);
        shipTitle.text = "LOADING SHIPS...";
        data.Initialize(); // Call Once
        data.FetchInventory(
            () =>
            {
                selection = new Product[data.allShipsInGame.Count]; // Size Selection Array to fit all ships owned
                // Instantiate Prefabs into Anchor Parent
                int index = 0;
                foreach (var ship in data.allShipsInGame)
                {
                    Product obj = Instantiate(ship.Value,anchor.position,anchor.rotation,anchor);
                    obj.gameObject.SetActive(false);
                    obj.GetComponent<ShipControl>().enabled = false;
                    selection[index] = obj;
                    ++index;
                }
                selection[0].gameObject.SetActive(true);
                shipTitle.text = selection[0].name;
                data.selectedShipIndex = 0;
                shopButton.gameObject.SetActive(true);
                playButton.gameObject.SetActive(true);
                UpdateCredits();
            }
            );
    }

    /** Rotates the Ship Anchor
     * Using uint to prevent negative numbers allowing MODULO to reset to 0 rotating left
     */
    public void Rotate(int dir)
    {
        uint direction = (uint)dir; // Required because Unity Inspector does not like unsigned integers
        selection[data.selectedShipIndex].gameObject.SetActive(false);
        data.selectedShipIndex = (data.selectedShipIndex - direction) % (uint)selection.Length;
        selection[data.selectedShipIndex].gameObject.SetActive(true);
        shipTitle.text = selection[data.selectedShipIndex].productName;

        bool owned = data.shipsOwnedIndex.ContainsKey(selection[data.selectedShipIndex].productID); 
        purchaseButton.gameObject.SetActive(!owned);
        playButton.gameObject.SetActive(owned);
    }

    public void ChangeShipPreviewVisibility(bool visibility)
    { anchor.gameObject.SetActive(visibility); }

    public void ReSeatShips()
    {
        if (selection != null)
        {
            foreach (var ship in selection)
                Destroy(ship);
        }
        selection = new Product[data.shipsOwned.Count]; // Size Selection Array to fit all ships owned
        // Instantiate Prefabs into Anchor Parent
        for (int i = 0; i < data.shipsOwned.Count; ++i)
        {
            var obj = Instantiate(data.shipsOwned[i],anchor.position,anchor.rotation,anchor);
            obj.gameObject.SetActive(false);
            obj.GetComponent<ShipControl>().enabled = false;
            selection[i] = obj;
        }
        selection[data.selectedShipIndex].gameObject.SetActive(true);
        shipTitle.text = selection[data.selectedShipIndex].name;
    }
    
    public void BuyProduct()
    {
        var product = selection[data.selectedShipIndex];
        if (data.shipsOwnedIndex.ContainsKey(product.productID)) return;
        purchaseButton.enabled = false;
        shipTitle.text = "PENDING";
        var req = new PurchaseItemRequest
        {
            ItemId = product.productID,
            VirtualCurrency = "XD", // XRCredits
            Price = 120 // TODO FIX HARD CODED PRICE
        };
        
        PlayFabClientAPI.PurchaseItem(req,
            success =>
            {
                foreach (var ship in success.Items)
                    data.AddShip(ship.ItemId);
                shipTitle.text = product.productName;
                purchaseButton.enabled = true;
                purchaseButton.gameObject.SetActive(false);
                playButton.gameObject.SetActive(true);
                data.FetchInventory(()=>
                {
                    UpdateCredits();
                    ShipLazySusan.Instance.ReSeatShips();
                });
            }, failure =>
            {
                Debug.LogError(failure.GenerateErrorReport());
                purchaseButton.enabled = true;
                shipTitle.text = product.productName;
                switch (failure.Error)
                {
                    case PlayFabErrorCode.InsufficientFunds:
                    {
                        StartCoroutine(DisplayWarning());
                        break;
                    }
                }
            }
            );
    }

    private IEnumerator DisplayWarning()
    {
        insufficientFundsWarning.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        insufficientFundsWarning.gameObject.SetActive(false);
    }

    private void UpdateCredits()
    {
        currencyValue.text = "XRCredits: " + data.xr_credits;
    }
}
