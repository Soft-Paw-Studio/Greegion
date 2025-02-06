using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class PegionBellyController : MonoBehaviour
{
    [Range(0, 1)] public float size = 0.5f;

    [ReadOnly] public Vector3 bellyPosition;
    public Vector3 bellyOffset;

    private Vector3 velocity;
    private Vector3 acceleration;

    public float elasticity = 5.0f; // 弹性系数（越大回弹越快）
    private float damping = 0.8f; // 初始阻尼
    public float dampingIncrease = 2.0f; // 阻尼增长速率（每秒增加的阻尼值）
    public float dampingMax = 2.0f;
    
    private static readonly int BellyPosition = Shader.PropertyToID("_BellyPosition");
    private static readonly int BellyOffset = Shader.PropertyToID("_BellyOffset");
    private static readonly int Size = Shader.PropertyToID("_Size");

    void Update()
    {
        Shader.SetGlobalFloat(Size, size);

        
        if(!Application.isPlaying) return;
        
        // 计算弹簧力（Hooke’s Law: F = -kX）
        Vector3 displacement = bellyPosition - transform.position;
        Vector3 springForce = -elasticity * displacement;

        // 计算阻尼力
        Vector3 dampingForce = -damping * velocity;

        // 计算总加速度
        acceleration = springForce + dampingForce;

        // 更新速度
        velocity += acceleration * Time.deltaTime;

        // 更新肚子的位置
        bellyPosition += velocity * Time.deltaTime;

        // **动态增加阻尼，使振幅更快衰减**
        damping += dampingIncrease * Time.deltaTime;
        damping = Mathf.Clamp(damping, 0.8f, dampingMax); // 限制阻尼最大值，防止过大导致硬直

        Shader.SetGlobalVector(BellyPosition, bellyPosition);
        Shader.SetGlobalVector(BellyOffset, bellyOffset);
    }
}