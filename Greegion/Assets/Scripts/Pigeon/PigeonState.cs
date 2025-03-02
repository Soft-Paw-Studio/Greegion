using UnityEngine;

namespace Pigeon
{
    public abstract class PigeonState
    {
        protected StateManager StateManager;
        protected PigeonCharacterController Controller;
        protected Rigidbody rb;
        
        public virtual void Enter(PigeonCharacterController pigeon)
        {
            Controller = pigeon;
            rb = pigeon.rigidbody;
        }
        public virtual void Update()
        {
            
        }
        public virtual void Exit()
        {
            
        }

        public virtual void HandleInput()
        {
            
        }

        public virtual void FixedUpdate()
        {
        }
    }
}
