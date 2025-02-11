using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Create InputHandler", fileName = "InputHandler", order = 0)]
public class InputHandler : ScriptableObject,PigeonInput.IGameplayActions
{
    private PigeonInput controls;

    public event Action<Vector2> Move;
    public event Action Jump;

    public void EnableInput()
    {
        controls.Enable();
    }

    public void DisableInput()
    {
        controls.Disable();
    }
    
    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new PigeonInput();
            controls.Gameplay.SetCallbacks(this);
        }
        controls.Enable();
    }
    
    private void OnDisable()
    {
        controls.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Jump?.Invoke();
        }
    }
}
