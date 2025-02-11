using System;
using System.Collections.Generic;

namespace Pigeon
{
    public class StateManager
    {
        public PigeonState CurrentState { get; private set; }
        private PigeonController controller;
        private Dictionary<Type, PigeonState> stateDictionary = new();
        
        public StateManager(PigeonController controller)
        {
            this.controller = controller;
        }

        public void ChangeState<T>() where T : PigeonState, new()
        {
            // 退出当前状态
            CurrentState?.Exit();

            // 获取或创建新状态
            if (!stateDictionary.TryGetValue(typeof(T), out var newState))
            {
                newState = new T();
                stateDictionary[typeof(T)] = newState;
            }

            CurrentState = newState;

            // 进入新状态
            CurrentState?.Enter(controller);
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }
}
