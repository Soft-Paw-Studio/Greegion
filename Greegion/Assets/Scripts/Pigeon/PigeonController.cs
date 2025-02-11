using System;
using Pigeon.States;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pigeon
{
    public class PigeonController : MonoBehaviour
    {
        public float gravity = -9.81f;
        public Camera mainCam;
        public PigeonData data;
        public StateManager StateManager;
        public InputHandler inputHandler ;
        public CharacterController controller;
        
        public Vector3 MoveDirection { get; private set; }
        public Vector3 targetPosition;
        private Vector3 currentVelocity;
        private Vector3 currentMovement;
        public float verticalVelocity;

        [ShowInInspector][ReadOnly]
        public string CurrentState;
        
        private void Awake()
        {
            mainCam = Camera.main;
            
            StateManager = new StateManager(this);
            StateManager.ChangeState<IdleState>();
            
            inputHandler.EnableInput();
            inputHandler.Move += HandleMove;
            inputHandler.Jump += HandleJump;
        }

        private void OnDestroy()
        {
            inputHandler.Move -= HandleMove;
            inputHandler.Jump -= HandleJump;
        }
        
        private void Update()
        {
            CurrentState = StateManager.CurrentState.GetType().ToString();
            
            ApplyGravity(); 
            StateManager.Update();
            MoveCharacter();
        }

        private void HandleMove(Vector2 direction)
        {
            var normalizedDirection = direction.normalized;
            var camForward = Vector3.ProjectOnPlane(mainCam.transform.forward, Vector3.up).normalized;
            var camRight = Vector3.ProjectOnPlane(mainCam.transform.right, Vector3.up).normalized;
            MoveDirection = (camRight * normalizedDirection.x + camForward * normalizedDirection.y).normalized;
        }
        
        private void HandleJump()
        {
            StateManager.ChangeState<JumpState>();
        }
        
        private void MoveCharacter()
        {
            currentMovement = Vector3.SmoothDamp(currentMovement, MoveDirection, ref currentVelocity, data.smoothTime);
            var finalMove = currentMovement * data.speed + Vector3.up * verticalVelocity;
            controller.Move(finalMove * Time.deltaTime);

            if (currentMovement.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(currentMovement);
            }
        }
        
        private void ApplyGravity()
        {
            verticalVelocity = GravityManager.Instance.GetGravityEffect(this,verticalVelocity, controller.isGrounded);
        }
        
        public void SetTargetPosition(Vector3 targetPosition)
        {
            this.targetPosition = targetPosition;
        }
    }
}
