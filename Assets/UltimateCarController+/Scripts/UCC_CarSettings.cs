using UnityEngine;
namespace KairaDigitalArts
{
    public enum DriveTrain { FWD, RWD, AWD }
    public enum TransmissionType { Automatic, Manual }

    [ExecuteInEditMode]
    public class UCC_CarSettings : MonoBehaviour
    {
        [SerializeField] public UCC_CarController carController;
        [Range(0.1f, 1f)]
        public float aerodynamicEfficiency = 0.85f;
        public AnimationCurve torqueCurve;
        [Range (180,1080)]
        public float maxSteeringWheelRotation = 360f;

        public float shiftDelayDuration = 0.5f;

        [Tooltip("Height of car in meters")]
        public float height;
        [Tooltip("Width of car in meters")]
        public float width;
        [Tooltip("The maximum RPM of the engine.")]
        public float maxRPM = 7000f;
        [Tooltip("The minimum RPM of the engine.")]
        public float minRPM = 1000f;
        [Tooltip("The maximum steering angle at low speeds.")]
        public float maxSteerAngle = 30f;
        [Tooltip("The minimum steering angle at high speeds.")]
        public float minSteerAngle = 10f;

        [Tooltip("Torque applied to rear wheel (applicable only for AWD)")]
        [Range(0.1f, 0.9f)]
        public float rearWheelTorqueDistribution;
        [Range(0f, 1f)]
        public float rearWheelBrakeDistribution;
        [Range(0.1f, 2.5f)]
        public float frontTireStifness;
        [Range(0.1f, 2.5f)]
        public float rearTireStifness;
        [Range(0.1f, 2f)]
        public float rearWheelRadius = 1;
        [Range(0.1f, 2f)]
        public float frontWheelRadius = 1;
        [Range(0.1f, 2f)]
        public float rearWheelColliderRadius;
        [Range(0.1f, 2f)]
        public float frontWheelColliderRadius;

        [Header("Suspension")]
        [Tooltip("The damping ratio for the suspension system.")]
        public float dampingRatio = 0.8f;
        [Tooltip("The natural frequency of the suspension system.")]
        public float naturalFrequency = 10f;
        [Tooltip("The wheelbase: distance between the front and rear axles.")]
        public float wheelBase = 2.5f;
        [Tooltip("The track width: distance between the two front wheels.")]
        public float trackWidth = 1.5f;
        public float rearSuspensionLength = 0.2f;
        public float frontSuspensionLength = 0.2f; 
        [Range(-1, 1)]
        public float forceShift = 0.03f;

        [Header("Torque and Brakes")]
        [Tooltip("The maximum torque applied to the drive wheels.")]
        public float maxTorque = 1500f;
        [Tooltip("The brake torque applied when braking.")]
        public float brakeTorque = 3000f;
        [Tooltip("The handbrake torque applied when handbrake is engaged.")]
        public float handbrakeTorque = 50000f;

        public bool useN2O;
        public float n2oPower;
        public float n2oMaxDuration;
        public float n2oCooldownMultiplier = 1f;

        public float frontForwardExtremumSlip = 0.6f;
        public float frontForwardExtremumValue = 1f;
        public float frontForwardAsymptoteSlip = 0.5f;
        public float frontForwardAsymptoteValue = 0.7f;
                     
        public float frontSidewaysExtremumSlip = 2.5f;
        public float frontSidewaysExtremumValue = 2.5f;
        public float frontSidewaysAsymptoteSlip = 2f;
        public float frontSidewaysAsymptoteValue = 0.5f;

        public float rearForwardExtremumSlip = 0.6f;
        public float rearForwardExtremumValue = 1f;
        public float rearForwardAsymptoteSlip = 0.5f;
        public float rearForwardAsymptoteValue = 0.7f;
                     
        public float rearSidewaysExtremumSlip = 2.5f;
        public float rearSidewaysExtremumValue = 2.5f;
        public float rearSidewaysAsymptoteSlip = 2f;
        public float rearSidewaysAsymptoteValue = 0.5f;


        [Tooltip("The type of drivetrain: FWD (Front Wheel Drive), RWD (Rear Wheel Drive), AWD (All Wheel Drive).")]
        [ SerializeField] public DriveTrain driveTrain = DriveTrain.RWD;
        [Tooltip("The type of transmission: Automatic or Manual.")]
        [SerializeField] public TransmissionType transmissionType;
        [Header("Gear Settings")]
        public bool useClutch;
        public int maxGear = 6;
        public float[] gearRatios;
        [Range(1f, 5f)]
        public float differantial;

