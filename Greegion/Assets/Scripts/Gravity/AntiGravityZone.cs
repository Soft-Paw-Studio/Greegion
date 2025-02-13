using System;
using Pigeon;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class AntiGravityZone : MonoBehaviour
{
    public float antiGravityForce = 9.81f; // 反重力大小，默认等于地球重力
    public bool overrideMaxFallSpeed = true; // 是否忽略最大下落速度限制

    [OnInspectorInit("Awake")]
    private Collider collider;

    private void Awake()
    {
        TryGetComponent(out collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PigeonController>(out var pigeon))
        {
            GravityManager.Instance.SetAntiGravity(pigeon, antiGravityForce, overrideMaxFallSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PigeonController>(out var pigeon))
        {
            GravityManager.Instance.ResetGravity(pigeon);
        }
    }

    private void OnDrawGizmos()
    {
        if (collider == null)
        {
            TryGetComponent(out collider);
        }
        
        Gizmos.color = new Color(1,0.5f,0,0.1f);
        Gizmos.DrawCube(collider.bounds.center,collider.bounds.size);
        Gizmos.color = new Color(1,0.5f,0,1);
        Gizmos.DrawWireCube(collider.bounds.center,collider.bounds.size);

        Handles.color = new Color(1,0.5f,0,0.5f);
        Handles.ArrowHandleCap(0,new Vector3(collider.bounds.center.x,collider.bounds.center.y - 0.5f,collider.bounds.center.z),Quaternion.LookRotation(Vector3.up), 1f,EventType.Repaint);
    }
}