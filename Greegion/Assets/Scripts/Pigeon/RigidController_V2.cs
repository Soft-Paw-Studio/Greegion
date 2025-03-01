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
    [SerializeField] private float jumpHeight;
    [SerializeField] private float checkSphereSize;
    [SerializeField] private bool isGrounded;

    private float currentYVelocity;
    [SerializeField] private float maxFallSpeed = 10;

    private void Awake()
    {
        MainCam = Camera.main;
        
        inputHandler.EnableInput();
        inputHandler.Move += HandleMove;
        inputHandler.Jump += HandleJump;
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            rigid.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.Impulse);
        }
    }

    private void HandleMove(Vector2 direction)
    {
        var normalizedDirection = direction.normalized;
        var camForward = Vector3.ProjectOnPlane(MainCam.transform.forward, Vector3.up).normalized;
        var camRight = Vector3.ProjectOnPlane(MainCam.transform.right, Vector3.up).normalized;
        movementInput = (camRight * normalizedDirection.x + camForward * normalizedDirection.y).normalized;
    }

    private void Update()
    {
        CheckGrounded();
        ApplyGravity();
        
        if (currentMovement.sqrMagnitude > 0.01)
        {
            rigid.rotation = Quaternion.LookRotation(currentMovement);
        }
    }

    private void FixedUpdate()
    {
        currentMovement = Vector3.SmoothDamp(currentMovement, movementInput, ref currentVelocity, smoothTime);
        rigid.linearVelocity = new Vector3(currentMovement.x * speed, rigid.linearVelocity.y, currentMovement.z * speed);
    }

    private void ApplyGravity()
    {
        var gravity = -9.81f * Time.deltaTime;
        
        if (!isGrounded)
        {
            rigid.linearVelocity = Vector3.Max(rigid.linearVelocity,new Vector3(rigid.linearVelocity.x,gravity,rigid.linearVelocity.z));
        }
        else
        {
            rigid.linearVelocity += new Vector3(0,-2,0);
        }
    }
    
    private void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(transform.position, checkSphereSize);
    }
}
