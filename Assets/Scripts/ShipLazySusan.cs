using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShipLazySusan : MonoBehaviour
{
    public static ShipLazySusan Instance;
    
    [Header("Configuration")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Transform anchor;
    [SerializeField] private Product[] selection;
    [SerializeField] private TMP_Text shipTitle;
    
    [Header("Buttons To Delay")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button playButton;

    private void Awake()
    { Instance = this; }
    
    public void Start()
    {
        shopButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        shipTitle.text = "LOADING SHIPS...";
        playerData.Initialize(); // Call Once
        playerData.FetchInventory(
            () =>
            {
                selection = new Product[playerData.shipsOwned.Count]; // Size Selection Array to fit all ships owned
                // Instantiate Prefabs into Anchor Parent
                for (int i = 0; i < playerData.shipsOwned.Count; ++i)
                {
                    Product obj = Instantiate(playerData.shipsOwned[i],anchor.position,anchor.rotation,anchor);
                    obj.gameObject.SetActive(false);
                    obj.GetComponent<ShipControl>().enabled = false;
                    selection[i] = obj;
                }
                selection[0].gameObject.SetActive(true);
                shipTitle.text = selection[0].name;
                playerData.selectedShipIndex = 0;
                shopButton.gameObject.SetActive(true);
                playButton.gameObject.SetActive(true);
                MenuPrepper.Instance.UpdateCredits();
            }
            );
    }

    /** Rotates the Ship Anchor
     * Using uint to prevent negative numbers allowing MODULO to reset to 0 rotating left
     */
    public void Rotate(int dir)
    {
        uint direction = (uint)dir; // Required because Unity Inspector does not like unsigned integers
        selection[playerData.selectedShipIndex].gameObject.SetActive(false);
        playerData.selectedShipIndex = (playerData.selectedShipIndex - direction) % (uint)selection.Length;
        selection[playerData.selectedShipIndex].gameObject.SetActive(true);
        shipTitle.text = selection[playerData.selectedShipIndex].productName;
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
        selection = new Product[playerData.shipsOwned.Count]; // Size Selection Array to fit all ships owned
        // Instantiate Prefabs into Anchor Parent
        for (int i = 0; i < playerData.shipsOwned.Count; ++i)
        {
            var obj = Instantiate(playerData.shipsOwned[i],anchor.position,anchor.rotation,anchor);
            obj.gameObject.SetActive(false);
            obj.GetComponent<ShipControl>().enabled = false;
            selection[i] = obj;
        }
        selection[playerData.selectedShipIndex].gameObject.SetActive(true);
        shipTitle.text = selection[playerData.selectedShipIndex].name;
    }
}
