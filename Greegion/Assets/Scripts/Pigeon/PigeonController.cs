using UnityEngine;

public partial class PigeonController : Singleton<Pigeon.StateMachine.PigeonController>
{
    public PigeonMovement movement;

    protected override void Awake()
    {
        base.Awake();
        movement = new PigeonMovement();
    }
}
