using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class RController_V1 : MonoBehaviour
{
    [Header("Movement")]
    public InputHandler input;
    public float moveSpeed = 6f;
    public float rotationTime = 6f;
    public float acceleration = 10f;
    public float airControl = 0.5f;
    
    [Header("Jump")]
    public float jumpForce = 8f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    public Rigidbody rb;
    
    [ReadOnly]public Vector3 moveDirection;
    [ReadOnly] public bool isGrounded;
    [ReadOnly]public float verticalVelocity;



    private void Awake()
    {
        input.Move += InputOnMove;
        input.Jump += InputOnJump;
    }

    private void InputOnJump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void InputOnMove(Vector2 dir)
    { 
        moveDirection = new Vector3(dir.x, 0f, dir.y);
        if (Camera.main != null) moveDirection = Camera.main.transform.TransformDirection(moveDirection);
        moveDirection.y = 0f;
        moveDirection.Normalize();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 计算目标速度
        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // 保持垂直速度

        // 根据地面状态调整控制力
        float controlFactor = isGrounded ? 1f : airControl;
    
        // 应用速度变化
        Vector3 velocityChange = (targetVelocity - rb.linearVelocity) * (acceleration * controlFactor);
        velocityChange.y = 0; // 不直接修改垂直速度
    
        rb.AddForce(velocityChange, ForceMode.Acceleration);
    }

    void Update()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDirection),
                rotationTime * Time.deltaTime);
        }
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.position, 0.1f,LayerMask.GetMask("Ground"));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}
