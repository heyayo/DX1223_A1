using TMPro;
using UnityEngine;

public class MenuPrepper : MonoBehaviour
{
    public static MenuPrepper Instance;
    
    [SerializeField] private PlayerData data;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] public TMP_Text statusText;
    [SerializeField] private GameObject shopPanel;
    
    private void Awake()
    {
        Instance = this;
        data = PlayerData.RetrieveData();
        UpdateCredits();
        shopPanel.SetActive(false);
    }

    public void UpdateCredits()
    {
        currencyText.text = "XRCredits: " + data.xr_credits;
    }
}