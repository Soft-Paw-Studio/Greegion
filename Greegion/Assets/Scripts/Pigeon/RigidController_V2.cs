using System;
using UnityEngine;

public class RigidController_V2 : MonoBehaviour
{
    private Camera MainCam;
    public Rigidbody rigid;
    public InputHandler inputHandler;
    public float speed = 3;
    
    private Vector3 movementInput;
    private Vector3 currentMovement;
    private Vector3 currentVelocity;
    [SerializeField] private float smoothTime;

    private void Awake()
    {
        MainCam = Camera.main;
        
        inputHandler.EnableInput();
        inputHandler.Move += HandleMove;
    }
    
    private void HandleMove(Vector2 direction)
    {
        var normalizedDirection = direction.normalized;
        var camForward = Vector3.ProjectOnPlane(MainCam.transform.forward, Vector3.up).normalized;
        var camRight = Vector3.ProjectOnPlane(MainCam.transform.right, Vector3.up).normalized;
        movementInput = (camRight * normalizedDirection.x + camForward * normalizedDirection.y).normalized;
    }

    private void FixedUpdate()
    {
        currentMovement = Vector3.SmoothDamp(currentMovement, movementInput, ref currentVelocity, smoothTime);
        rigid.linearVelocity = new Vector3(currentMovement.x * speed, rigid.linearVelocity.y, currentMovement.z * speed);
    }
}
