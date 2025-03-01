using System;
using UnityEngine; using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerWithForce : MonoBehaviour
{
    [Header("Movement Parameters")] [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField] private float maxVelocity = 8f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airControl = 0.3f;

    [Header("Jump Parameters")] [SerializeField]
    private float jumpHeight = 2f;

    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Slide Parameters")] [SerializeField]
    private float wallSlideSpeed = 2f;

    [SerializeField] private float wallCheckDistance = 0.3f;

    [Header("Force Settings")] [SerializeField]
    private float mass = 1f; // 用于模拟AddForce效果的质量参数
    [Header("Gravity Settings")]
    [SerializeField] private float maxFallSpeed = -6f;

    private CharacterController controller;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isAgainstWall;
    private Vector3 wallNormal;
    private float jumpBufferCounter;
    private bool canJump = true;
    [SerializeField] private float coyoteTimeDuration = 0.2f;
    private float coyoteTimeCounter = 0f;

// 用于模拟速度（包含水平和垂直分量）
    private Vector3 velocity;
    private Vector3 lastVelocity;
    private float gravity;

    public InputHandler inputHandler;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        gravity = Physics.gravity.y;
        inputHandler.Move += OnMove;
        inputHandler.Jump += OnJump;
    }
    

    private void FixedUpdate()
    {
        // 处理跳跃缓冲计时
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 当角色处于地面且无输入时，对水平速度进行阻尼处理
        if (isGrounded && moveInput.magnitude < 0.1f)
        {
            Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
            horizontalVel = Vector3.Lerp(horizontalVel, Vector3.zero, groundDrag * Time.deltaTime);
            velocity.x = horizontalVel.x;
            velocity.z = horizontalVel.z;
        }
        
        // 地面检测
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundLayer);
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
            canJump = true;
        }
        else if (coyoteTimeCounter > 0)
        {
            coyoteTimeCounter -= Time.deltaTime;
            canJump = true;
        }
        else
        {
            canJump = false;
        }

        // 检测是否靠墙
        CheckWall();

        // 根据输入处理水平移动（含空中控制）
        HandleMovement();

        // 处理跳跃逻辑（包括跳跃缓冲与土狼时间）
        HandleJump();

        // 当角色接触地面且下落时，归零垂直速度
        if (isGrounded && velocity.y < 0.1f)
        {
            velocity.y = 1; // 限制最大下落速度
        }
        else
        {
            // 应用重力
            velocity.y += gravity * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, maxFallSpeed); // 限制最大下落速度
        }

        // 处理墙面滑行：在空中、靠墙且下落时，限制下落速度
        HandleWallSlide();


        lastVelocity = velocity;

        // 最后使用CharacterController进行移动
        controller.Move(lastVelocity * Time.deltaTime);

    }

    private void HandleMovement()
    {
        // 计算移动方向（转换为世界空间）
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
        moveDir = Camera.main.transform.TransformDirection(moveDir);
        moveDir.y = 0f;
        moveDir.Normalize();

        // 计算目标水平速度
        Vector3 targetVelocity = moveDir * moveSpeed;
        Vector3 currentHorizontal = new Vector3(velocity.x, 0, velocity.z);
        Vector3 velocityChange = targetVelocity - currentHorizontal;
        if (!isGrounded)
        {
            velocityChange *= airControl;
        }

        velocity.x += velocityChange.x;
        velocity.z += velocityChange.z;

        // 限制水平速度
        Vector3 newHorizontal = new Vector3(velocity.x, 0, velocity.z);
        if (newHorizontal.magnitude > maxVelocity)
        {
            newHorizontal = newHorizontal.normalized * maxVelocity;
            velocity.x = newHorizontal.x;
            velocity.z = newHorizontal.z;
        }
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0 && canJump)
        {
            float jumpVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
            velocity.y = jumpVelocity;
            jumpBufferCounter = 0f;
            canJump = false;
            coyoteTimeCounter = 0f;
        }
    }

    private void CheckWall()
    {
        isAgainstWall = false;
        // 以 CharacterController 的中心点为原点检测周围是否有墙体
        Vector3 origin = transform.position + controller.center;
        RaycastHit hit;
        for (float angle = 0; angle < 360; angle += 45)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            if (Physics.Raycast(origin, direction, out hit, wallCheckDistance, groundLayer))
            {
                isAgainstWall = true;
                wallNormal = hit.normal;
                break;
            }
        }
    }

    private void HandleWallSlide()
    {
        if (isAgainstWall && !isGrounded && velocity.y < 0)
        {
            velocity.y = -wallSlideSpeed;
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

// 模拟Rigidbody的AddForce操作
    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
    {
        switch (mode)
        {
            case ForceMode.Force:
                // 施加持续力：加速度 = 力 / 质量，再乘以 deltaTime
                velocity += force / mass * Time.deltaTime;
                break;
            case ForceMode.Impulse:
                // 瞬间冲量：直接改变速度
                velocity += force / mass;
                break;
            case ForceMode.Acceleration:
                // 直接施加加速度（不受质量影响）
                velocity += force * Time.deltaTime;
                break;
            case ForceMode.VelocityChange:
                // 直接改变速度（不受质量影响）
                velocity += force;
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckDistance);

        Gizmos.color = Color.blue;
        if (Application.isPlaying && controller != null)
        {
            Vector3 origin = transform.position + controller.center;
            for (float angle = 0; angle < 360; angle += 45)
            {
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                Gizmos.DrawRay(origin, direction * wallCheckDistance);
            }
        }
    }
}