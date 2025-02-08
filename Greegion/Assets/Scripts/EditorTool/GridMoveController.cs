using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class GridMoveController : MonoBehaviour
{
    [SerializeField] private ParticleSystem bounceEffect;
    
    [Header("Movement Settings")]
    [SerializeField, Range(0, 1)] private float moveDuration = 0.5f;
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField, Range(0, 10)] private float bounceHeight = 1f;
    [SerializeField] private AnimationCurve bounceCurve;
    [SerializeField] private float maxJumpHeight = 3f;
    
    private PegionActions inputActions;
    private Collider characterCollider;
    private Queue<Vector3> moveInputQueue = new();
    private bool isMoving;
    
    // Movement state flags
    private bool IsPathBlocked { get; set; }
    private bool IsGroundMissing { get; set; }
    private bool IsHeightExceeded { get; set; }
    private Vector3 targetSurfacePoint;

    private void Awake()
    {
        inputActions = new PegionActions();
        inputActions.Enable();
        inputActions.Gameplay.Movement.performed += HandleMovementInput;
        TryGetComponent(out characterCollider);
    }

    private void Start()
    {
        transform.position = SnapPositionToGrid(transform.position);
    }

    private void OnDestroy()
    {
        inputActions.Disable();
        inputActions.Gameplay.Movement.performed -= HandleMovementInput;
    }

    private void HandleMovementInput(InputAction.CallbackContext context)
    {
        Vector2 input = inputActions.Gameplay.Movement.ReadValue<Vector2>();
        if (input.magnitude < 0.5f) return;
        
        moveInputQueue.Enqueue(new Vector3(input.x, 0, input.y));
    }

    private void FixedUpdate()
    {
        if (moveInputQueue.Count == 0 || isMoving) return;
        
        Vector3 nextMove = moveInputQueue.Peek();
        if (CanMove(nextMove))
        {
            isMoving = true;
            StartCoroutine(ExecuteMovement(nextMove));
        }
        else
        {
            moveInputQueue.Dequeue(); // Clear invalid move
        }
    }

    private bool CanMove(Vector3 moveDirection)
    {
        Vector3 forwardPoint = characterCollider.bounds.center + moveDirection;
        Vector3 heightCheckPoint = forwardPoint + Vector3.up * maxJumpHeight;

        // Check movement constraints
        IsPathBlocked = Physics.CheckSphere(forwardPoint, 0.1f, collisionLayer);
        IsGroundMissing = !Physics.Raycast(forwardPoint, Vector3.down, out RaycastHit groundHit, Mathf.Infinity, collisionLayer);
        IsHeightExceeded = Physics.CheckSphere(heightCheckPoint, 0.1f, collisionLayer);

        // Find landing point if movement is possible
        if (!IsHeightExceeded && Physics.Raycast(heightCheckPoint, Vector3.down, out RaycastHit surfaceHit, Mathf.Infinity, collisionLayer))
        {
            targetSurfacePoint = surfaceHit.point;
            return true;
        }

        return !IsPathBlocked && !IsGroundMissing;
    }

    private IEnumerator ExecuteMovement(Vector3 moveDirection)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = SnapPositionToGrid(startPosition + moveDirection);
        targetPosition.y = Mathf.Ceil(targetSurfacePoint.y);

        // Face movement direction
        Vector3 lookPosition = targetPosition;
        transform.LookAt(lookPosition, Vector3.up);
        transform.rotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y,transform.rotation.eulerAngles.z);

        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / moveDuration;
            
            // Calculate arc movement
            Vector3 midPoint = CalculateArcMidPoint(startPosition, targetPosition);
            
            Vector3 currentPosition = CalculateBezierPoint
            (
                normalizedTime,
                startPosition,
                midPoint,
                targetPosition
            );

            transform.position = Vector3.Lerp
            (
                transform.position,
                currentPosition,
                bounceCurve.Evaluate(normalizedTime)
            );

            yield return null;
        }

        // Ensure exact final position
        transform.position = targetPosition;
        bounceEffect.Play();

        moveInputQueue.Dequeue();
        isMoving = false;
    }

    private Vector3 CalculateArcMidPoint(Vector3 start, Vector3 end)
    {
        Vector3 midPoint = (start + end) * 0.5f;
        midPoint.y += bounceHeight;
        return midPoint;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 start, Vector3 mid, Vector3 end)
    {
        float oneMinusT = 1f - t;
        return (oneMinusT * oneMinusT * start) + 
               (2f * oneMinusT * t * mid) + 
               (t * t * end);
    }

    private Vector3 SnapPositionToGrid(Vector3 position)
    {
        return new Vector3
        (
            Mathf.Round(position.x),
            Mathf.Round(position.y),
            Mathf.Round(position.z)
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IFood collectable))
        {
            collectable.Collect();
            Debug.Log($"Collected: {other.name}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!characterCollider) return;

        Vector3 forwardPoint = characterCollider.bounds.center + transform.forward;
        Vector3 heightCheckPoint = forwardPoint + Vector3.up * maxJumpHeight;

        // Draw height check point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(heightCheckPoint, 0.1f);

        // Draw ground check ray
        if (Physics.Raycast(heightCheckPoint, Vector3.down, out RaycastHit hit, Mathf.Infinity, collisionLayer))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(heightCheckPoint, hit.point);
            Gizmos.DrawSphere(hit.point, 0.1f);
        }
    }
}