namespace Pigeon
{
    public abstract class PigeonState
    {
        protected StateManager StateManager;
        protected PigeonController Controller;

        // 进入状态时调用
        public virtual void Enter(PigeonController pigeon)
        {
            Controller = pigeon;
        }

        // 每帧调用
        public virtual void Update()
        {
            
        }

        // 退出状态时调用
        public virtual void Exit()
        {
            
        }
    }
}
