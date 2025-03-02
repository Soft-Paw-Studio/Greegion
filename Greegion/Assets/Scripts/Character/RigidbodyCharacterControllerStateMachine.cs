using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyCharacterControllerStateMachine : MonoBehaviour
{
    #region Inspector Fields
    [BoxGroup("DEBUG")]
    [ReadOnly] public Vector3 moveDirection;
    [ReadOnly] public Vector3 debugRigidVelocity;
    [ReadOnly] public CharacterState currentState;
    
    [Header("Movement Parameters")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxVelocity = 8f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airControl = 0.3f;
    
    [Header("Jump Parameters")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float coyoteTimeDuration = 0.2f;
    
    [Header("Wall Slide Parameters")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private float bounceForce = 5f;
    [SerializeField] private double wallBounceThreshold = -0.5f;
    #endregion

    #region Public Properties
    public Rigidbody rb { get; private set; }
    public Collider characterCollider { get; private set; }
    public Vector2 moveInput { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isAgainstWall { get; private set; }
    public Vector3 wallNormal { get; private set; }
    public float jumpBufferCounter { get; set; }
    public float coyoteTimeCounter { get; private set; }
    public InputHandler inputHandler;
    public bool hitByWall { get; set; }
    #endregion

    #region Private Fields
    private Dictionary<Type, ICharacterState> states = new Dictionary<Type, ICharacterState>();
    private ICharacterState currentStateInstance;
    private Ray[] rays = new Ray[16];
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        characterCollider = GetComponent<Collider>();
        
        // 绑定输入事件
        inputHandler.Move += OnMove;
        inputHandler.Jump += OnJump;

        // 初始化状态
        states.Add(typeof(GroundedState), new GroundedState(this));
        states.Add(typeof(JumpingState), new JumpingState(this));
        states.Add(typeof(FallingState), new FallingState(this));
        states.Add(typeof(WallSlideState), new WallSlideState(this));

        // 设置初始状态
        ChangeState<GroundedState>();
    }

    private void Start()
    {
        // 初始化射线
        for (float angle = 0; angle < 360; angle += 22.5f)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            rays[Mathf.FloorToInt(angle / 22.5f)] = new Ray(characterCollider.bounds.center, direction);
        }
    }

    public void OnMove(Vector2 direction)
    {
        moveInput = direction;
    }
    
    public void OnJump()
    { 
        jumpBufferCounter = jumpBufferTime;
    }

    private void Update()
    {
        // 更新射线原点和方向
        for (float angle = 0; angle < 360; angle += 22.5f)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            int index = Mathf.FloorToInt(angle / 22.5f);
            rays[index].origin = characterCollider.bounds.center;
            rays[index].direction = direction;
        }
        
        // 更新角色朝向
        moveDirection = GetCameraRelativeMoveDirection();
        if (moveDirection.magnitude != 0f)
        {
            float angleLimit = currentStateInstance.GetRotationSpeed();
            rb.rotation = Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(moveDirection), angleLimit);
        }

        debugRigidVelocity = rb.linearVelocity;
        
        // 更新状态
        currentStateInstance.UpdateState();
        
        // 更新计时器
        UpdateTimers();
    }

    private void FixedUpdate()
    {
        // 更新状态检查
        UpdateStateChecks();
        
        // 当前状态的固定更新
        currentStateInstance.FixedUpdateState();
        
        // // 在水平方向上应用阻尼，同时保留垂直运动
        // //if (isGrounded && !(currentStateInstance is JumpingState))
        // {
        // 只对水平运动应用阻尼
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 dampenedHorizontalVelocity = horizontalVelocity * (1 - groundDrag * Time.fixedDeltaTime);
        
        // 保留原始的垂直速度
        rb.linearVelocity = new Vector3(
            dampenedHorizontalVelocity.x,
            rb.linearVelocity.y,
            dampenedHorizontalVelocity.z
        );
        // }
    }

    public void ApplyMovement(float airReduce = 0)
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 targetVelocity = moveDirection * MoveSpeed;
            Vector3 velocityChange = targetVelocity - currentHorizontalVelocity;
            
            // 在空中减少控制
            velocityChange *= Mathf.Lerp(1,AirControl,airReduce);
            
            // 应用速度变化
            Vector3 newVelocity = new Vector3(
                rb.linearVelocity.x + velocityChange.x,
                rb.linearVelocity.y,
                rb.linearVelocity.z + velocityChange.z
            );
            
            // 应用最大速度限制
            Vector3 horizontalVelocity = new Vector3(newVelocity.x, 0f, newVelocity.z);
            if (horizontalVelocity.magnitude > MaxVelocity)
            {
                horizontalVelocity = horizontalVelocity.normalized * MaxVelocity;
                newVelocity = new Vector3(horizontalVelocity.x, newVelocity.y, horizontalVelocity.z);
            }
            
            rb.linearVelocity = newVelocity;
        }
    }

    private void UpdateStateChecks()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);
        
        isAgainstWall = false;
        
        // 在多个方向上检查墙壁
        for (float angle = 0; angle < 360; angle += 22.5f)
        {
            if (Physics.Raycast(rays[Mathf.FloorToInt(angle / 22.5f)], out RaycastHit hit, wallCheckDistance, groundLayer))
            {
                isAgainstWall = true;
                wallNormal = hit.normal;
                break;
            }
        }
        
        // 根据检测结果自动切换状态
        if (isGrounded && !(currentStateInstance is GroundedState))
        {
            ChangeState<GroundedState>();
            coyoteTimeCounter = coyoteTimeDuration;
            hitByWall = false;
        }
        else if (!isGrounded && rb.linearVelocity.y < 0.1f && !(currentStateInstance is FallingState) && !(currentStateInstance is WallSlideState))
        {
            ChangeState<FallingState>();
        }
        else if (isAgainstWall && !isGrounded && rb.linearVelocity.y < 0.1f && Vector3.Dot(transform.forward, wallNormal) < wallBounceThreshold)
        {
            ChangeState<WallSlideState>();
        }
    }

    private void UpdateTimers()
    {
        jumpBufferCounter -= Time.deltaTime;
        
        if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    public Vector3 GetCameraRelativeMoveDirection()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        moveDirection = Camera.main.transform.TransformDirection(moveDirection);
        moveDirection.y = 0f;
        return moveDirection.normalized;
    }

    public void ChangeState<T>() where T : ICharacterState
    {
        if (currentStateInstance != null)
        {
            currentStateInstance.ExitState();
        }
        
        currentStateInstance = states[typeof(T)];
        currentState = (CharacterState)Enum.Parse(typeof(CharacterState), typeof(T).Name.Replace("State", ""));
        currentStateInstance.EnterState();
    }

    public float CalculateJumpVelocity()
    {
        return Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
    }

    private void OnDrawGizmos()
    {
        // 绘制地面检测范围
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
        
        // 绘制墙壁检测射线
        if (Application.isPlaying)
        {
            for (float angle = 0; angle < 360; angle += 22.5f)
            {
                Ray gizmoRay = rays[Mathf.FloorToInt(angle / 22.5f)];
                
                if (Physics.Raycast(gizmoRay, wallCheckDistance, groundLayer))
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.green;
                }
                
                Gizmos.DrawRay(gizmoRay.origin, gizmoRay.direction * wallCheckDistance);
            }
        }
    }
    
    // 用于访问参数
    public float MoveSpeed => moveSpeed;
    public float MaxVelocity => maxVelocity;
    public float GroundDrag => groundDrag;
    public float AirControl => airControl;
    public float RotationSpeed => rotationSpeed;
    public float WallSlideSpeed => wallSlideSpeed;
    public float BounceForce => bounceForce;
    public LayerMask GroundLayer => groundLayer;
}