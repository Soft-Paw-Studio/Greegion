using System;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EffectZone : MonoBehaviour
{
    public EffectType effectType;
    public float intensity = 10f;
    public float duration = 5f; // 效果持续时间，0表示永久，或者由区域持续存在决定
    public bool periodic = false; // 如果是周期性机关（比如大风扇会间歇工作）
    public float periodOn = 3f;  // 开启时间
    public float periodOff = 2f; // 关闭时间

    private bool isActive = true;
    private float timer = 0f;

    [OnInspectorInit(nameof(SetCollider))]
    private BoxCollider col;

    private void SetCollider() => col = GetComponent<BoxCollider>();
    
    private void Update()
    {
        // 如果是周期性机关，则定时切换状态
        if (periodic)
        {
            timer += Time.deltaTime;
            if (isActive && timer >= periodOn)
            {
                isActive = false;
                timer = 0f;
            }
            else if (!isActive && timer >= periodOff)
            {
                isActive = true;
                timer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IEffectReceiver>(out var receiver))
        {
            if (!periodic || (periodic && isActive))
            {
                // 创建效果实例（注意：如果duration为0，可以认为效果持续到离开区域为止）
                EffectInstance instance = new EffectInstance(effectType, intensity, duration);
                //receiver.ApplyEffect(instance);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IEffectReceiver>(out var receiver))
        {
            receiver.RemoveEffect(effectType);
        }
    }

    private void OnDrawGizmos()
    {
        GizmoUtilities.DrawArrowCube(col.bounds.center,col.bounds.size,Vector3.up,Color.cyan,0.1f);
    }
}