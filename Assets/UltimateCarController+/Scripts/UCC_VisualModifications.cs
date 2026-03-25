using System.Collections.Generic;
using UnityEngine;

namespace KairaDigitalArts
{
    [ExecuteInEditMode]
    public class UCC_VisualModifications : MonoBehaviour
    {
        public UCC_CarController carController;

        [Range(0f, 0.2f)]
        public float frontWheelsSpacer;
        [Range(0f, 0.2f)]
        public float rearWheelsSpacer;
        [Range(-45f, 45f)]
        public float frontWheelCamberAngle;
        [Range(-45f, 45f)]
        public float rearWheelCamberAngle;
        [Range(-45f, 45f)]
        public float frontWheelToeAngle;
        [Range(-45f, 45f)]
        public float rearWheelToeAngle;

        public Color frontTireSmokeColor;
        public Color rearTireSmokeColor;

        [HideInInspector]
        public float lastFrontWheelCamberAngle;
        [HideInInspector]
        public float lastRearWheelCamberAngle;
        [HideInInspector]
        public float lastFrontWheelToeAngle;
        [HideInInspector]
        public float lastRearWheelToeAngle;

        private void Update()
        {
            SetVisuals(carController.frontRightWheel, carController.frontLeftWheel, carController.rearRightWheel, carController.rearLeftWheel);
        }

        private void SetParticleColor(ParticleSystem particleSystem, Color color)
        {
            if (particleSystem != null)
            {
                ParticleSystem.MainModule mainModule = particleSystem.main;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(color);
            }
            else
            {
                Debug.LogWarning("ParticleSystem is null. Ensure it is assigned properly.");
            }
        }

        public void SetVisuals(WheelCollider rightFrontWheel, WheelCollider leftFrontWheel, WheelCollider rightRearWheel, WheelCollider leftRearWheel)
        {
            SetParticleColor(leftFrontWheel.GetComponentInChildren<ParticleSystem>(), frontTireSmokeColor);
            SetParticleColor(rightFrontWheel.GetComponentInChildren<ParticleSystem>(), frontTireSmokeColor);
            SetParticleColor(leftRearWheel.GetComponentInChildren<ParticleSystem>(), rearTireSmokeColor);
            SetParticleColor(rightRearWheel.GetComponentInChildren<ParticleSystem>(), rearTireSmokeColor);
            AdjustCamberToe(frontWheelToeAngle, frontWheelCamberAngle, rearWheelToeAngle, rearWheelCamberAngle);
            leftFrontWheel.center = Vector3.zero;
            rightFrontWheel.center = Vector3.zero;

            Vector3 leftFrontCenter = leftFrontWheel.center;
            leftFrontCenter.x -= frontWheelsSpacer;
            leftFrontWheel.center = leftFrontCenter;

            Vector3 rightFrontCenter = rightFrontWheel.center;
            rightFrontCenter.x += frontWheelsSpacer;
            rightFrontWheel.center = rightFrontCenter;

            leftRearWheel.center = Vector3.zero;
            rightRearWheel.center = Vector3.zero;

            Vector3 leftRearCenter = leftRearWheel.center;
            leftRearCenter.x -= rearWheelsSpacer;
            leftRearWheel.center = leftRearCenter;

            Vector3 rightRearCenter = rightRearWheel.center;
            rightRearCenter.x += rearWheelsSpacer;
            rightRearWheel.center = rightRearCenter;
        }

        public void AdjustCamberToe(float frontWheelToeAngle, float frontWheelCamberAngle, float rearWheelToeAngle, float rearWheelCamberAngle)
        {
            if (Mathf.Approximately(frontWheelToeAngle, lastFrontWheelToeAngle) &&
                Mathf.Approximately(frontWheelCamberAngle, lastFrontWheelCamberAngle) &&
                Mathf.Approximately(rearWheelToeAngle, lastRearWheelToeAngle) &&
                Mathf.Approximately(rearWheelCamberAngle, lastRearWheelCamberAngle))
            {
                return;
            }
            lastFrontWheelToeAngle = frontWheelToeAngle;
            lastFrontWheelCamberAngle = frontWheelCamberAngle;
            lastRearWheelToeAngle = rearWheelToeAngle;
            lastRearWheelCamberAngle = rearWheelCamberAngle;

             AdjustWheelCamberToe(carController.frontLeftWheel, -frontWheelToeAngle, -frontWheelCamberAngle);
             AdjustWheelCamberToe(carController.frontRightWheel, frontWheelToeAngle, frontWheelCamberAngle);
             AdjustWheelCamberToe(carController.rearLeftWheel, -rearWheelToeAngle, -rearWheelCamberAngle);
             AdjustWheelCamberToe(carController.rearRightWheel, rearWheelToeAngle, rearWheelCamberAngle);
        }

        private void AdjustWheelCamberToe(WheelCollider wheel, float toeAngle, float camberAngle)
        {
            for (int i = 0; i < wheel.transform.childCount; i++)
            {
                Transform childObject = wheel.transform.GetChild(i);
                if (childObject != null && childObject.childCount > 0)
                {
                    Transform wheelMesh = childObject.GetChild(0);

                    Vector3 currentRotation = wheelMesh.localEulerAngles;

                    currentRotation.x = 0; 
                    currentRotation.y = toeAngle; 
                    currentRotation.z = camberAngle; 

                    wheelMesh.localEulerAngles = currentRotation;
                }
            }
        }
    }
}
