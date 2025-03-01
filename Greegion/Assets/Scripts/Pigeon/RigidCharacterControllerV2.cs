using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyCharacterControllerV4 : MonoBehaviour
{
    [Header("Movement Parameters")]
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

    public Rigidbody rb;
    public Vector2 moveInput;
    public bool isGrounded;
    public bool isAgainstWall;
    public Vector3 wallNormal;
    public float jumpBufferCounter;
    public bool canJump = true;
    public float coyoteTimeCounter = 0f;
    public InputHandler inputHandler;
    public Collider collider;

    public bool jumped;
    
    //Initial on gamestart
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        // 绑定输入事件
        inputHandler.Move += OnMove;
        inputHandler.Jump += OnJump;
    }
    
    public void OnMove(Vector2 direction)
    {
        moveInput = direction;
    }
    
    public void OnJump()
    { 
        jumpBufferCounter = jumpBufferTime;
        jumped = true;
    }
    
    private void FixedUpdate()
    {
        // 更新状态检查
        UpdateStateChecks();
        
        // 处理移动和跳跃
        HandleMovementPhysics();
        HandleJumpPhysics();
        HandleWallSlidePhysics();
        
        // 更新计时器
        UpdateTimers();
    }

    private void UpdateStateChecks()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);
        
        isAgainstWall = false;
        RaycastHit hit;
        
        // 在8个方向上检查墙壁
        for (float angle = 0; angle < 360; angle += 45)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            if (Physics.Raycast(collider.bounds.center, direction, out hit, wallCheckDistance, groundLayer))
            {
                isAgainstWall = true;
                wallNormal = hit.normal;
                break;
            }
        }
        
        // 更新接地状态相关逻辑
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
            canJump = true;

        }

        if (rb.linearVelocity.y <= 0.1f)
        {
            jumped = false;
        }
    }

    private void UpdateTimers()
    {
        jumpBufferCounter -= Time.fixedDeltaTime;
        
        if (!isGrounded)
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
            if (coyoteTimeCounter <= 0f)
            {
                canJump = false;
            }
        }
    }
    
    private void HandleMovementPhysics()
    {
        // 获取摄像机相对的移动方向
        Vector3 moveDirection = GetCameraRelativeMoveDirection();

        if (moveDirection.magnitude > 0.1f)
        {
            // 使用上一帧的速度计算新的移动
            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 targetVelocity = moveDirection * moveSpeed;
            Vector3 velocityChange = targetVelocity - currentHorizontalVelocity;

            // 在空中减少控制
            if (!isGrounded)
            {
                velocityChange *= airControl;
            }

            // 应用速度变化
            Vector3 newVelocity = new Vector3(
                rb.linearVelocity.x + velocityChange.x,
                rb.linearVelocity.y,
                rb.linearVelocity.z + velocityChange.z
            );

            // 应用最大速度限制
            Vector3 horizontalVelocity = new Vector3(newVelocity.x, 0f, newVelocity.z);
            if (horizontalVelocity.magnitude > maxVelocity)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxVelocity;
                newVelocity = new Vector3(horizontalVelocity.x, newVelocity.y, horizontalVelocity.z);
            }

            rb.linearVelocity = newVelocity;
        }

        // 根据接地状态应用阻力
        rb.linearDamping = isGrounded ? groundDrag : 0f;
    }
    
    private Vector3 GetCameraRelativeMoveDirection()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        moveDirection = Camera.main.transform.TransformDirection(moveDirection);
        moveDirection.y = 0f;
        return moveDirection.normalized;
    }

    private void HandleJumpPhysics()
    {
        if (jumpBufferCounter > 0 && canJump && !jumped)
        {
            float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            
            // 保持水平速度，只修改垂直速度
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
            
            // 重置状态
            jumpBufferCounter = 0f;
            canJump = false;
            coyoteTimeCounter = 0f;
        }
    }
    
    private void HandleWallSlidePhysics()
    {
        if (isAgainstWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x, 
                Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed), 
                rb.linearVelocity.z
            );
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 绘制地面检测范围
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
        
        // 绘制墙壁检测射线
        Gizmos.color = isAgainstWall ? Color.yellow : Color.blue;
        for (float angle = 0; angle < 360; angle += 45)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Gizmos.DrawRay(collider.bounds.center, direction * wallCheckDistance);
        }
    }
}