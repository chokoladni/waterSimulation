// GENERATED AUTOMATICALLY FROM 'Assets/BoatInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @BoatInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @BoatInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""BoatInputActions"",
    ""maps"": [
        {
            ""name"": ""Boat"",
            ""id"": ""6e073d98-ffc8-4afd-b63e-29aca9a7ca14"",
            ""actions"": [
                {
                    ""name"": ""Accelerate"",
                    ""type"": ""PassThrough"",
                    ""id"": ""74fd831c-fa96-4b2e-bb53-dd4768082831"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Rotate"",
                    ""type"": ""PassThrough"",
                    ""id"": ""6397d674-ef93-4664-b77a-475125308ed4"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""04d64d62-0d79-4d9a-ab7d-3ca25ac04331"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Quit"",
                    ""type"": ""Button"",
                    ""id"": ""7421cabb-bd80-47e0-8e6f-2622557205b0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""896fe7db-76df-421c-b275-1ad9c10ba46d"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Accelerate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""38d13317-57a2-44fd-9d5a-7c8cb2db1767"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Accelerate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""c5cc62fe-b266-4f1a-b2e8-f94e67f7b466"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Accelerate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""b48312f9-251f-4f43-8a5e-1bffb8f8babf"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""035e2896-6a1f-4d20-a1f8-e7d60b089337"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""326aa58a-4227-467d-9071-75d693c92c35"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""ebe762f4-4e46-4136-af47-b79c49fca024"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f4a26ed3-c7e6-4b32-851e-15a85b8d617a"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Quit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Boat
        m_Boat = asset.FindActionMap("Boat", throwIfNotFound: true);
        m_Boat_Accelerate = m_Boat.FindAction("Accelerate", throwIfNotFound: true);
        m_Boat_Rotate = m_Boat.FindAction("Rotate", throwIfNotFound: true);
        m_Boat_Pause = m_Boat.FindAction("Pause", throwIfNotFound: true);
        m_Boat_Quit = m_Boat.FindAction("Quit", throwIfNotFound: true);
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

    // Boat
    private readonly InputActionMap m_Boat;
    private IBoatActions m_BoatActionsCallbackInterface;
    private readonly InputAction m_Boat_Accelerate;
    private readonly InputAction m_Boat_Rotate;
    private readonly InputAction m_Boat_Pause;
    private readonly InputAction m_Boat_Quit;
    public struct BoatActions
    {
        private @BoatInputActions m_Wrapper;
        public BoatActions(@BoatInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Accelerate => m_Wrapper.m_Boat_Accelerate;
        public InputAction @Rotate => m_Wrapper.m_Boat_Rotate;
        public InputAction @Pause => m_Wrapper.m_Boat_Pause;
        public InputAction @Quit => m_Wrapper.m_Boat_Quit;
        public InputActionMap Get() { return m_Wrapper.m_Boat; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BoatActions set) { return set.Get(); }
        public void SetCallbacks(IBoatActions instance)
        {
            if (m_Wrapper.m_BoatActionsCallbackInterface != null)
            {
                @Accelerate.started -= m_Wrapper.m_BoatActionsCallbackInterface.OnAccelerate;
                @Accelerate.performed -= m_Wrapper.m_BoatActionsCallbackInterface.OnAccelerate;
                @Accelerate.canceled -= m_Wrapper.m_BoatActionsCallbackInterface.OnAccelerate;
                @Rotate.started -= m_Wrapper.m_BoatActionsCallbackInterface.OnRotate;
                @Rotate.performed -= m_Wrapper.m_BoatActionsCallbackInterface.OnRotate;
                @Rotate.canceled -= m_Wrapper.m_BoatActionsCallbackInterface.OnRotate;
                @Pause.started -= m_Wrapper.m_BoatActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_BoatActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_BoatActionsCallbackInterface.OnPause;
                @Quit.started -= m_Wrapper.m_BoatActionsCallbackInterface.OnQuit;
                @Quit.performed -= m_Wrapper.m_BoatActionsCallbackInterface.OnQuit;
                @Quit.canceled -= m_Wrapper.m_BoatActionsCallbackInterface.OnQuit;
            }
            m_Wrapper.m_BoatActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Accelerate.started += instance.OnAccelerate;
                @Accelerate.performed += instance.OnAccelerate;
                @Accelerate.canceled += instance.OnAccelerate;
                @Rotate.started += instance.OnRotate;
                @Rotate.performed += instance.OnRotate;
                @Rotate.canceled += instance.OnRotate;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @Quit.started += instance.OnQuit;
                @Quit.performed += instance.OnQuit;
                @Quit.canceled += instance.OnQuit;
            }
        }
    }
    public BoatActions @Boat => new BoatActions(this);
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.FindControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    public interface IBoatActions
    {
        void OnAccelerate(InputAction.CallbackContext context);
        void OnRotate(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnQuit(InputAction.CallbackContext context);
    }
}
