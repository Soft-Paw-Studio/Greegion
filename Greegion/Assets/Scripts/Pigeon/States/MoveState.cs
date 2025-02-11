namespace Pigeon.States
{
    public class MoveState : PigeonState
    {
        public override void Update()
        {
            if (Controller.MoveDirection.magnitude == 0)
            {
                Controller.StateManager.ChangeState<IdleState>();
            }
        }
    }
}
