using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShipControl : MonoBehaviour
{
    [Header("Configuration")]
    [Header("Tilt")]
    [SerializeField] private float tiltRange;
    [SerializeField] private float tiltSpeed;

    [Header("Rotary Engines")]
    [SerializeField] private float rotarySpeed;
    
    [Header("Thrust Engine")]
    [SerializeField] private float thrustSpeed;

    [Header("Weapons System")]
    [SerializeField] private Rigidbody projectile;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileVelocity;
    [SerializeField] private float fireRate;
    [SerializeField] private bool automaticWeapons = false;

    private Rigidbody _rigidbody;
    private Transform _transform;
    private float _thrust;
    private float _rotary;
    private float _pitch;
    private float _yaw;
    private float _netFireRate;

    private delegate bool FireType(KeyCode key);

    private FireType _chosenType;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _transform = transform;
        _pitch = 0;
        _yaw = 0;
        _transform.rotation = Quaternion.Euler(-90, 0, 0);

        _chosenType = automaticWeapons ? Input.GetKey : Input.GetKeyDown;
    }

    private void Update()
    {
        MoveAndRotate();
        Shoot();
    }
    
    private void FixedUpdate()
    {
        // Movement
        Vector3 dir = Quaternion.Euler(0,0,_yaw) * new Vector3(0, _thrust * thrustSpeed, 0);
        _rigidbody.AddForce(dir);
    }

    private void MoveAndRotate()
    {
        // Update Forces
        _rotary = Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A));
        _thrust = Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S));
        
        // Ship Tilter
        float tiltDir = Math.Clamp(_thrust, -1, 1);
        _pitch = Mathf.MoveTowards(_pitch, tiltRange * tiltDir, tiltSpeed * Time.deltaTime);

        _yaw -= _rotary * rotarySpeed * Time.deltaTime;
        
        var rot = Quaternion.Euler(0,0,_yaw);
        rot *= Quaternion.Euler(_pitch - 90,0,0);
        _transform.rotation = rot;
    }

    private void Shoot()
    {
        // TODO Fire Rate
        if (_chosenType(KeyCode.Space))
        {
            var rot = Quaternion.Euler(0, 0, _yaw);
            var obj = Instantiate(projectile,firePoint.position,Quaternion.identity);
            Transform objt = obj.transform;
            objt.rotation = rot;
            Vector3 velo = rot * new Vector3(0, projectileVelocity, 0);
            obj.AddRelativeForce(velo,ForceMode.Impulse);
            Destroy(obj.gameObject,2);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Asteroid"))
        {
            GameScript.Instance.EndGame();
            // TODO Particle Effects
            Destroy(gameObject);
        }
    }
}
