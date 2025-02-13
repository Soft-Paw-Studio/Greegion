using System.Collections.Generic;
using UnityEngine;
using System;

public class EffectController : MonoBehaviour, IEffectReceiver
{
    // 当前激活的效果，按 EffectType 分类（支持同一类型多个效果也可以设计合并规则）
    private Dictionary<EffectType, List<EffectInstance>> activeEffects = new Dictionary<EffectType, List<EffectInstance>>();

    // 全局事件，可用于通知其他系统效果的变化
    public event Action<EffectInstance> OnEffectApplied;
    public event Action<EffectInstance> OnEffectRemoved;

    private void Update()
    {
        // 更新所有效果的剩余时间
        foreach (var kvp in activeEffects)
        {
            // 复制一份列表防止迭代期间删除
            List<EffectInstance> effects = new List<EffectInstance>(kvp.Value);
            foreach (var effect in effects)
            {
                effect.UpdateEffect(Time.deltaTime);
                if (effect.Duration <= 0)
                {
                    RemoveEffect(effect.EffectType);
                }
            }
        }
    }

    public void ApplyEffect(EffectInstance effect)
    {
        if (!activeEffects.ContainsKey(effect.EffectType))
            activeEffects[effect.EffectType] = new List<EffectInstance>();

        activeEffects[effect.EffectType].Add(effect);
        effect.OnEffectEnd += Effect_OnEffectEnd;

        OnEffectApplied?.Invoke(effect);
        // 可在这里调用具体的效果处理逻辑，比如：
        ProcessEffect(effect, true);
    }

    public void ApplyEffect(EffectType effectType, float intensity, float duration)
    {
        
    }

    public void RemoveEffect(EffectType effectType)
    {
        if (activeEffects.ContainsKey(effectType))
        {
            // 这里我们简单移除所有该类型效果
            foreach (var effect in activeEffects[effectType])
            {
                effect.OnEffectEnd -= Effect_OnEffectEnd;
                ProcessEffect(effect, false);
                OnEffectRemoved?.Invoke(effect);
            }
            activeEffects.Remove(effectType);
        }
    }

    private void Effect_OnEffectEnd(EffectInstance effect)
    {
        RemoveEffect(effect.EffectType);
    }

    /// <summary>
    /// 处理效果的具体逻辑。apply 为 true 表示应用效果，false 表示取消效果。
    /// 此处可以根据 EffectType 分发到不同的逻辑处理，比如改变重力、修改速度、加伤害等等
    /// </summary>
    private void ProcessEffect(EffectInstance effect, bool apply)
    {
        // 这里仅举例：比如反重力效果
        if (effect.EffectType == EffectType.AntiGravity)
        {
            // 例如调用 GravityManager 来设置或重置反重力
            //if (apply)
                //GravityManager.Instance.SetAntiGravity(GetComponent<PigeonController>(), effect.Intensity, false);
            //else
                //GravityManager.Instance.ResetGravity(GetComponent<PigeonController>());
        }
        // 针对其他效果，可以加入类似逻辑
        else if (effect.EffectType == EffectType.WindForce)
        {
            // WindForce 可能会通过一个全局风力系统来对单位施加一个方向性推力
            // 具体逻辑由你决定：例如在 PigeonController 的 Update 里读取 windForce 效果的强度来加力
        }
        else if (effect.EffectType == EffectType.Slippery)
        {
            // 结冰地板效果可以降低角色的摩擦或调整移动平滑参数
            // 例如修改角色移动控制器的摩擦系数
        }
        // 其他效果……
    }
}
