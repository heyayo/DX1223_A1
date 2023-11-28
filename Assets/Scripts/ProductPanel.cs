using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class ProductPanel : MonoBehaviour
{
    [SerializeField] private PlayerData data;
    [SerializeField] private TMP_Text productTitle;
    [SerializeField] private Image productPreview;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text purchaseText;
    [SerializeField] private TMP_Text statusText;
    public Product product;

    private void Awake()
    { data = PlayerData.RetrieveData(); }

    public void Start()
    {
        productTitle.text = product.productName;
        productPreview.sprite = product.productImage;
        purchaseButton.onClick.AddListener(BuyProduct);
        if (data.shipsOwnedIndex.ContainsKey(product.productID))
        {
            purchaseText.text = "OWNED";
            purchaseButton.enabled = false;
        }
        else
            purchaseText.text = "PURCHASE";

        statusText = MenuPrepper.Instance.statusText;
    }

    public void BuyProduct()
    {
        if (data.shipsOwnedIndex.ContainsKey(product.productID)) return;
        purchaseButton.enabled = false;
        purchaseText.text = "PENDING";
        var req = new PurchaseItemRequest
        {
            ItemId = product.productID,
            VirtualCurrency = "XD", // XRCredits
            Price = 120
        };
        
        PlayFabClientAPI.PurchaseItem(req,
            success =>
            {
                foreach (var ship in success.Items)
                    data.AddShip(ship.ItemId);
                purchaseText.text = "OWNED";
                purchaseButton.enabled = false;
                data.FetchInventory(()=>
                {
                    MenuPrepper.Instance.UpdateCredits();
                    ShipLazySusan.Instance.ReSeatShips();
                });
            }, FailCode
            );
    }

    private void FailCode(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
        purchaseButton.enabled = true;
        purchaseText.text = "PURCHASE";
        switch (error.Error)
        {
            case PlayFabErrorCode.InsufficientFunds:
            {
                statusText.text = "Insufficient Funds";
                break;
            }
        }
    }
}
