using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PigeonController : MonoBehaviour
{
    [ReadOnly] public Vector3 moveDirection;
    private Vector3 currentMovement;

    [Range(0, 3)] public float speed;
    public float smoothTime;
    public float jumpForce = 5f;  // 跳跃力度
    public float gravity = 9.81f; // 重力

    private CharacterController controller;
    private PegionActions input;
    private Vector3 moveVectorRef;
    private Camera _camera;
    private float verticalVelocity; // 垂直方向速度

    private void Awake()
    {
        input = new PegionActions();
        input.Enable();

        // 绑定跳跃事件
        input.Gameplay.Jump.performed += ctx => OnJump();
    }

    private void Start()
    {
        _camera = Camera.main;
        TryGetComponent(out controller);
    }

    private void OnMovement()
    {
        var move = input.Gameplay.Movement.ReadValue<Vector2>().normalized;

        // 获取摄像机方向并投影到 XZ 平面
        Vector3 camForward = _camera.transform.forward;
        Vector3 camRight = _camera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camRight * move.x + camForward * move.y).normalized;
    }

    private void OnJump()
    {
        if (controller.isGrounded) // 只有在地面上才能跳跃
        {
            verticalVelocity = jumpForce;
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // 轻微下压防止悬浮
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime; // 应用重力
        }
    }

    private void Update()
    {
        OnMovement();
        ApplyGravity();

        // 计算水平移动
        currentMovement = Vector3.SmoothDamp(currentMovement, moveDirection, ref moveVectorRef, smoothTime);

        // 计算最终的移动向量
        Vector3 finalMove = currentMovement * speed + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        // 旋转角色（避免零向量错误）
        if (currentMovement.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(currentMovement);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ICollectable collectable))
        {
            collectable.Collect();
            Debug.Log("Eat");
        }
    }
}
