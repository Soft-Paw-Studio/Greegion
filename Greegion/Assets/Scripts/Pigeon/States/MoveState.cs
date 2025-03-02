using UnityEngine;

namespace Pigeon.States
{
    public class MoveState : PigeonState
    {
        public override void Enter(PigeonCharacterController pigeon)
        {
            Controller.inputHandler.Jump += InputHandlerOnJump;
        }

        private void InputHandlerOnJump()
        {
            StateManager.ChangeState<JumpState>();
        }

        public override void Exit()
        {
            Controller.inputHandler.Jump -= InputHandlerOnJump;
        }

        public override void FixedUpdate()
        {
            if (Controller.targetMovement.magnitude > 0.1f)
            {
        
                // 使用上一帧的速度计算新的移动
                Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Vector3 targetVelocity = Controller.targetMovement * 5;
                Vector3 velocityChange = targetVelocity - currentHorizontalVelocity;
                
                // 应用速度变化
                Vector3 newVelocity = new Vector3(
                    rb.linearVelocity.x + velocityChange.x,
                    rb.linearVelocity.y,
                    rb.linearVelocity.z + velocityChange.z
                );
                
                rb.linearVelocity = newVelocity;
            }
            else
            {
                Controller.StateManager.ChangeState<IdleState>();
            }
        }
    }
}
