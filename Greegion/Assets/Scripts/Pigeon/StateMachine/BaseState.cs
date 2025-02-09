using UnityEngine;

namespace Pigeon.StateMachine
{
    [System.Serializable]
    public abstract class BaseState
    {
        internal PigeonController C;
        internal PegionActions input;
        internal PigeonStateMachine stateMachine;
        
        protected internal void InitialState(PigeonController character)
        {
            C = character;
            input = character.input;
            stateMachine = C.stateMachine;
        }
        
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public virtual void OnControllerHit(ControllerColliderHit hit)
        {
        }

        public virtual void OnTriggerEnter(Collider other)
        {
        }

        public virtual void OnTriggerStay()
        {
        }

        public virtual void OnTriggerExit()
        {
        }
        
        protected void MoveCharacter(Vector3 movement)
        {
            C.currentMovement = Vector3.SmoothDamp(C.currentMovement, movement, ref C.currentVelocity, 0.2f);
            C.controller.Move(C.currentMovement * Time.deltaTime * 3);
        }
    }
}
