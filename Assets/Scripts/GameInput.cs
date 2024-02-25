using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";
    private const string ANY_KEY = "Any Key";

    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;

    public enum Binding
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        InteractAlternate,
        Pause,
        Gamepad_Interact,
        Gamepad_InteractAlternate,
        Gamepad_Pause
    }

    private PlayerInputActions playerInputActions;
    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }

        // gameInput = GameInput.Instance; // Asegúrate de obtener la instancia correcta de GameInput.
        // LoadCurrentBindings();

        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose();
    }

    private void Pause_performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_performed(InputAction.CallbackContext context)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.Move_Up:
                return playerInputActions.Player.Move.bindings[1].ToDisplayString();
            case Binding.Move_Down:
                return playerInputActions.Player.Move.bindings[2].ToDisplayString();
            case Binding.Move_Left:
                return playerInputActions.Player.Move.bindings[3].ToDisplayString();
            case Binding.Move_Right:
                return playerInputActions.Player.Move.bindings[4].ToDisplayString();
            case Binding.Interact:
                return playerInputActions.Player.Interact.bindings[0].ToDisplayString();
            case Binding.InteractAlternate:
                return playerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString();
            case Binding.Pause:
                return playerInputActions.Player.Pause.bindings[0].ToDisplayString();
            case Binding.Gamepad_Interact:
                return playerInputActions.Player.Interact.bindings[1].ToDisplayString();
            case Binding.Gamepad_InteractAlternate:
                return playerInputActions.Player.InteractAlternate.bindings[1].ToDisplayString();
            case Binding.Gamepad_Pause:
                return playerInputActions.Player.Pause.bindings[1].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, Action onActionRebound)
    {
        playerInputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;
        string previousBinding;

        switch (binding)
        {
            default:
            case Binding.Move_Up:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.Move_Down:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.Move_Left:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.Move_Right:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 4;
                break;
            case Binding.Interact:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.InteractAlternate:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 0;
                break;
            case Binding.Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 0;
                break;
            case Binding.Gamepad_Interact:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 1;
                break;
            case Binding.Gamepad_InteractAlternate:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 1;
                break;
            case Binding.Gamepad_Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 1;
                break;
        }

        previousBinding = inputAction.bindings[bindingIndex].ToDisplayString();

        // Initially, we attempted to use the callback.Cancel() method to revert the key rebinding operation
        // when a duplicate key was detected. The expectation was that Cancel() would undo any changes made
        // during the interactive rebinding process. However, we found that callback.Cancel() did not effectively revert the changes
        // in our context, leaving the system in a state where the newly assigned duplicate key was temporarily applied.
        //
        // To properly handle duplicate key rebinding and ensure the system's state remained consistent,
        // we decided to implement an alternative solution. Instead of relying on Cancel() to revert changes,
        // we now explicitly save the previous key binding before starting the rebinding process.
        // If a duplicate key is detected, we manually apply a key binding override to restore the previous key.
        // This solution ensures that only valid changes are saved and maintains the consistency of our key binding system.
        //
        // This change was implemented as a practical solution to the unexpected behavior of callback.Cancel()
        // and to enhance the robustness of our key rebinding logic.
        inputAction.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("<Keyboard>/escape")
            .WithControlsExcluding("<Gamepad>/start")
            .WithCancelingThrough("<Keyboard>/escape")
            .WithCancelingThrough("<Gamepad>/start")
            .OnMatchWaitForAnother(.1f)
            .OnCancel(callback =>
            {
                playerInputActions.Player.Enable();
                callback.Dispose();
                onActionRebound();
            })
            .OnComplete(callback =>
            {
                // TODO: Differentiate between keyboard and gamepad buttons
                string newKey = callback.selectedControl.displayName;
                Debug.Log(newKey);
                if (!IsKeyAlreadyBound(newKey, binding) && !newKey.Equals(ANY_KEY, StringComparison.OrdinalIgnoreCase))
                {
                    playerInputActions.Player.Enable();

                    PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInputActions.SaveBindingOverridesAsJson());
                    PlayerPrefs.Save();

                    OnBindingRebind?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    inputAction.ApplyBindingOverride(bindingIndex, previousBinding);
                    playerInputActions.Player.Enable();

                }
                callback.Dispose();
                onActionRebound();
            })
            .Start();
        // playerInputActions.RemoveAllBindingOverrides(); // Reset all keybindings saved
    }

    public bool IsKeyAlreadyBound(string newBinding, Binding bindingToRebind)
    {
        foreach (Binding binding in Enum.GetValues(typeof(Binding)))
        {
            if (binding != bindingToRebind)
            {
                string currentBindingText = GetBindingText(binding);
                if (newBinding.Equals(currentBindingText, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }

}
