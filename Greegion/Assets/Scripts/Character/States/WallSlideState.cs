using UnityEngine;

public class WallSlideState : ICharacterState
{
    private RigidbodyCharacterControllerStateMachine controller;
    private float wallSlideTimer = 0;
    private bool canWallJump = true;

    public WallSlideState(RigidbodyCharacterControllerStateMachine controller)
    {
        this.controller = controller;
    }

    public void EnterState()
    {
        controller.rb.linearDamping = 0;
        canWallJump = true;
        wallSlideTimer = 0;
        controller.hitByWall = true;
    }

    public void UpdateState()
    {
        // 处理墙跳
        if (controller.jumpBufferCounter > 0 && canWallJump)
        {
            PerformWallJump();
            controller.ChangeState<JumpingState>();
            return;
        }

        // 如果不再贴墙或者已着地，切换状态
        if (!controller.isAgainstWall)
        {
            controller.ChangeState<FallingState>();
        }
    }

    public void FixedUpdateState()
    {
        // 限制下落速度
        Vector3 velocity = controller.rb.linearVelocity;
        if (velocity.y < -controller.WallSlideSpeed)
        {
            velocity.y = -controller.WallSlideSpeed;
            controller.rb.linearVelocity = velocity;
        }
        
        // 增加计时器
        wallSlideTimer += Time.fixedDeltaTime;
        
        // 如果玩家仍在尝试向墙移动，可能执行墙反弹
        if (controller.moveDirection.magnitude > 0 && wallSlideTimer > 0.5f)
        {
            if (Vector3.Dot(controller.transform.forward, controller.wallNormal) < -0.5)
            {
                PerformWallBounce();
            }
        }
    }

    public void ExitState()
    {
        controller.hitByWall = false;
    }

    public float GetRotationSpeed()
    {
        return controller.RotationSpeed * 0.5f;
    }
    
    private void PerformWallJump()
    {
        Vector3 jumpDirection = controller.wallNormal + Vector3.up;
        jumpDirection.Normalize();
        
        float jumpVelocity = controller.CalculateJumpVelocity();
        controller.rb.linearVelocity = jumpDirection * jumpVelocity;
        
        // 重置跳跃缓冲
        controller.jumpBufferCounter = 0f;
    }
    
    private void PerformWallBounce()
    {
        Vector3 forceDirection = controller.wallNormal + Vector3.up;
        forceDirection.Normalize();
        
        controller.rb.linearVelocity = Vector3.zero;
        controller.rb.AddForce(forceDirection * controller.BounceForce, ForceMode.VelocityChange);
        
        // 改变面向方向
        controller.rb.rotation = Quaternion.LookRotation(controller.wallNormal);
        
        // 切换回下落状态
        controller.ChangeState<FallingState>();
    }
}