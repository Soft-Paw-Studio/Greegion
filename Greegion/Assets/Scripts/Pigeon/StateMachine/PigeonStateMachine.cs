using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pigeon.StateMachine
{
    [Serializable]
    public class PigeonStateMachine
    {
        public BaseState CurrentState;
        private PigeonController character;
        private Dictionary<Type, BaseState> stateDictionary = new();
        
        public PigeonStateMachine(PigeonController character)
        {
            this.character = character;
        }
        
        internal void ChangeState<T>() where T : BaseState, new()
        {
            CurrentState?.ExitState();

            if (!stateDictionary.TryGetValue(typeof(T), out BaseState newState))
            {
                // 按需创建并缓存状态
                newState = new T();
                stateDictionary[typeof(T)] = newState;
                newState.InitialState(character);
            }

            CurrentState = newState;
            CurrentState.EnterState();
        }
    }
    
}
