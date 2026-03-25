using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KairaDigitalArts {
    public class UCC_InputSystem : MonoBehaviour
    {
        [Header("Input Values")]
        [SerializeField] public float steerInput;
        [SerializeField] public float gasInput;
        [SerializeField] public float brakeInput;
        [SerializeField] public float clutchInput;
        [SerializeField] public float n2oInput;
        [SerializeField] public float handbrakeInput;

        [Range(0.1f, 1f)] public float brakeLinearity = 1;
        [Range(0.1f, 1f)] public float steerLinearity = 1;
        [Range(0.1f, 1f)] public float throttleLinearity = 1;

        public UCC_PlayerController controller;

        private const float zeroThreshold = 1e-3f;

        [Header("UI Buttons")]
        public bool mobileControllerEnabled;
        [HideInInspector]
        public Button gasButton;
        [HideInInspector]
        public Button brakeButton;
        [HideInInspector]
        public Button handbrakeButton;
        [HideInInspector]
        public Button nitroButton;
        [HideInInspector]
        public Button steerleft;
        [HideInInspector]
        public Button steerRight;

        private void Awake()
        {
            controller = new UCC_PlayerController();
        }

        private void OnEnable()
        {
            controller.Driving.Enable();

            if (mobileControllerEnabled)
            {
                AttachButtonEvents(gasButton, "Gas");
                AttachButtonEvents(brakeButton, "Brake");
                AttachButtonEvents(handbrakeButton, "Handbrake");
                AttachButtonEvents(nitroButton, "Nitro");
                AttachButtonEvents(steerleft, "SteerLeft");
                AttachButtonEvents(steerRight, "SteerRight");
            }
        }

        private void OnDisable()
        {
            controller.Driving.Disable();

            if (mobileControllerEnabled)
            {
                RemoveButtonEvents(gasButton);
                RemoveButtonEvents(brakeButton);
                RemoveButtonEvents(handbrakeButton);
                RemoveButtonEvents(nitroButton);
                RemoveButtonEvents(steerleft);
                RemoveButtonEvents(steerRight);
            }
        }

        private void Update()
        {
            ControlTouch();
        }
        private void AttachButtonEvents(Button button, string inputType)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }
            var pointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            pointerDown.callback.AddListener((data) => HandleUIButtonInput(inputType, true));
            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            pointerUp.callback.AddListener((data) => HandleUIButtonInput(inputType, false));
            trigger.triggers.Add(pointerUp);
        }

        private void RemoveButtonEvents(Button button)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                trigger.triggers.Clear();
            }
        }

        public void GetAxis()
        {
            if (!mobileControllerEnabled)
            {
                float targetSteerInput = controller.Driving.Steer.ReadValue<float>();
                if (Mathf.Abs(targetSteerInput) > zeroThreshold)
                {
                    if (targetSteerInput > 0)
                    {
                        steerInput += targetSteerInput * steerLinearity / 45;
                        steerInput = Mathf.Min(steerInput, targetSteerInput);
                    }
                    else
                    {
                        steerInput += targetSteerInput * steerLinearity / 45;
                        steerInput = Mathf.Max(steerInput, targetSteerInput);
                    }
                }
                else
                {
                    steerInput = 0f;
                }
                steerInput = Mathf.Clamp(steerInput, -1f, 1f);

                float targetGasInput = controller.Driving.Throttle.ReadValue<float>();
                if (Mathf.Abs(targetGasInput) > zeroThreshold)
                {
                    gasInput += targetGasInput * throttleLinearity / 90;
                    gasInput = Mathf.Min(gasInput, targetGasInput);
                }
                else
                {
                    gasInput = 0f;
                }
                gasInput = Mathf.Clamp(gasInput, 0f, 1f);

                float targetBrakeInput = controller.Driving.Brake.ReadValue<float>();
                if (Mathf.Abs(targetBrakeInput) > zeroThreshold)
                {
                    brakeInput += targetBrakeInput * brakeLinearity / 90;
                    brakeInput = Mathf.Min(brakeInput, targetBrakeInput);
                }
                else
                {
                    brakeInput = 0f;
                }
                brakeInput = Mathf.Clamp(brakeInput, 0f, 1f);

                clutchInput = controller.Driving.Clutch.ReadValue<float>();
                handbrakeInput = controller.Driving.Handbrake.ReadValue<float>();
                n2oInput = controller.Driving.N2O.ReadValue<float>();
            }
        }

        public void HandleUIButtonInput(string inputType, bool isPressed)
        {
            switch (inputType)
            {
                case "Gas":
                    gasInput = isPressed ? 1f : 0f;
                    break;
                case "Brake":
                    brakeInput = isPressed ? 1f : 0f;
                    break;
                case "Handbrake":
                    handbrakeInput = isPressed ? 1f : 0f;
                    break;
                case "Nitro":
                    n2oInput = isPressed ? 1f : 0f;
                    break;
                case "SteerLeft":
                    steerInput = isPressed ? -1f : 0f;
                    break;
                case "SteerRight":
                    steerInput = isPressed ? 1f : 0f;
                    break;
                default:
                    break;
            }
        }

        void ControlTouch()
        {
            if (mobileControllerEnabled && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    ProcessTouch(touch.position);
                }
            }
        }

        private void ProcessTouch(Vector2 touchPosition)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = touchPosition;

            RaycastResult raycastResult = RaycastUI(pointerData);
            if (raycastResult.gameObject != null)
            {
                Button button = raycastResult.gameObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                }
            }
        }

        private RaycastResult RaycastUI(PointerEventData pointerData)
        {
            GraphicRaycaster raycaster = FindObjectOfType<GraphicRaycaster>();
            if (raycaster != null)
            {
                var results = new System.Collections.Generic.List<RaycastResult>();
                raycaster.Raycast(pointerData, results);

                if (results.Count > 0)
                {
                    return results[0];
                }
            }
            return new RaycastResult();
        }
    }
}
