using System;
using UnityEngine;

public class PigeonChar : MonoBehaviour
{
    public InputHandler input;

    [Range(0,1)] public float groundedRadius = 0.1f;
    [Range(0,125)]public float moveSpeed;
    [Range(0,1)]public float airControlMultiplier;
    [Range(0,5)]public float jumpCooldown;
    [Range(0,5)]public float jumpHeight;
    [Range(0,1)]public float lowJumpMultiplier;
    [Range(0,1)]public float fallMultiplier;
    [Range(0,25)]public float rotationSpeed;

    private Rigidbody rb;
    private bool isGrounded;
    private float lastJumpTime;
    private Vector3 moveDirection;
    public Vector2 moveInput;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input.EnableInput();
        
        input.Move += OnMove;
        input.Jump += OnJump;
    }

    private void OnJump()
    {
        TryJump();
    }

    private void OnMove(Vector2 inputDirection)
    {
        moveInput = inputDirection;
    }

    private void Update()
    {
        CheckGrounded();


        if (moveDirection.magnitude > 0.1f)
        {
            rb.rotation = Quaternion.Lerp(rb.rotation,Quaternion.LookRotation(moveDirection),Time.deltaTime * rotationSpeed);
        }
    }

    private void FixedUpdate()
    {
        CalculateMoveDirection();Move();
    }
    
    private void CalculateMoveDirection()
    {
        if (moveInput.sqrMagnitude < 0.1f)
        {
            moveDirection = Vector3.zero;
            return;
        }
        
        // 获取摄像机前方和右方向
        Vector3 cameraForward = _camera.transform.forward;
        Vector3 cameraRight = _camera.transform.right;
        
        // 将y分量清零，保持在水平面上移动
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // 计算最终移动方向
        moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;
    }
    
    private void Move()
    {
        float currentMoveSpeed = moveSpeed;
        
        // 如果在空中，应用空中控制系数
        if (!isGrounded)
        {
            currentMoveSpeed *= airControlMultiplier;
        }
        
        // 保留当前的Y轴速度（不影响跳跃或重力）
        Vector3 velocity = new Vector3(moveDirection.x * currentMoveSpeed, rb.linearVelocity.y, moveDirection.z * currentMoveSpeed);
        rb.linearVelocity = velocity;
    }
    
    private void TryJump()
    {
        // 如果在地面上且跳跃冷却已过
        if (isGrounded)
        {
            // 计算跳跃所需的初速度: v = sqrt(2 * g * h)
            float jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            
            // 设置垂直速度
            Vector3 velocity = rb.linearVelocity;
            velocity.y = jumpVelocity;
            rb.linearVelocity = velocity;
        }
    }

    private void CheckGrounded()
    {
        isGrounded =  Physics.CheckSphere(transform.position, groundedRadius, LayerMask.GetMask("Ground"));
        rb.useGravity = !isGrounded;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position,groundedRadius);
    }
}
