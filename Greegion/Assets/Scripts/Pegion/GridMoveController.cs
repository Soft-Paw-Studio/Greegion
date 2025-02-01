using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GridMoveController : MonoBehaviour
{
    //Parameters
    [Header("Move Settings")]
    [Range(0,1)]
    public float moveDuration = 0.5f;
    public LayerMask layer;
    
    [Header("Bounce Settings")]
    [Range(0,2)]
    public float bounceHeight = 1;
    public AnimationCurve bounceCurve;
    
    //Store Values
    private PegionActions input;
    private Collider controllerCollider;
    private List<Vector3> inputBuffer = new();
    
    //States
    private bool isMoving;

    public bool hitWall;
    [FormerlySerializedAs("hitNothing")] public bool hitEmpty;
    public bool higherThanMaxHeight;

    public float maxJumpHeight = 3;

    public Vector3 targetSurface;
    private RaycastHit hitSurface;

    private void CheckMovable()
    {
        var forwardPoint = controllerCollider.bounds.center + transform.forward;
        //There's multiple condition will stop player to move
        //1. Blocked by environment.
        hitWall = Physics.CheckSphere(forwardPoint, 0.1f, layer);
        //2. Is an empty space. e.g. water, void.
        hitEmpty = !Physics.CheckSphere(forwardPoint + Vector3.down, 0.1f, layer);
        //3. Is higher than max jump height.(Will hit the wall something.)
        higherThanMaxHeight = Physics.Raycast(new Ray(forwardPoint + Vector3.up * maxJumpHeight, Vector3.down),
            out hitSurface, Mathf.Infinity, layer);

        targetSurface = hitSurface.point;
    }

    private void Awake()
    {
        input = new PegionActions();
        input.Enable();
        
        //Add input events.
        input.Gameplay.Movement.performed += OnMovementPressed;
        input.Gameplay.Jump.performed += OnJumpPressed;
    }

    private void Start()
    {
        TryGetComponent(out controllerCollider);
        transform.position = SnapToGrid(transform.position);
    }

    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3
        {
            x = Mathf.Round(pos.x),
            y = Mathf.Round(pos.y),
            z = Mathf.Round(pos.z)
        };
    }

    private void OnJumpPressed(InputAction.CallbackContext obj)
    {
        // 处理跳跃逻辑
    }

    private void OnMovementPressed(InputAction.CallbackContext obj)
    {
        //Store input vector into lists.
        var move = input.Gameplay.Movement.ReadValue<Vector2>();
        
        if(move.magnitude < 1) return;
        
        inputBuffer.Add(new Vector3(move.x, 0, move.y));
    }

    private void FixedUpdate()
    {
        if (inputBuffer.Count <= 0 || isMoving) return;
        
        isMoving = true;
        StartCoroutine(MoveTo(inputBuffer[0],moveDuration));
    }

    private IEnumerator MoveTo(Vector3 moveTo,float duration)
    {

        
        var startPosition = transform.position;
        var targetPosition = SnapToGrid(transform.position + moveTo);
        transform.LookAt(targetPosition, Vector3.up);

        CheckMovable();
        
        //Break the coroutine if hit wall.
        if (hitWall || hitEmpty)
        {
            isMoving = false;
            inputBuffer.RemoveAt(0);
            yield break;
        }
        
        float time = 0;
        while (time < duration)
        {
            //Update pigeon height and position
            var targetHeight = ((bounceCurve.Evaluate(time / duration) * bounceHeight) + 1) * targetPosition.y;
            var finalTargetPosition = new Vector3(targetPosition.x, targetHeight, targetPosition.z);
            transform.position = Vector3.Lerp(startPosition, finalTargetPosition, time / duration);
            
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        
        inputBuffer.RemoveAt(0);
        isMoving = false;
    }

    private void OnDrawGizmos()
    {
        if (!controllerCollider) return;
        
        var targetPoint = controllerCollider.bounds.center + transform.forward + Vector3.up * maxJumpHeight;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPoint, 0.1f);
        
        var ray = new Ray
        {
            origin = targetPoint,
            direction = Vector3.down
        };
        
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layer))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(targetPoint,hit.point);
            Gizmos.DrawSphere(hit.point,0.1f);
        }

    }
}