        private void Update()
        {
            UpdateCarSetup(carController.frontRightWheel, carController.frontLeftWheel, carController.rearRightWheel, carController.rearLeftWheel);
            carController.UpdateWheelPose(carController.frontRightWheel, carController.frontLeftWheel);
            carController.UpdateWheelPose(carController.rearRightWheel, carController.rearLeftWheel);
        }
        public void UpdateCarSetup(WheelCollider rightFrontWheel, WheelCollider leftFrontWheel, WheelCollider rightRearWheel, WheelCollider leftRearWheel)
        {

            WheelFrictionCurve leftFrontWheelForwardFrictionCurve = leftFrontWheel.forwardFriction;
            WheelFrictionCurve rightFrontWheelForwardFrictionCurve = rightFrontWheel.forwardFriction;
            WheelFrictionCurve rightRearWheelForwardFrictionCurve = rightRearWheel.forwardFriction;
            WheelFrictionCurve leftRearWheelForwardFrictionCurve = leftRearWheel.forwardFriction;

            WheelFrictionCurve leftFrontWheelSidewayFrictionCurve = leftFrontWheel.sidewaysFriction;
            WheelFrictionCurve rightFrontWheelSideWayFrictionCurve = rightFrontWheel.sidewaysFriction;
            WheelFrictionCurve rightRearWheelSideWayFrictionCurve = rightRearWheel.sidewaysFriction;
            WheelFrictionCurve leftRearWheelSidewayFrictionCurve = leftRearWheel.sidewaysFriction;

            leftRearWheel.suspensionDistance = rearSuspensionLength;
            rightRearWheel.suspensionDistance = rearSuspensionLength;

            rightFrontWheel.suspensionDistance = frontSuspensionLength;
            leftFrontWheel.suspensionDistance = frontSuspensionLength;
            // Adjusting rear wheels
            SetWheelRadius(leftRearWheel, leftRearWheel.GetComponentInChildren<MeshRenderer>().transform, rearWheelRadius);
            SetWheelRadius(rightRearWheel, rightRearWheel.GetComponentInChildren<MeshRenderer>().transform, rearWheelRadius);

            // Adjusting front wheels
            SetWheelRadius(leftFrontWheel, leftFrontWheel.GetComponentInChildren<MeshRenderer>().transform, frontWheelRadius);
            SetWheelRadius(rightFrontWheel, rightFrontWheel.GetComponentInChildren<MeshRenderer>().transform, frontWheelRadius);

            leftRearWheel.radius = rearWheelColliderRadius;
            rightRearWheel.radius = rearWheelColliderRadius;

            rightFrontWheel.radius = frontWheelColliderRadius;
            leftFrontWheel.radius = frontWheelColliderRadius;

            leftFrontWheelForwardFrictionCurve.stiffness = frontTireStifness;
            rightFrontWheelForwardFrictionCurve.stiffness = frontTireStifness;
            leftFrontWheelSidewayFrictionCurve.stiffness = frontTireStifness;
            rightFrontWheelSideWayFrictionCurve.stiffness = frontTireStifness;

            rightRearWheelSideWayFrictionCurve.stiffness = rearTireStifness;
            leftRearWheelSidewayFrictionCurve.stiffness = rearTireStifness;
            rightRearWheelForwardFrictionCurve.stiffness = rearTireStifness;
            leftRearWheelForwardFrictionCurve.stiffness = rearTireStifness;

            leftFrontWheel.forwardFriction = leftFrontWheelForwardFrictionCurve;
            rightFrontWheel.forwardFriction = rightFrontWheelForwardFrictionCurve;
            leftRearWheel.forwardFriction = leftRearWheelForwardFrictionCurve;
            rightRearWheel.forwardFriction = rightRearWheelForwardFrictionCurve;

            leftFrontWheel.sidewaysFriction = leftFrontWheelSidewayFrictionCurve;
            rightFrontWheel.sidewaysFriction = rightFrontWheelSideWayFrictionCurve;
            leftRearWheel.sidewaysFriction = leftRearWheelSidewayFrictionCurve;
            rightRearWheel.sidewaysFriction = rightRearWheelSideWayFrictionCurve;
            
            WheelFrictionCurve frontForwardFriction = new WheelFrictionCurve
            {
                extremumSlip = frontForwardExtremumSlip,
                extremumValue = frontForwardExtremumValue,
                asymptoteSlip = frontForwardAsymptoteSlip,
                asymptoteValue = frontForwardAsymptoteValue,
                stiffness = frontTireStifness
            };

            WheelFrictionCurve frontSidewaysFriction = new WheelFrictionCurve
            {
                extremumSlip = frontSidewaysExtremumSlip,
                extremumValue = frontSidewaysExtremumValue,
                asymptoteSlip = frontSidewaysAsymptoteSlip,
                asymptoteValue = frontSidewaysAsymptoteValue,
                stiffness = frontTireStifness
            };

            WheelFrictionCurve rearForwardFriction = new WheelFrictionCurve
            {
                extremumSlip = rearForwardExtremumSlip,
                extremumValue = rearForwardExtremumValue,
                asymptoteSlip = rearForwardAsymptoteSlip,
                asymptoteValue = rearForwardAsymptoteValue,
                stiffness = rearTireStifness
            };

            WheelFrictionCurve rearSidewaysFriction = new WheelFrictionCurve
            {
                extremumSlip = rearSidewaysExtremumSlip,
                extremumValue = rearSidewaysExtremumValue,
                asymptoteSlip = rearSidewaysAsymptoteSlip,
                asymptoteValue = rearSidewaysAsymptoteValue,
                stiffness = rearTireStifness
            };
            carController.frontLeftWheel.forwardFriction = frontForwardFriction;
            carController.frontRightWheel.forwardFriction = frontForwardFriction;
            carController.frontLeftWheel.sidewaysFriction = frontSidewaysFriction;
            carController.frontRightWheel.sidewaysFriction = frontSidewaysFriction;

            carController.rearLeftWheel.forwardFriction = rearForwardFriction;
            carController.rearRightWheel.forwardFriction = rearForwardFriction;
            carController.rearLeftWheel.sidewaysFriction = rearSidewaysFriction;
            carController.rearRightWheel.sidewaysFriction = rearSidewaysFriction;
        }// Update both the WheelCollider radius and visual wheel scale
        private void SetWheelRadius(WheelCollider wheelCollider, Transform wheelVisual, float radius)
        {
            wheelVisual.localScale = new Vector3(radius, radius, radius);
        }
    }
}
