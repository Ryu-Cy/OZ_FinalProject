using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// New Input System 이벤트를 수신하여 가공된 입력 데이터를 전달하는 컨트롤러
/// </summary>
public class PlayerInputController : MonoBehaviour
{
    private PlayerInputActions inputActions;

    // 프로퍼티
    public Vector2 InputVector { get; private set; }
    public bool IsWalkPressed { get; private set; }
    public bool IsSprintPressed { get; private set; }

    // 이벤트
    public event Action OnDodgeTriggered;
    public event Action OnAttackTriggered;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;

        inputActions.Player.Walk.performed += OnWalkPerformed;
        inputActions.Player.Walk.canceled += OnWalkCanceled;

        inputActions.Player.Sprint.performed += OnSprintPerformed;
        inputActions.Player.Sprint.canceled += OnSprintCanceled; 
        
        inputActions.Player.Dodge.performed += OnDodgePerformed;
        inputActions.Player.Attack.performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Player.Walk.performed -= OnWalkPerformed;
        inputActions.Player.Walk.canceled -= OnWalkCanceled;

        inputActions.Player.Sprint.performed -= OnSprintPerformed;
        inputActions.Player.Sprint.canceled -= OnSprintCanceled;

        inputActions.Player.Dodge.performed -= OnDodgePerformed;
        inputActions.Player.Attack.performed -= OnAttackPerformed;

        inputActions.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context) => InputVector = context.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext context) => InputVector = Vector2.zero;

    private void OnWalkPerformed(InputAction.CallbackContext context) => IsWalkPressed = true;
    private void OnWalkCanceled(InputAction.CallbackContext context) => IsWalkPressed = false;

    private void OnSprintPerformed(InputAction.CallbackContext context) => IsSprintPressed = true;
    private void OnSprintCanceled(InputAction.CallbackContext context) => IsSprintPressed = false;

    private void OnDodgePerformed(InputAction.CallbackContext context) => OnDodgeTriggered?.Invoke();
    private void OnAttackPerformed(InputAction.CallbackContext context) => OnAttackTriggered?.Invoke();
}