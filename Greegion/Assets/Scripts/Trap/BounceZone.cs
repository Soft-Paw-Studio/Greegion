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
            rigid.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
        }

        if (other.TryGetComponent(out CharacterControllerWithForce cha))
        {
            Debug.Log("BOUNCE");
            Vector3 bounceDirection = Vector3.up;
            cha.AddForce(bounceDirection * bounceForce,ForceMode.Impulse);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("really?");
        if (hit.gameObject.TryGetComponent(out CharacterControllerWithForce cha))
        {
            Debug.Log("BOUNCE");
            Vector3 bounceDirection = Vector3.up;
            cha.AddForce(bounceDirection * bounceForce,ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Rigidbody rigid))
        {
            Debug.Log("BOUNCE");
            Vector3 bounceDirection = Vector3.up;
            rigid.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
        }

        if (other.gameObject.TryGetComponent(out CharacterControllerWithForce cha))
        {
            Debug.Log("BOUNCE");
            Vector3 bounceDirection = Vector3.up;
            cha.AddForce(bounceDirection * bounceForce,ForceMode.Impulse);
        }
    }
}
