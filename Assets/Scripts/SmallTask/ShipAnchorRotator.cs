using System;
using UnityEngine;

public class ShipAnchorRotator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float rotationSpeed = 1f;

    private Transform _theTransform;
    private float rotation = 0f;

    private void Awake()
    {
        _theTransform = transform;
    }

    private void Update()
    {
        rotation += Time.deltaTime * rotationSpeed;
        _theTransform.rotation = Quaternion.Euler(-90,rotation,0);
    }
}
