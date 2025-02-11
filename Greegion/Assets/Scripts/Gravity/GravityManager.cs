using System.Collections.Generic;
using Pigeon;
using UnityEngine;

public class GravityManager : Singleton<GravityManager>
{
    public float defaultGravity = -9.81f; // 默认重力
    public float maxFallSpeed = -10f;     // 限制最大下落速度
    public float groundedGravity = -2f;   // 站在地面时的小重力
    
    private Dictionary<PigeonController, float> antiGravityForces = new(); // 存储每个角色的重力状态

    
    public float GetGravityEffect(PigeonController pigeon, float currentVelocity, bool isGrounded)
    {
        float gravity = antiGravityForces.ContainsKey(pigeon) ? antiGravityForces[pigeon] : defaultGravity;
        
        if (isGrounded && currentVelocity < 0)
        {
            return groundedGravity; // 避免角色漂浮在地面
        }

        currentVelocity += gravity * Time.deltaTime;
        return Mathf.Max(currentVelocity, maxFallSpeed);
    }
    
    public void SetAntiGravity(PigeonController pigeon, float antiGravity, bool ignoreMaxFallSpeed)
    {
        antiGravityForces[pigeon] = antiGravity;
        if (ignoreMaxFallSpeed)
        {
            maxFallSpeed = 0; // 允许角色无限上升
        }
    }

    public void ResetGravity(PigeonController pigeon)
    {
        if (antiGravityForces.ContainsKey(pigeon))
        {
            antiGravityForces.Remove(pigeon);
        }
        maxFallSpeed = -10f; // 恢复正常最大下落速度
    }
}