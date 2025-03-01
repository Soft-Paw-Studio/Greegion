using System;
using UnityEngine;

public class PController : MonoBehaviour
{
    public Camera MainCam;
    public InputHandler input;
    public Rigidbody rigid;
    public Vector3 targetMovement;
    public Vector3 inputDirection;
    public float speed;
    public float currentVerticalSpeed;

    private void Awake()
    {
        MainCam = Camera.main;
        input.Move += InputOnMove;
        input.Jump += InputOnJump;
    }

    private void InputOnJump()
    {
        if (isGrounded)
        {
            currentVerticalSpeed = jumpSpeed;
        }
    }

    Vector3 lastVelocity = Vector3.zero;
    [SerializeField] private float jumpSpeed = 10;
    [SerializeField] private bool isGrounded;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private bool saveLastVelo;
    [SerializeField] private float airControlRate;

    private void InputOnMove(Vector2 dir)
    {
        var normalizedDirection = dir.normalized;
        var camForward = Vector3.ProjectOnPlane(MainCam.transform.forward, Vector3.up).normalized;
        var camRight = Vector3.ProjectOnPlane(MainCam.transform.right, Vector3.up).normalized;
        targetMovement = (camRight * normalizedDirection.x + camForward * normalizedDirection.y).normalized;
    }

    private void FixedUpdate()
    {
        Vector3 _velocity = Vector3.zero;
        
        _velocity += targetMovement * speed;
        
        if (!isGrounded)
        {
            _velocity *= airControlRate;
            currentVerticalSpeed -= gravity * Time.deltaTime;
        }
        else
        {
            if (currentVerticalSpeed <= 0f)
                currentVerticalSpeed = 0f;
        }
        
        _velocity += Vector3.up * currentVerticalSpeed;

        if (saveLastVelo)
        {
            //Save current velocity for next frame;
            lastVelocity = _velocity;

            rigid.linearVelocity = lastVelocity;
        }
        else
        {
            rigid.linearVelocity = _velocity;
        }

    }

    private void Update()
    {
        CheckIsGrounded();
    }

    private void CheckIsGrounded()
    {
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, LayerMask.GetMask("Ground"));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,0.1f);
    }
}
