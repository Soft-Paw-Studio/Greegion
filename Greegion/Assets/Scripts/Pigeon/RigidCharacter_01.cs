using System;
using Unity.Mathematics;
using UnityEngine;

public class RigidCharacter_01 : MonoBehaviour
{
    public InputHandler inputHandler;

    private Camera cam;

    private CharacterController controller;
    private Vector3 targetMovement;
    [SerializeField] private float speed;

    private void Awake()
    {
        cam = Camera.main;
        controller = GetComponent<CharacterController>();
        
        inputHandler.Move += InputHandlerOnMove;
    }

    private void InputHandlerOnMove(Vector2 direction)
    {
        var normalizedDirection = direction.normalized;
        var camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        var camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
        targetMovement = (camRight * normalizedDirection.x + camForward * normalizedDirection.y).normalized;
    }

    private void Update()
    {
        controller.SimpleMove(targetMovement * speed);
        //controller.Move(targetMovement * speed * Time.deltaTime);
    }
}
