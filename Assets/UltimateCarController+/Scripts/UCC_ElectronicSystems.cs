using KairaDigitalArts;
using System.Collections;
using UnityEngine;

namespace KairaDigitalArts
{
    public class UCC_ElectronicSystems : MonoBehaviour
    {
        [SerializeField] public UCC_CarController carController;
        [SerializeField] public UCC_CarSettings carSettings;

        [Header("Electronic Systems")]
        public bool enableESC = true;
        public bool enableTC = true;
        public bool ABS = false;
        [Range(1f, 90f)]
        public float tcPower;
        public float escBrakeForce = 5000f;
        [Tooltip("Activate ABS system if enabled.")]
        public float absResponseSpeed = 2.0f;
        public float absSlipThreshold = 0.1f;
        public IEnumerator ApplyTractionControl()
        {
            if (carController.isSlippingForward)
            {
                float tempLeftFront = carController.frontLeftWheel.motorTorque;
                float tempRightFront = carController.frontRightWheel.motorTorque;
                float tempLeftRear = carController.rearLeftWheel.motorTorque;
                float tempRightRear = carController.rearRightWheel.motorTorque;

                carController.frontLeftWheel.motorTorque = 0;
                carController.frontRightWheel.motorTorque = 0;
                carController.rearLeftWheel.motorTorque = 0;
                carController.rearRightWheel.motorTorque = 0;

                yield return new WaitForSeconds(0.1f);

                carController.frontLeftWheel.motorTorque = tempLeftFront;
                carController.frontRightWheel.motorTorque = tempRightFront;
                carController.rearLeftWheel.motorTorque = tempLeftRear;
                carController.rearRightWheel.motorTorque = tempRightRear;
            }
        }
        public IEnumerator ApplyABS()
        {
            float tempLeftFront = carController.frontLeftWheel.brakeTorque;
            float tempRightFront = carController.frontRightWheel.brakeTorque;
            float tempLeftRear = carController.rearLeftWheel.brakeTorque;
            float tempRightRear = carController.rearRightWheel.brakeTorque;

            carController.frontLeftWheel.brakeTorque = 0;
            carController.frontRightWheel.brakeTorque = 0;
            carController.rearLeftWheel.brakeTorque = 0;
            carController.rearRightWheel.brakeTorque = 0;

            yield return new WaitForSeconds(0.1f);

            carController.frontLeftWheel.brakeTorque = tempLeftFront;
            carController.frontRightWheel.brakeTorque = tempRightFront;
            carController.rearLeftWheel.brakeTorque = tempLeftRear;
            carController.rearRightWheel.brakeTorque = tempRightRear;
        }
        public IEnumerator ApplyESC()
        {
            if (carController.driftOffset > 0.15f)
            {
                yield return new WaitForSeconds(0.1f);
                carController.frontRightWheel.brakeTorque = carSettings.brakeTorque;
                carController.rearRightWheel.brakeTorque = carSettings.brakeTorque;
                carController.frontLeftWheel.brakeTorque = carSettings.brakeTorque;
                carController.rearLeftWheel.brakeTorque = carSettings.brakeTorque;
            }

            yield return new WaitForSeconds(0.1f);

            carController.frontLeftWheel.brakeTorque = 0f;
            carController.frontRightWheel.brakeTorque = 0f;
            carController.rearLeftWheel.brakeTorque = 0f;
            carController.rearRightWheel.brakeTorque = 0f;
        }
    }
}
