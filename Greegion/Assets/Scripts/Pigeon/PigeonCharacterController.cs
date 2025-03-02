using System;
using Pigeon;
using Pigeon.States;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PigeonCharacterController : MonoBehaviour
{
    [ShowInInspector][ReadOnly]
    public string CurrentState;
    public InputHandler inputHandler ; 
    public PigeonData data;

    internal Rigidbody rigidbody;

    private Camera MainCam { get; set; }
    public StateManager StateManager { get; private set; }

    internal Vector3 currentMovement;
    internal Vector3 targetMovement;
    
    private void Awake()
    {
        MainCam = Camera.main;
        StateManager = new StateManager(this);
        StateManager.ChangeState<IdleState>();

        rigidbody = GetComponent<Rigidbody>();
        
        inputHandler.EnableInput();
        inputHandler.Move += HandleMove;
    }
    
    private void HandleMove(Vector2 direction)
    {
        var normalizedDirection = direction.normalized;
        var camForward = Vector3.ProjectOnPlane(MainCam.transform.forward, Vector3.up).normalized;
        var camRight = Vector3.ProjectOnPlane(MainCam.transform.right, Vector3.up).normalized;
        targetMovement = (camRight * normalizedDirection.x + camForward * normalizedDirection.y).normalized;
    }

    private void ApplyMovement(Vector3 movement)
    {
        rigidbody.linearVelocity = movement;
    }

    private void Update()
    {
        CurrentState = StateManager.CurrentState.GetType().ToString();
        StateManager.Update();
    }

    private void FixedUpdate()
    {
        StateManager.FixedUpdate();
    }
}
