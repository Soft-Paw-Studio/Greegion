using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pigeon.StateMachine
{
    public class PigeonController : Singleton<PigeonController>
    {
        [ShowInInspector] 
        public BaseState state;
        
        internal PigeonStateMachine stateMachine;
        internal PegionActions input;
        internal CharacterController controller;

        
        internal Vector3 targetMovement;
        internal Vector3 currentMovement;
        internal Vector3 currentVelocity;
        internal Vector3 inputDirection;
        
        protected override void Awake()
        {
            base.Awake();
            input = new PegionActions();
            input.Enable();

            TryGetComponent(out controller);
            stateMachine = new PigeonStateMachine(this);
        }

        private void Start() => stateMachine.ChangeState<IdleState>();

        private void Update()
        {
            CalculateMoveDirection();
            state = stateMachine.CurrentState;
            stateMachine.CurrentState.UpdateState();
        }

        private void CalculateMoveDirection()
        {
            var i = input.Gameplay.Movement.ReadValue<Vector2>();
            inputDirection = new Vector3(i.x, 0, i.y);
        }

        private void OnTriggerEnter(Collider other) => stateMachine.CurrentState.OnTriggerEnter(other);
        private void OnControllerColliderHit(ControllerColliderHit hit) => stateMachine.CurrentState.OnControllerHit(hit);
    }
}
