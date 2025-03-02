using System;
using UnityEngine;

public class BounceZone : MonoBehaviour
{
    public float bounceForce;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody rigid))
        {
            Debug.Log("BOUNCE");
            Vector3 bounceDirection = Vector3.up;
            rigid.linearVelocity = Vector3.zero;
            rigid.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
        }
    }
}
