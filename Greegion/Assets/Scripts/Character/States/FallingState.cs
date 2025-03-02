using UnityEngine;

public class FallingState : ICharacterState
{
    private RigidbodyCharacterControllerStateMachine controller;
    private bool canJump = false;

    public FallingState(RigidbodyCharacterControllerStateMachine controller)
    {
        this.controller = controller;
    }

    public void EnterState()
    {
        controller.rb.useGravity = true;
        
        // 使用Coyote Time允许短时间内跳跃
        canJump = controller.coyoteTimeCounter > 0;
    }

    public void UpdateState()
    {
        // 处理Coyote Time跳跃
        if (controller.jumpBufferCounter > 0 && canJump)
        {
            controller.ChangeState<JumpingState>();
            return;
        }

        // 检查是否可以切换到滑墙状态
        if (controller.isAgainstWall && controller.moveDirection.magnitude > 0)
        {
            if (Vector3.Dot(controller.transform.forward, controller.wallNormal) < -0.5)
            {
                controller.ChangeState<WallSlideState>();
            }
        }
    }

    public void FixedUpdateState()
    {
        // 空中移动控制
        HandleAirMovement();
    }

    public void ExitState()
    {
        // Nothing specific to clean up
    }

    public float GetRotationSpeed()
    {
        return controller.RotationSpeed * controller.AirControl * 10;
    }
    
    private void HandleAirMovement()
    {
        controller.ApplyMovement(1);
    }
}