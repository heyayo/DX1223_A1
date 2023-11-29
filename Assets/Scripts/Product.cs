using UnityEngine;
using UnityEngine.Serialization;

public class Product : MonoBehaviour
{
    public enum PRODUCT_TAG
    {
        SHIP,
        GENERIC
    }
    
    public string productName;
    public string productID;
    public uint productPrice;
    public PRODUCT_TAG productTag = PRODUCT_TAG.GENERIC;

    // Allows Editor to see Product ID as Prefab Name but exist in game as ProductName
    private void Awake()
    { name = productName; }
}
