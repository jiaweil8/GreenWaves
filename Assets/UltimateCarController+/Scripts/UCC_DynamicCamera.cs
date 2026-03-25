using UnityEngine;

namespace KairaDigitalArts
{
    public class UCC_DynamicCamera : MonoBehaviour
    {
        public UCC_CarController carController;
        public UCC_InputSystem inputSystem;

        public Transform car;
        public Transform chaseCameraLocation;
        public Transform actionCameraLocation;
        public Transform cockpitCameraLocation;
        private Transform[] camLocations;
        [Range(0f, 20f)]
        public float smoothTime;
        public int locationIndicator = 0;

        private Vector3 velocity = Vector3.zero;

        private void Start()
        {
            camLocations = new Transform[]
            {
                chaseCameraLocation,
                actionCameraLocation,
                cockpitCameraLocation
            };
        }

        private void LateUpdate()
        {
            if (inputSystem.controller.Driving.ChangeCamera.triggered)
            {
                locationIndicator = (locationIndicator + 1) % camLocations.Length;
            }

            if (locationIndicator == 1)
            {
                ActionCameraBehavior();
            }
            else
            {
                StaticCameraBehaviour();
            }
        }

        private void ActionCameraBehavior()
        {
            Vector3 targetPosition = camLocations[1].position;

            float adjustedSmoothTime = smoothTime;

            if (carController.driftOffset > 0.3f)
            {
                adjustedSmoothTime *= carController.driftOffset * 7 ;

                Vector3 offset = (targetPosition - car.position).normalized * 2f;
                targetPosition = car.position + offset*2;
            }
            else
            {
                adjustedSmoothTime = smoothTime;
            }
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, adjustedSmoothTime * Time.deltaTime);
            transform.LookAt(car);
        }
        private void StaticCameraBehaviour()
        {
            transform.position = camLocations[locationIndicator].position;
            if(locationIndicator != 2) 
            { 
            transform.LookAt(car);
            }
            else
            {
                transform.rotation = car.rotation;
            }
        }
    }
}
