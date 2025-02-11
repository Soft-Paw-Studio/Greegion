using Pigeon;
using UnityEngine;

public class AntiGravityZone : MonoBehaviour
{
    public float antiGravityForce = 9.81f; // 反重力大小，默认等于地球重力
    public bool overrideMaxFallSpeed = true; // 是否忽略最大下落速度限制
    
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
}