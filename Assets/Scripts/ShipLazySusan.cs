using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Serialization;

public class ShipLazySusan : MonoBehaviour
{
    [FormerlySerializedAs("inventory")]
    [Header("Configuration")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Transform anchor;
    [SerializeField] private GameObject[] selection;
    [SerializeField] private uint currentlySelected;

    public void Start()
    {
        playerData.Initialize(); // Call Once
        playerData.FetchInventory(
            () =>
            {
                selection = new GameObject[playerData.shipsOwned.Count]; // Size Selection Array to fit all ships owned
                // Instantiate Prefabs into Anchor Parent
                for (int i = 0; i < playerData.shipsOwned.Count; ++i)
                {
                    GameObject obj = Instantiate(playerData.shipsOwned[i],anchor.position,anchor.rotation,anchor);
                    obj.SetActive(false);
                    obj.GetComponent<PlayerControl>().enabled = false;
                    selection[i] = obj;
                }
                selection[0].SetActive(true);
            }
            );
    }

    /** Rotates the Ship Anchor
     * Using uint to prevent negative numbers allowing MODULO to reset to 0 rotating left
     */
    public void Rotate(int dir)
    {
        uint direction = (uint)dir; // Required because Unity Inspector does not like unsigned integers
        selection[currentlySelected].SetActive(false);
        currentlySelected = (currentlySelected - direction) % (uint)selection.Length;
        selection[currentlySelected].SetActive(true);
    }
}
