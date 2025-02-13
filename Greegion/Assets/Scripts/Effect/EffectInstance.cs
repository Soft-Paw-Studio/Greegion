using System;
using UnityEngine;

public class EffectInstance
{
    public EffectType EffectType { get; private set; }
    public float Intensity { get; private set; }
    public float Duration { get; private set; } // 剩余时间，0 表示永久效果
    public event Action<EffectInstance> OnEffectEnd;

    public EffectInstance(EffectType effectType, float intensity, float duration)
    {
        EffectType = effectType;
        Intensity = intensity;
        Duration = duration;
    }

    // 调用此方法以更新效果剩余时间
    public void UpdateEffect(float deltaTime)
    {
        if (Duration > 0)
        {
            Duration -= deltaTime;
            if (Duration <= 0)
            {
                Duration = 0;
                OnEffectEnd?.Invoke(this);
            }
        }
    }
}