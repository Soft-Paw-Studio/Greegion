using UnityEngine;

public class JumpingState : ICharacterState
{
    private RigidbodyCharacterControllerStateMachine controller;

    public JumpingState(RigidbodyCharacterControllerStateMachine controller)
    {
        this.controller = controller;
    }

    public void EnterState()
    {
        
        float jumpVelocity = controller.CalculateJumpVelocity();
        
        // 保持水平速度，只修改垂直速度
        controller.rb.linearVelocity = new Vector3(
            controller.rb.linearVelocity.x, 
            jumpVelocity, 
            controller.rb.linearVelocity.z
        );
        
        // 重置跳跃缓冲
        controller.jumpBufferCounter = 0f;
    }

    public void UpdateState()
    {
        // 当垂直速度减小到一定值时，切换到下落状态
        if (controller.rb.linearVelocity.y < 0.1f)
        {
            controller.ChangeState<FallingState>();
        }
    }

    public void FixedUpdateState()
    {
        // 空中移动控制
        HandleAirMovement();
    }

    public void ExitState()
    {
        // Nothing to clean up
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