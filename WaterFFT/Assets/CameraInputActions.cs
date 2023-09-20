// GENERATED AUTOMATICALLY FROM 'Assets/CameraInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @CameraInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @CameraInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""CameraInputActions"",
    ""maps"": [
        {
            ""name"": ""Camera"",
            ""id"": ""91e4324b-c6f9-43f7-821e-fecb41da2fab"",
            ""actions"": [
                {
                    ""name"": ""rotate"",
                    ""type"": ""PassThrough"",
                    ""id"": ""6e1d312e-0912-448f-b219-d40ce7fc65dc"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""zoom"",
                    ""type"": ""PassThrough"",
                    ""id"": ""65a32a04-6ef0-4050-aa0c-c857147521da"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""disableMovement"",
                    ""type"": ""Button"",
                    ""id"": ""43c19565-818c-4e66-952f-824ff28429f2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2f129b4f-27f7-416a-b176-98d6806a18a0"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""44c4895c-86d8-46bc-a5f6-cd8f4d99face"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9742a523-5dd9-45f3-9499-95935dc4a8af"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""disableMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Camera
        m_Camera = asset.FindActionMap("Camera", throwIfNotFound: true);
        m_Camera_rotate = m_Camera.FindAction("rotate", throwIfNotFound: true);
        m_Camera_zoom = m_Camera.FindAction("zoom", throwIfNotFound: true);
        m_Camera_disableMovement = m_Camera.FindAction("disableMovement", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Camera
    private readonly InputActionMap m_Camera;
    private ICameraActions m_CameraActionsCallbackInterface;
    private readonly InputAction m_Camera_rotate;
    private readonly InputAction m_Camera_zoom;
    private readonly InputAction m_Camera_disableMovement;
    public struct CameraActions
    {
        private @CameraInputActions m_Wrapper;
        public CameraActions(@CameraInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @rotate => m_Wrapper.m_Camera_rotate;
        public InputAction @zoom => m_Wrapper.m_Camera_zoom;
        public InputAction @disableMovement => m_Wrapper.m_Camera_disableMovement;
        public InputActionMap Get() { return m_Wrapper.m_Camera; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CameraActions set) { return set.Get(); }
        public void SetCallbacks(ICameraActions instance)
        {
            if (m_Wrapper.m_CameraActionsCallbackInterface != null)
            {
                @rotate.started -= m_Wrapper.m_CameraActionsCallbackInterface.OnRotate;
                @rotate.performed -= m_Wrapper.m_CameraActionsCallbackInterface.OnRotate;
                @rotate.canceled -= m_Wrapper.m_CameraActionsCallbackInterface.OnRotate;
                @zoom.started -= m_Wrapper.m_CameraActionsCallbackInterface.OnZoom;
                @zoom.performed -= m_Wrapper.m_CameraActionsCallbackInterface.OnZoom;
                @zoom.canceled -= m_Wrapper.m_CameraActionsCallbackInterface.OnZoom;
                @disableMovement.started -= m_Wrapper.m_CameraActionsCallbackInterface.OnDisableMovement;
                @disableMovement.performed -= m_Wrapper.m_CameraActionsCallbackInterface.OnDisableMovement;
                @disableMovement.canceled -= m_Wrapper.m_CameraActionsCallbackInterface.OnDisableMovement;
            }
            m_Wrapper.m_CameraActionsCallbackInterface = instance;
            if (instance != null)
            {
                @rotate.started += instance.OnRotate;
                @rotate.performed += instance.OnRotate;
                @rotate.canceled += instance.OnRotate;
                @zoom.started += instance.OnZoom;
                @zoom.performed += instance.OnZoom;
                @zoom.canceled += instance.OnZoom;
                @disableMovement.started += instance.OnDisableMovement;
                @disableMovement.performed += instance.OnDisableMovement;
                @disableMovement.canceled += instance.OnDisableMovement;
            }
        }
    }
    public CameraActions @Camera => new CameraActions(this);
    public interface ICameraActions
    {
        void OnRotate(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
        void OnDisableMovement(InputAction.CallbackContext context);
    }
}
