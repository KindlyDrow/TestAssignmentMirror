using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event Action OnDashPerfomed;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this; 
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Dash.performed += Dash_performed;
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnDashPerfomed?.Invoke();
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Dash.performed -= Dash_performed;
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }
}
