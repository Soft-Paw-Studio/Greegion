using UnityEngine;

namespace Pigeon.StateMachine
{
    public class MoveState : BaseState
    {
        public override void EnterState()
        {
            
        }

        public override void UpdateState()
        {
            MoveCharacter(C.inputDirection);

            if (C.currentMovement.sqrMagnitude > 0.01f)
            {
                C.transform.rotation = Quaternion.LookRotation(C.currentMovement);
            }

            if (C.inputDirection.sqrMagnitude <= 0)
            {
                stateMachine.ChangeState<IdleState>();
            }
        }

        public override void ExitState()
        {

        }
    }
}
