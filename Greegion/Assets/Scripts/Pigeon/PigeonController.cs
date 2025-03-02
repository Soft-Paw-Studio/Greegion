using System;
using Pigeon.States;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pigeon
{
    public class PigeonController : MonoBehaviour,IEffectReceiver
    {
        [ShowInInspector][ReadOnly]
        public string CurrentState;
        public InputHandler inputHandler ;        
        public PigeonData data;

        private Camera MainCam { get; set; }
        public StateManager StateManager { get; private set; }
        public CharacterController Controller { get; private set; }

        public Vector3 targetMovement { get; private set; }
        private Vector3 currentVelocity;
        private Vector3 currentMovement;
        internal float verticalVelocity;

        internal float gravity = -9.81f;

        private void Awake()
        {
            MainCam = Camera.main;
            Controller = GetComponent<CharacterController>();
            
            //StateManager = new StateManager(this);
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
            
            StateManager.Update();
            MoveCharacter();

        }

        private void HandleMove(Vector2 direction)
        {
            var normalizedDirection = direction.normalized;
            var camForward = Vector3.ProjectOnPlane(MainCam.transform.forward, Vector3.up).normalized;
            var camRight = Vector3.ProjectOnPlane(MainCam.transform.right, Vector3.up).normalized;
            targetMovement = (camRight * normalizedDirection.x + camForward * normalizedDirection.y).normalized;
        }
        
        private void HandleJump()
        {
            StateManager.ChangeState<JumpState>();
        }
        
        private void MoveCharacter()
        {
            currentMovement = Vector3.SmoothDamp(currentMovement, targetMovement, ref currentVelocity, data.smoothTime);
            var finalMove = currentMovement * data.speed + Vector3.up * verticalVelocity;
            Controller.Move(finalMove * Time.deltaTime);

            if (currentMovement.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(currentMovement);
            }
        }
        //verticalVelocity = GravityManager.Instance.GetGravityEffect(this,verticalVelocity, Controller.isGrounded);

        public void ApplyEffect(EffectType effectType, float intensity, float duration)
        {
            throw new NotImplementedException();
        }

        public void RemoveEffect(EffectType effectType)
        {
            throw new NotImplementedException();
        }
    }
}
