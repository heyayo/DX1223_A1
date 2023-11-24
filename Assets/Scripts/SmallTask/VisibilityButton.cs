using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisibilityButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField field;
    [SerializeField] private Image image;
    [SerializeField] private Sprite defaultSprite;

    public void OnEnable()
    {
        image.sprite = defaultSprite;
        field.contentType = TMP_InputField.ContentType.Password;
    }

    public void OnDisable()
    {
        image.sprite = defaultSprite;
        field.contentType = TMP_InputField.ContentType.Password;
    }
}
