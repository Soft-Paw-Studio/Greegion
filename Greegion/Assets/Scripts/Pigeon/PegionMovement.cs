using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 控制鸽子的移动、跳跃和肚子物理效果的主要类
/// </summary>
public class PegionMovement : Singleton<PegionMovement>
{
    //Parameters
    [Header("Movement Parameters")] 
    [SerializeField][Range(0, 5)]
    private float speed = 3f;
    [SerializeField][Range(0, 1)]
    private float smoothTime = 0.1f;

    [Header("Jump Parameters")] 
    [SerializeField][Range(0, 5)]
    private float jumpHeight = 2f;
    [SerializeField][Range(0, 9.81f)]
    private float gravity = 9.81f;

    [Header("Belly System")] 
    [OnValueChanged(nameof(AdjustSize))] 
    [ProgressBar(0,1,r:0.85f,g:0.34f,b:0.13f,Height = 20)]
    [SerializeField]
    private float size = 0f;
    [SerializeField][Range(0,200)]
    private float elasticity = 5.0f;
    [SerializeField][Range(0,10)]
    private float damping = 0.8f;

    //Store Values
    private float currentSpeed;
    private float currentJumpHeight;
    private float verticalVelocity;
    
    private Vector3 moveDirection;
    private Vector3 currentMovement;
    private Vector3 moveVectorRef;
    private Vector3 bellyPosition;
    private Vector3 velocity;
    private Vector3 acceleration;

    private CharacterController controller;
    private PegionActions input;
    private Camera mainCamera;

    private static readonly int ShaderBellyPosition = Shader.PropertyToID("_BellyPosition");
    private static readonly int ShaderSize = Shader.PropertyToID("_Size");
    
    //Events
    private UnityAction<float> PigeonGetWeight;

    /// <summary>
    /// 初始化输入系统并注册跳跃事件
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        input = new PegionActions();
        input.Enable();
        input.Gameplay.Jump.performed += _ => OnJump();
        PigeonGetWeight += OnPigeonGetWeight;
    }

    /// <summary>
    /// 初始化相机引用、角色控制器和肚子位置
    /// </summary>
    private void Start()
    {
        mainCamera = Camera.main;
        TryGetComponent(out controller);
        bellyPosition = transform.position;
    }

    /// <summary>
    /// 每帧更新处理移动、物理和动画
    /// </summary>
    private void Update()
    {
        HandleMovementInput();
        MoveCharacter();
        ApplyGravity();
        BellyPhysics();
    }

    /// <summary>
    /// 处理收集物品的碰撞检测，增加体重
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IFood food))
        {
            food.Collect();
            PigeonGetWeight.Invoke(food.GetCalories());
        }
    }

    /// <summary>
    /// 读取并处理玩家的移动输入
    /// </summary>
    private void HandleMovementInput()
    {
        var moveInput = input.Gameplay.Movement.ReadValue<Vector2>();
        moveDirection = moveInput.sqrMagnitude > 0.01f
            ? GetMovementDirection(moveInput.normalized)
            : Vector3.zero;
    }

    /// <summary>
    /// 基于相机方向将2D输入转换为3D移动方向
    /// </summary>
    /// <param name="input">标准化的2D输入向量</param>
    /// <returns>相对于相机的3D移动方向</returns>
    private Vector3 GetMovementDirection(Vector2 input)
    {
        var camForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        var camRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
        return (camRight * input.x + camForward * input.y).normalized;
    }

    /// <summary>
    /// 应用平滑移动和旋转到角色
    /// 移动速度受体重影响
    /// </summary>
    private void MoveCharacter()
    {
        currentMovement = Vector3.SmoothDamp(currentMovement, moveDirection, ref moveVectorRef, smoothTime);
        Vector3 finalMove = currentMovement * (speed * (1 - size * 0.5f)) + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        if (currentMovement.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(currentMovement);
        }
    }

    /// <summary>
    /// 处理跳跃输入，使用物理公式计算初始速度
    /// </summary>
    private void OnJump()
    {
        if (!controller.isGrounded) return;
        float jumpInitialVelocity = Mathf.Sqrt(2 * gravity * jumpHeight);
        verticalVelocity = jumpInitialVelocity;
    }

    /// <summary>
    /// 应用重力效果，处理地面检测
    /// </summary>
    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            return;
        }

        verticalVelocity -= gravity * Time.deltaTime;
    }

    /// <summary>
    /// 更新肚子的弹簧物理系统
    /// 使用弹力和阻尼计算肚子的位置
    /// </summary>
    private void BellyPhysics()
    {
        Shader.SetGlobalFloat(ShaderSize, size);

        Vector3 displacement = bellyPosition - transform.position;
        Vector3 springForce = -elasticity * displacement;
        Vector3 dampingForce = -damping * velocity;

        acceleration = springForce + dampingForce;
        velocity += acceleration * Time.deltaTime;
        bellyPosition += velocity * Time.deltaTime;

        Shader.SetGlobalVector(ShaderBellyPosition, bellyPosition);
    }
    
    /// <summary>
    /// 增加肥胖度的事件
    /// </summary>
    /// <param name="weight"></param>
    private void OnPigeonGetWeight(float weight)
    {
        size = Mathf.Clamp01(size + weight);
    }
#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式下更新肚子大小和位置
    /// </summary>
    [OnInspectorInit]
    private void AdjustSize()
    {
        Shader.SetGlobalFloat(ShaderSize, size);
        Shader.SetGlobalVector(ShaderBellyPosition, transform.position);
    }

    /// <summary>
    /// 退出时重置肚子状态
    /// </summary>
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        AdjustSize();
    }
#endif
}