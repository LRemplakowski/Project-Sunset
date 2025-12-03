using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SunsetSystems.Input
{
    public class InputActionResponse : SerializedMonoBehaviour
    {
        [SerializeField]
        private InputActionReference _inputActionReference;
        [SerializeField]
        private UltEvent<InputAction.CallbackContext> _onStarted;
        [SerializeField]
        private UltEvent<InputAction.CallbackContext> _onPerformed;
        [SerializeField]
        private UltEvent<InputAction.CallbackContext> _onCanceled;

        private void OnEnable()
        {
            if (_inputActionReference == null) return;

            _inputActionReference.action.started += ActionStarted;
            _inputActionReference.action.performed += ActionPerformed;
            _inputActionReference.action.canceled += ActionCanceled;
        }

        private void OnDisable()
        {
            if (_inputActionReference == null) return;

            _inputActionReference.action.started -= ActionStarted;
            _inputActionReference.action.performed -= ActionPerformed;
            _inputActionReference.action.canceled -= ActionCanceled;
        }

        private void ActionStarted(InputAction.CallbackContext context)
        {
            _onStarted?.Invoke(context);
        }

        private void ActionPerformed(InputAction.CallbackContext context)
        {
            _onPerformed?.Invoke(context);
        }

        private void ActionCanceled(InputAction.CallbackContext context)
        {
            _onCanceled?.Invoke(context);
        }
    }
}
