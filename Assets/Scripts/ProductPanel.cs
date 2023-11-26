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
    public Product product;

    private void Awake()
    { data = PlayerData.RetrieveData(); }

    public void Create()
    {
        productTitle.text = product.productName;
        productPreview.sprite = product.productImage;
        purchaseButton.onClick.AddListener(BuyProduct);
    }
    
    public void BuyProduct()
    {
        var req = new PurchaseItemRequest
        {
            ItemId = product.productID,
            VirtualCurrency = "XD" // XRCredits
        };
        
        PlayFabClientAPI.PurchaseItem(req,
            success =>
            {
                foreach (var ship in success.Items)
                    data.AddShip(ship.ItemId);
            }, FailCode
            );
    }

    private void FailCode(PlayFabError error)
    { Debug.LogError(error.GenerateErrorReport()); }
}
