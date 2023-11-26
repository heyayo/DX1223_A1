using UnityEngine;

public class Product : MonoBehaviour
{
    public string productName;
    public string productID;
    public Sprite productImage;

    // Allows Editor to see Product ID as Prefab Name but exist in game as ProductName
    private void Awake()
    { name = productName; }
}
