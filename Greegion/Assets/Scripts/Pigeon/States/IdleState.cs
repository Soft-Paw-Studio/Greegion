namespace Pigeon.States
{
    public class IdleState : PigeonState
    {
        public override void Update()
        {
            if (Controller.MoveDirection.magnitude > 0)
            {
                Controller.StateManager.ChangeState<MoveState>();
            }
        }
    }
}
