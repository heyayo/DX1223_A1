using UnityEngine;
using TMPro;

public class ShipLazySusan : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Transform anchor;
    [SerializeField] private GameObject[] selection;
    [SerializeField] private TMP_Text shipTitle;

    public void Start()
    {
        shipTitle.text = "LOADING SHIPS...";
        playerData.Initialize(); // Call Once
        playerData.FetchInventory(
            () =>
            {
                selection = new GameObject[playerData.shipsOwned.Count]; // Size Selection Array to fit all ships owned
                // Instantiate Prefabs into Anchor Parent
                for (int i = 0; i < playerData.shipsOwned.Count; ++i)
                {
                    GameObject obj = Instantiate(playerData.shipsOwned[i],anchor.position,anchor.rotation,anchor).gameObject;
                    obj.SetActive(false);
                    obj.GetComponent<ShipControl>().enabled = false;
                    selection[i] = obj;
                }
                selection[0].SetActive(true);
                shipTitle.text = selection[0].name;
                playerData.selectedShipIndex = 0;
            }
            );
    }

    /** Rotates the Ship Anchor
     * Using uint to prevent negative numbers allowing MODULO to reset to 0 rotating left
     */
    public void Rotate(int dir)
    {
        uint direction = (uint)dir; // Required because Unity Inspector does not like unsigned integers
        selection[playerData.selectedShipIndex].SetActive(false);
        playerData.selectedShipIndex = (playerData.selectedShipIndex - direction) % (uint)selection.Length;
        selection[playerData.selectedShipIndex].SetActive(true);
    }

    public void ChangeShipPreviewVisibility(bool visibility)
    { anchor.gameObject.SetActive(visibility); }
}
