using UnityEngine;

namespace Pigeon.StateMachine
{
    public class IdleState : BaseState
    {
        public override void EnterState()
        {

        }

        public override void UpdateState()
        {
            MoveCharacter(Vector3.zero);
            
            if (C.inputDirection.sqrMagnitude > 0)
            {
                stateMachine.ChangeState<MoveState>();
            }
        }

        public override void ExitState()
        {

        }
    }
}

