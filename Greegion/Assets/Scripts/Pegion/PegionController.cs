using System;
using UnityEngine;
using System.Collections;

public class PegionController : MonoBehaviour
{
    // 移动模式枚举
    public enum MovementMode
    {
        Grid, // 网格移动
        Free // 自由移动
    }

    [Header("Movement Settings")] public MovementMode currentMovementMode = MovementMode.Grid;
    public float moveSpeed = 5f;
    public float gridSize = 1f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Ground Check")] public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    // 组件引用
    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 targetGridPosition;
    private bool isMoving;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        targetGridPosition = transform.position;
    }

    private void Update()
    {
        // 检查是否在地面上
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // 根据当前移动模式处理移动
        switch (currentMovementMode)
        {
            case MovementMode.Grid:
                HandleGrid();
                break;
            case MovementMode.Free:
                HandleFreeMovement();
                break;
        }

        // 处理跳跃
        //HandleJump();

        // 应用重力
        ApplyGravity();
    }

    private void HandleGrid()
    {
        
    }

    private void HandleGridMovement()
    {
        if (isMoving)
        {
            // 如果正在移动到目标格子
            float step = moveSpeed * Time.deltaTime;
            Vector3 movement = Vector3.MoveTowards(transform.position, targetGridPosition, step) - transform.position;
            characterController.Move(movement);

            if (Vector3.Distance(transform.position, targetGridPosition) < 0.001f)
            {
                isMoving = false;
                transform.position = targetGridPosition; // 确保精确到达目标位置
            }

            return;
        }

        // 获取输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            // 一次只允许一个方向的移动，优先水平方向
            if (Mathf.Abs(horizontal) > 0)
            {
                vertical = 0;
            }

            // 确定移动方向
            Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
            Vector3 newPosition = transform.position + direction * gridSize;

            // 检查新位置是否可以移动
            if (Physics.Raycast(newPosition + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f, groundLayer))
            {
                targetGridPosition = new Vector3(newPosition.x, hit.point.y, newPosition.z);
                isMoving = true;
            }
        }
    }

    private void HandleFreeMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f; // 小的负值，确保角色始终受到向下的力
        }

        characterController.Move(velocity * Time.deltaTime);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.TryGetComponent(out ICollectable collectable))
        {
            collectable.Collect();
            Debug.Log("Eat");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.TryGetComponent(out ICollectable collectable))
        {
            collectable.Collect();
            Debug.Log("Eat");
        }
    }
}