using System;
using Unity.Mathematics;
using UnityEngine;

public class RigidCharacterController : MonoBehaviour
{
    private Camera MainCam;
    public Rigidbody rigid;
    public InputHandler inputHandler;
    public float jumpHeight = 2f;
    public Vector3 currentVelocity;
    
    private Vector3 targetMovement;
    
    [Header("移动参数")]
    [SerializeField] private float maxSpeed = 5f;        
    [SerializeField] private float acceleration = 50f;   
    [SerializeField] private float deceleration = 30f;   
    [SerializeField] private float turnSpeed = 10f;      
    [SerializeField] private bool canMoveInAir = false;  

    [Header("碰撞检测")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;      
    [SerializeField] private float wallCheckDistance = 0.5f;  // 墙壁检测距离
    [SerializeField] private float wallFriction = 0.5f;      // 墙壁摩擦力
    [SerializeField] private float wallBounce = 0.1f;        // 墙壁反弹力
    
    private bool isGrounded;
    private Vector3 wallNormal;
    private bool isAgainstWall;
    
    private void Awake()
    {
        MainCam = Camera.main;
        
        inputHandler.EnableInput();
        inputHandler.Move += HandleMove;
        inputHandler.Jump += HandleJump;
    }

    private void Update()
    {
        // 地面检测
        isGrounded = Physics.CheckSphere
        (
            transform.position + Vector3.up * groundCheckRadius + Vector3.down * 0.1f,
            groundCheckRadius,
            groundLayer
        );

        // 墙壁检测
        CheckWallCollision();
    }

    private void CheckWallCollision()
    {
        isAgainstWall = false;
        wallNormal = Vector3.zero;

        // 发射射线检测周围的墙壁
        RaycastHit hit;
        Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
        
        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out hit, wallCheckDistance, groundLayer))
            {
                isAgainstWall = true;
                wallNormal = hit.normal;
                break;
            }
        }
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            rigid.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.Impulse);
        }
    }

    private void HandleMove(Vector2 direction)
    {
        var normalizedDirection = direction.normalized;
        var camForward = Vector3.ProjectOnPlane(MainCam.transform.forward, Vector3.up).normalized;
        var camRight = Vector3.ProjectOnPlane(MainCam.transform.right, Vector3.up).normalized;
        targetMovement = (camRight * normalizedDirection.x + camForward * normalizedDirection.y).normalized;
    }

    private void FixedUpdate()
    {
        if (!isGrounded && !canMoveInAir)
        {
            HandleWallCollision();
            return;
        }

        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rigid.linearVelocity, Vector3.up);
        
        if (targetMovement.sqrMagnitude > 0.01f)
        {
            Vector3 targetVelocity = targetMovement * maxSpeed;
            
            // 如果靠近墙壁，调整移动方向
            if (isAgainstWall && !isGrounded)
            {
                return;
            }
            
            Vector3 newVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
            
            rigid.linearVelocity = new Vector3(newVelocity.x, rigid.linearVelocity.y, newVelocity.z);
            
            if (!isAgainstWall)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetMovement);
                rigid.rotation = Quaternion.Slerp(rigid.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }
        }
        else if (horizontalVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 newVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                deceleration * Time.fixedDeltaTime
            );
            
            rigid.linearVelocity = new Vector3(newVelocity.x, rigid.linearVelocity.y, newVelocity.z);
        }

        HandleWallCollision();
    }

    private void HandleWallCollision()
    {
        if (isAgainstWall)
        {
            // 获取当前速度
            Vector3 velocity = rigid.linearVelocity;
            
            // 如果角色正在向墙壁移动
            float wallDot = Vector3.Dot(velocity, -wallNormal);
            if (wallDot > 0)
            {
                // 计算反弹速度
                Vector3 bounceVelocity = Vector3.Reflect(velocity, wallNormal) * wallBounce;
                // 保持一部分向下的速度，以防止角色卡在墙上
                bounceVelocity.y = velocity.y;
                
                // 应用新的速度
                rigid.linearVelocity = bounceVelocity;
            }
            
            // 添加一个微小的力使角色远离墙壁
            rigid.AddForce(wallNormal * 0.5f, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        // 绘制地面检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckRadius + Vector3.down * 0.1f, groundCheckRadius);
        
        // 绘制墙壁检测射线
        Gizmos.color = Color.red;
        Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
        foreach (Vector3 dir in directions)
        {
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, dir * wallCheckDistance);
        }
    }
}