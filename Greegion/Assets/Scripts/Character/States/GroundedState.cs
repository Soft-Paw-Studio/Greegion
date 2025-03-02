using UnityEngine;

public class GroundedState : ICharacterState
{
    private RigidbodyCharacterControllerStateMachine controller;
    private bool canJump = true;

    public GroundedState(RigidbodyCharacterControllerStateMachine controller)
    {
        this.controller = controller;
    }

    public void EnterState()
    {
        canJump = true;
    }

    public void UpdateState()
    {
        // 处理跳跃
        if (controller.jumpBufferCounter > 0 && canJump)
        {
            controller.ChangeState<JumpingState>();
        }
    }

    public void FixedUpdateState()
    {
        // 地面移动
        HandleGroundMovement();
    }

    public void ExitState()
    {
        canJump = false;
    }

    public float GetRotationSpeed()
    {
        return controller.RotationSpeed;
    }

    private void HandleGroundMovement()
    {
        controller.ApplyMovement(0);
    }
}