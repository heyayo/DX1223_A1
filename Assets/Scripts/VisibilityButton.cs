using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisibilityButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite defaultSprite;
    
    public void OnEnable()
    { image.sprite = defaultSprite; }
}
