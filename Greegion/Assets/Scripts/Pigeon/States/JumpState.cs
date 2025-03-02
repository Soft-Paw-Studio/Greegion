using UnityEngine;

namespace Pigeon.States
{
    public class JumpState : PigeonState
    {
        //public override void Enter(PigeonController pigeon)
        // {
        //     //base.Enter(pigeon);
        //     //if (!pigeon.Controller.isGrounded) return;
        //     //Controller.verticalVelocity = Mathf.Sqrt(2 * Mathf.Abs(Controller.gravity) * Controller.data.jumpHeight);
        // }

        public override void Update()
        {
            //if (Controller.Controller.isGrounded && Controller.verticalVelocity < 0)
            {
                Controller.StateManager.ChangeState<IdleState>();
            }
        }
    }
}