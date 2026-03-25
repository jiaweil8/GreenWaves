using UnityEngine;
using Cinemachine;

namespace KairaDigitalArts {
    public class UCC_CinemachineCamera : MonoBehaviour
    {
        public UCC_CarController carController;
        public UCC_InputSystem inputSystem;
        public GameObject playerCar;
        public Camera mainCam;

        public CinemachineVirtualCamera[] virtualCameras;
        private int currentCameraIndex = 0;

        private float elapsedTime;
        private float targetTime = 4f;
        bool transactionComplete;
        bool isGearChanged;

        private float targetDistance;
        private void Update()
        {
            float epsilon = 0.01f;
            ChangeCamera();
            if (currentCameraIndex != 1)
            {
                var thirdPersonFollow = virtualCameras[currentCameraIndex].GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                if (thirdPersonFollow != null)
                {
                    if (isGearChanged)
                    {
                        elapsedTime += Time.deltaTime;

                        float transitionSpeed = Mathf.SmoothStep(0, 1, elapsedTime / targetTime);
                        thirdPersonFollow.CameraDistance = Mathf.Lerp(
                            thirdPersonFollow.CameraDistance,
                            targetDistance,
                            transitionSpeed
                        );

                        if (Mathf.Abs(thirdPersonFollow.CameraDistance - targetDistance) < epsilon)
                        {
                            isGearChanged = false;
                            elapsedTime = 0f;
                            thirdPersonFollow.CameraDistance = targetDistance;
                        }
                    }
                    else
                    {
                        targetDistance = Mathf.Abs(thirdPersonFollow.CameraDistance);
                        if (carController.currentGear < 0)
                        {
                            targetDistance *= -1;
                        }
                        isGearChanged = true;
                    }
                }
            }
        }

        void ChangeCamera()
        {
            if (inputSystem.controller.Driving.ChangeCamera.triggered)
            {
                if (mainCam.GetComponent<UCC_CinemachineCamera>() != null)
                {
                    virtualCameras[currentCameraIndex].Priority = 0;
                    currentCameraIndex = (currentCameraIndex + 1) % virtualCameras.Length;
                    virtualCameras[currentCameraIndex].Priority = 10;
                }
            }
        }
    }
}
