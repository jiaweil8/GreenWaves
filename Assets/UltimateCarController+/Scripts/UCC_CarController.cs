using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KairaDigitalArts
{
    public class UCC_CarController : MonoBehaviour
    {
        [Header("Scripts")]
        public UCC_ElectronicSystems electronicSystems;
        public UCC_VisualModifications visualModifications;
        public UCC_CarSettings carSettings;
        public UCC_CarLights carLights;
        public UCC_Gauge gauge;
        public UCC_InputSystem inputSystem;
        public UCC_EngineSound engineSound;
        public UCC_ExtraCarOptions carExtras;

        [Header("General Settings")]
        [Header("Wheels")]
        [Tooltip("The wheel collider for the front left wheel.")]
        public WheelCollider frontLeftWheel;
        [Tooltip("The wheel collider for the front right wheel.")]
        public WheelCollider frontRightWheel;
        [Tooltip("The wheel collider for the rear left wheel.")]
        public WheelCollider rearLeftWheel;
        [Tooltip("The wheel collider for the rear right wheel.")]
        public WheelCollider rearRightWheel;
        [Tooltip("The visual wheel mesh prefab.")]
        public GameObject steeringWheel;
        public GameObject cockpitGagueNeedle;
        public Rigidbody carRigidbody;

        [Header("Display")]
        public int speedKmh;
        public int speedMph;
        public float currentTorque;
        public float currentWheelTorque;
        public int currentGear;
        public float engineRPM;
        [Tooltip("The car's engine horsepower.")]
        public float horsepower;
        public bool isEngineOn;
        public bool isLightsOn;
        [Tooltip("The maximum speed of the car in km/h.")]
        public float maxSpeed;

        [HideInInspector]
        public int currentSpeed;
        [HideInInspector]
        public bool isSlippingForward;
        [HideInInspector]
        public bool isSlippingSideways;
        [HideInInspector]
        [SerializeField] float n2oDuration;
        [HideInInspector]
        public float driftOffset;
        [HideInInspector]
        public bool isReverse;
        [HideInInspector]
        private bool isRevLimiterActive = false;
        [HideInInspector]
        private float baseTorque;
        [HideInInspector]
        public float optimalSpeedForCurrentGear;
        [HideInInspector]
        private Quaternion currentNeedleRotation;
        [HideInInspector]
        private float brakeLightsIntensity;
        [HideInInspector]
        private Quaternion steeringWheelInitialRotation;
        private void Start()
        {
            if (carRigidbody == null) carRigidbody = GetComponent<Rigidbody>();
            if (carSettings.gearRatios == null || carSettings.gearRatios.Length != carSettings.maxGear)
            {
                carSettings.gearRatios = new float[carSettings.maxGear + 1];
                carSettings.gearRatios[0] = 0f;
                float baseRatio = 3.5f;
                for (int i = 1; i <= carSettings.maxGear; i++)
                {
                    carSettings.gearRatios[i] = baseRatio;
                    baseRatio *= 0.72f;
                }
            }
            else
            {
                float[] userRatios = new float[carSettings.gearRatios.Length +1];
                userRatios[0] = 0f;
                for (int i = 1; i < userRatios.Length; i++)
                {
                    userRatios[i] = carSettings.gearRatios[i - 1];
                }
                carSettings.gearRatios = userRatios;
            }
            maxSpeed = CalculateTopSpeed();
            if(carLights != null)
            {
                brakeLightsIntensity = carLights.brakeLightIntensity;
            }
            steeringWheelInitialRotation = steeringWheel.transform.localRotation;
        }
        private void Update()
        {
            speedKmh = currentSpeed;
            inputSystem.GetAxis();
            GaugeNeedle();
            HandleGearShifting();
            speedMph = (int)MathF.Round(currentSpeed / 1.60934f);
            engineRPM = CalculateEngineRPM();
            if (electronicSystems != null)
            {
                if (electronicSystems.enableESC) electronicSystems.ApplyESC();
                if (electronicSystems.enableTC) electronicSystems.ApplyTractionControl();
            }
            EngineControl();
            ControlLights();
            UseN2O();
        }
        private void FixedUpdate()
        {
            currentSpeed = (int)MathF.Round(carRigidbody.velocity.magnitude * 3.6f);
            driftOffset = CalculateDriftOffset();
            float dynamicSteerAngle = CalculateSteerAngle();
            ApplyAckermannSteering(dynamicSteerAngle);
            ApplyDrive();
            ApplyBrakes();
            ApplyHandbrake();
            UpdateSuspension();
            EngineBraking();
            isSlippingForward = CalculateForwardSlip(rearRightWheel, rearLeftWheel, frontRightWheel, frontLeftWheel);
            isSlippingSideways = CalculateSideWaysSlip(rearRightWheel, rearLeftWheel, frontRightWheel, frontLeftWheel);
        }

        private void LateUpdate()
        {
            InitiateSmokeAndSkidmarks();
        }

        void GaugeNeedle()
        {
            if (gauge != null)
            {
                Quaternion targetRotation = Quaternion.Euler(0, 0, Mathf.Lerp(gauge.minNeedleRotation, gauge.maxNeedleRotation, engineRPM / (carSettings.maxRPM * 1.1f)));
                currentNeedleRotation = Quaternion.Slerp(currentNeedleRotation, targetRotation, Time.deltaTime * 2f);
                gauge.rpmNeedle.rotation = currentNeedleRotation;
                if (cockpitGagueNeedle != null)
                {
                    cockpitGagueNeedle.transform.localRotation = currentNeedleRotation;
                }
                gauge.n20Indicator.GetComponent<Image>().fillAmount = 1 - (n2oDuration / carSettings.n2oMaxDuration);
            }
        }
        void InitiateSmokeAndSkidmarks()
        {
           InitiateSkidmarks(rearRightWheel);
           InitiateSkidmarks(rearLeftWheel);
           InitiateSkidmarks(frontLeftWheel);
           InitiateSkidmarks(frontRightWheel);
           InitiateSmoke(rearRightWheel);
           InitiateSmoke(rearLeftWheel);
           InitiateSmoke(frontLeftWheel);
           InitiateSmoke(frontRightWheel);
        }
        public void EngineBraking()
        {
            float brakeForce = 500f;

            if (currentSpeed > optimalSpeedForCurrentGear && currentSpeed>0) 
            {
                float speedDifference = currentSpeed - optimalSpeedForCurrentGear;

                float appliedForce = brakeForce * speedDifference;
                if (currentGear > 0)
                {
                    carRigidbody.AddForce(-transform.forward * appliedForce);
                }
                else
                {
                    carRigidbody.AddForce(transform.forward * appliedForce);
                }
            }
        }
        public void CalculateSpeedPerGear()
        {
            if (currentGear > 0)
            {
                optimalSpeedForCurrentGear = ((maxSpeed) / (carSettings.gearRatios[currentGear] / carSettings.differantial)) / (carSettings.gearRatios.Length - 2);

            }
            else
            {
                optimalSpeedForCurrentGear = ((maxSpeed) / (3.5f / carSettings.differantial)) / (carSettings.gearRatios.Length - 2);
            }
        }
        public void EngineControl()
        {
            if (inputSystem.controller.Driving.EnginePower.triggered && currentSpeed < 5)
            {
                StartCoroutine(ToggleEngineWithDelay());
            }
        }

        private IEnumerator ToggleEngineWithDelay()
        {
            float delay = 0;
            if (!isEngineOn)
            {
                engineSound.engineStartAudioSource.Play();
                delay = 1f;
            }
            yield return new WaitForSeconds(delay);
            if (isEngineOn)
            {
                engineSound.engineShutdownAudioSource.Play();
                engineSound.StopAllSounds();
            }
            isEngineOn = !isEngineOn;
        }
        private void ApplyDrive()
        {
            currentTorque = 0;
            currentWheelTorque = 0;

            if (!isEngineOn) return;

            float normalizedRPM = Mathf.Clamp01(Mathf.InverseLerp(carSettings.minRPM, carSettings.maxRPM, engineRPM));
            currentTorque = carSettings.torqueCurve.Evaluate(normalizedRPM) * carSettings.maxTorque * inputSystem.gasInput;
            RevLimiter();

            if (inputSystem.gasInput > 0 || inputSystem.brakeInput > 0)
            {
                if (currentGear == -1)
                {
                    if (carSettings.transmissionType == TransmissionType.Automatic)
                    {
                        currentTorque = inputSystem.brakeInput * carSettings.torqueCurve.Evaluate(normalizedRPM) * carSettings.maxTorque;
                    }
                    currentWheelTorque = -currentTorque * 2f * carSettings.differantial;
                    isReverse = true;
                }
                else if (inputSystem.gasInput > 0)
                {
                    currentWheelTorque = currentTorque * carSettings.gearRatios[currentGear] * carSettings.differantial;
                    isReverse = false;
                }
            }
            switch (carSettings.driveTrain)
            {
                case DriveTrain.FWD:
                    ApplyTorqueToWheels(currentWheelTorque, frontLeftWheel, frontRightWheel);
                    break;
                case DriveTrain.RWD:
                    ApplyTorqueToWheels(currentWheelTorque, rearLeftWheel, rearRightWheel);
                    break;
                case DriveTrain.AWD:
                    ApplyTorqueToWheels((currentWheelTorque / 2) * (1 - carSettings.rearWheelTorqueDistribution), frontLeftWheel, frontRightWheel);
                    ApplyTorqueToWheels((currentWheelTorque / 2) * carSettings.rearWheelTorqueDistribution, rearLeftWheel, rearRightWheel);
                    break;
            }
        }
        private void ApplyBrakes()
        {
            float appliedBrakeTorque = inputSystem.brakeInput * carSettings.brakeTorque;

            if (inputSystem.brakeInput > 0)
            {
                if (currentGear == -1 && carSettings.transmissionType == TransmissionType.Automatic)
                {
                    frontLeftWheel.brakeTorque = 0;
                    frontRightWheel.brakeTorque = 0;
                    rearLeftWheel.brakeTorque = 0;
                    rearRightWheel.brakeTorque = 0;
                }
                else
                {
                    frontLeftWheel.brakeTorque = (1 - carSettings.rearWheelBrakeDistribution) * appliedBrakeTorque;
                    frontRightWheel.brakeTorque = (1 - carSettings.rearWheelBrakeDistribution) * appliedBrakeTorque;
                    rearLeftWheel.brakeTorque = carSettings.rearWheelBrakeDistribution * appliedBrakeTorque;
                    rearRightWheel.brakeTorque = carSettings.rearWheelBrakeDistribution * appliedBrakeTorque;
                    if (electronicSystems != null && electronicSystems.ABS)
                    {
                        electronicSystems.ApplyABS();
                    }
                }
            }
            else
            {
                frontLeftWheel.brakeTorque = 0;
                frontRightWheel.brakeTorque = 0;
                rearLeftWheel.brakeTorque = 0;
                rearRightWheel.brakeTorque = 0;
            }
        }
        private bool isShifting = false;

        private void HandleGearShifting()
        {
            if (carSettings.transmissionType == TransmissionType.Automatic)
            {
                if (currentSpeed >= optimalSpeedForCurrentGear * 0.9f && currentGear < carSettings.maxGear && inputSystem.gasInput > 0)
                {
                    currentGear++;
                }

                if (currentSpeed < optimalSpeedForCurrentGear * 0.5f && currentGear > 1)
                {
                    currentGear--;
                }

                if (inputSystem.brakeInput > 0 && currentSpeed < 0.1f && currentGear > -1)
                {
                    currentGear = -1;
                }

                if ((currentGear == -1 || currentGear == 0 )&& inputSystem.gasInput > 0)
                {
                    currentGear = 1;
                }
            }
            else if (carSettings.transmissionType == TransmissionType.Manual)
            {
                if (carSettings.useClutch)
                {
                    bool isClutchPressed = inputSystem.clutchInput > 0;

                    if (isClutchPressed)
                    {
                        if (inputSystem.controller.Driving.GearUp.triggered && currentGear < carSettings.maxGear && !isShifting)
                        {
                            StartCoroutine(GearShiftCoroutine(1));
                        }
                        if (inputSystem.controller.Driving.GearDown.triggered && currentGear > -1 && !isShifting)
                        {
                            StartCoroutine(GearShiftCoroutine(-1));
                        }
                    }
                }
                else
                {
                    if (inputSystem.controller.Driving.GearUp.triggered && currentGear < carSettings.maxGear && !isShifting)
                    {
                        StartCoroutine(GearShiftCoroutine(1));
                    }
                    if (inputSystem.controller.Driving.GearDown.triggered && currentGear > -1 && !isShifting)
                    {
                        StartCoroutine(GearShiftCoroutine(-1));
                    }
                }
            }
        }

        private IEnumerator GearShiftCoroutine(int direction)
        {
            isShifting = true;
            yield return new WaitForSeconds(carSettings.shiftDelayDuration);

            if (direction == 1)
            {
                currentGear++;
            }
            else if (direction == -1)
            {
                currentGear--;
                if (currentGear > 0 && currentSpeed < 5)
                {
                    currentGear = 1;
                }
            }
            isShifting = false;
        }
        private float currentSteeringWheelRotation = 0f;
        private float returnSpeed = 500f;

        private void RotateSteeringWheel()
        {
            float maxSteer = inputSystem.steerInput * carSettings.maxSteeringWheelRotation;
            if (Mathf.Abs(inputSystem.steerInput) > 0.01f)
            {
                currentSteeringWheelRotation += maxSteer * Time.deltaTime;

                if (inputSystem.steerInput < 0)
                {
                    currentSteeringWheelRotation = Mathf.Max(currentSteeringWheelRotation, maxSteer);
                }
                else
                {
                    currentSteeringWheelRotation = Mathf.Min(currentSteeringWheelRotation, maxSteer);
                }

                currentSteeringWheelRotation = Mathf.Clamp(currentSteeringWheelRotation, -carSettings.maxSteeringWheelRotation, carSettings.maxSteeringWheelRotation);
            }
            else if (Mathf.Abs(inputSystem.steerInput) <= 0.01f && Mathf.Abs(currentSteeringWheelRotation) > 0.01f)
            {
                float step = returnSpeed * Time.deltaTime;
                currentSteeringWheelRotation = Mathf.MoveTowards(currentSteeringWheelRotation, 0, step);
            }
            steeringWheel.transform.localRotation = steeringWheelInitialRotation * Quaternion.Euler(0, 0, -currentSteeringWheelRotation);
        }
        private void RevLimiter()
        {
            if (engineRPM >= carSettings.maxRPM * 0.99f)
            {
                isRevLimiterActive = true;
                if (engineSound != null) 
                {
                    if(inputSystem.gasInput > 0)
                    {
                        engineSound.PlayMaxRpm();
                    }
                    else
                    {
                        if (engineSound != null)
                        {
                            engineSound.StopMaxRpm();
                        }
                    }
                }
            }
            else if (engineRPM < carSettings.maxRPM * 0.90f)
            {
                isRevLimiterActive = false;
                if (engineSound != null)
                {
                    engineSound.StopMaxRpm();
                }
            }
            if (isRevLimiterActive)
            {
                currentTorque = 0;
            }
        }
        private float CalculatePower()
        {
            horsepower = (carSettings.maxTorque * carSettings.maxRPM) / (5252 * 1.35579f);
            float powerKW = horsepower * 0.7457f;
            return powerKW;
        }
        private float CalculateTopSpeed()
        {
            float enginePowerKw = CalculatePower();
            float enginePowerW = enginePowerKw * 1000f;

            float ro = 1.225f; // Air density (kg/m³)
            float Cd = 0.4f / carSettings.aerodynamicEfficiency; // Drag coefficient
            float A = carSettings.height * carSettings.width; // Frontal area (m²)

            double v = Math.Pow(2 * enginePowerW / (ro * Cd * A), 1.0 / 3.0);
            v = v - (v * 0.01f);
            float topSpeed = (float)v;
            topSpeed = topSpeed * 3.6f;

            return topSpeed;
        }
        private void ApplyTorqueToWheels(float torque, WheelCollider leftWheel, WheelCollider rightWheel)
        {
            leftWheel.motorTorque = torque;
            rightWheel.motorTorque = torque;
        }

        private bool accelarateHighRPM;
        private float CalculateEngineRPM()
        {
            CalculateSpeedPerGear();
            float rpm = 0;
            if (isEngineOn)
            {
                float randomFactor = UnityEngine.Random.Range(-10, +10);
                float wheelRPM = 0;
                if (currentGear > 0)
                {
                    float tireCircumference = Mathf.PI * 0.65f;
                    float tireRevolutionsPerSecond = (currentSpeed / tireCircumference) + 0.01f;
                    tireRevolutionsPerSecond *= 60f;

                    float ratio = currentSpeed / optimalSpeedForCurrentGear;
                    float slipRatio;
                    bool isClutchPressed = inputSystem.clutchInput > 0;

                    if (isClutchPressed && (inputSystem.gasInput > 0 || inputSystem.brakeInput > 0))
                    {
                        slipRatio = 10;
                    }
                    else
                    {

                        slipRatio = ((rearRightWheel.rpm + rearLeftWheel.rpm) / 2) / tireRevolutionsPerSecond;
                        slipRatio = Mathf.Clamp(slipRatio, 1, 2.0f);

                    }
                    rpm = (carSettings.maxRPM - carSettings.minRPM) * ratio * slipRatio + carSettings.minRPM;
                    if(accelarateHighRPM && currentGear == 1)
                    {
                        float normalRPM = rpm;
                        rpm = engineRPM;

                        rpm = Mathf.Lerp(rpm,normalRPM,Time.deltaTime * 1.5f);
                        if(rpm == normalRPM)
                        {
                            accelarateHighRPM = false;
                        }
                    }
                }
                else if (currentGear < 0) 
                {
                    wheelRPM = Mathf.Abs(((rearRightWheel.rpm + rearLeftWheel.rpm)) * 3f * carSettings.differantial) + randomFactor;
                    rpm = Mathf.Lerp(engineRPM, Mathf.Max(carSettings.minRPM - 100, wheelRPM), Time.deltaTime * 3f);
                }
                else
                {
                    float gasInput = inputSystem.gasInput;
                    rpm = Mathf.Lerp(engineRPM, carSettings.minRPM + gasInput * (carSettings.maxRPM - carSettings.minRPM), Time.deltaTime * 2f);
                    if(rpm > carSettings.minRPM + randomFactor)
                    {
                        accelarateHighRPM = true;
                    }
                    else
                    {
                        accelarateHighRPM = false;
                    }
                }

                if (rpm < carSettings.minRPM)
                {
                    rpm = carSettings.minRPM + randomFactor;
                }
                if (rpm > carSettings.maxRPM)
                {
                    rpm = carSettings.maxRPM + randomFactor;
                }
            }
            return rpm;
        }
        float CalculateDriftOffset()
        {
            float driftOffset = MathF.Abs(Vector3.Dot(carRigidbody.velocity.normalized, carRigidbody.transform.right.normalized));
            return driftOffset;
        }
        private float CalculateSteerAngle()
        {
            float steerFactor = Mathf.Clamp(1 - (currentSpeed / maxSpeed), 0.1f, 1f);
            float steerSensitivity = Mathf.Lerp(carSettings.minSteerAngle, carSettings.maxSteerAngle, steerFactor);

            return steerSensitivity;
        }

        private void ApplyAckermannSteering(float steerAngle)
        {
            float angle = steerAngle * inputSystem.steerInput;
            if (inputSystem.steerInput != 0)
            {
                float turningRadius = carSettings.wheelBase / Mathf.Tan(angle * Mathf.Deg2Rad);
                float innerSteerAngle = Mathf.Atan(carSettings.wheelBase / (turningRadius - carSettings.trackWidth / 2)) * Mathf.Rad2Deg;
                float outerSteerAngle = Mathf.Atan(carSettings.wheelBase / (turningRadius + carSettings.trackWidth / 2)) * Mathf.Rad2Deg;

                if (inputSystem.steerInput > 0)
                {
                    frontLeftWheel.steerAngle = innerSteerAngle;
                    frontRightWheel.steerAngle = outerSteerAngle;
                }
                else
                {
                    frontLeftWheel.steerAngle = outerSteerAngle;
                    frontRightWheel.steerAngle = innerSteerAngle;
                }
            }
            else
            {
                frontLeftWheel.steerAngle = 0;
                frontRightWheel.steerAngle = 0;
            }
            RotateSteeringWheel();
        }
        private void ApplyHandbrake()
        {
            if (inputSystem.handbrakeInput > 0)
            {
                rearLeftWheel.brakeTorque = carSettings.handbrakeTorque;
                rearRightWheel.brakeTorque = carSettings.handbrakeTorque;
            }
            else
            {
                rearLeftWheel.brakeTorque = 0;
                rearRightWheel.brakeTorque = 0;
            }
        }

        public bool usingN2o;
        public void UseN2O()
        {
            if (!carSettings.useN2O) return;

            if (baseTorque == 0) baseTorque = carSettings.maxTorque;

            if (inputSystem.n2oInput > 0 && n2oDuration < carSettings.n2oMaxDuration)
            {
                usingN2o = true;
                engineSound.usingN2O = true;
                carSettings.maxTorque = baseTorque * carSettings.n2oPower;
                n2oDuration += Time.deltaTime;
            }
            else
            {
                usingN2o = false;
                engineSound.usingN2O = false;
                carSettings.maxTorque = baseTorque;
                if (n2oDuration < 0)
                {
                    n2oDuration = 0;
                }
                else
                {
                    n2oDuration -= Time.deltaTime * carSettings.n2oCooldownMultiplier;
                }
            }
        }

        private void UpdateSuspension()
        {
            UpdateSuspensionForWheel(frontLeftWheel, carSettings.frontSuspensionLength);
            UpdateSuspensionForWheel(frontRightWheel, carSettings.frontSuspensionLength);
            UpdateSuspensionForWheel(rearLeftWheel, carSettings.rearSuspensionLength);
            UpdateSuspensionForWheel(rearRightWheel, carSettings.rearSuspensionLength);
        }
        private void UpdateSuspensionForWheel(WheelCollider wheel, float supensionLength)
        {
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = Mathf.Pow(Mathf.Sqrt(wheel.sprungMass) * carSettings.naturalFrequency, 2);
            spring.damper = 2 * carSettings.dampingRatio * Mathf.Sqrt(spring.spring * wheel.sprungMass);
            wheel.suspensionSpring = spring;
            wheel.suspensionDistance = supensionLength;
            Vector3 wheelRelativeBody = transform.InverseTransformPoint(wheel.transform.position);
            float distance = GetComponent<Rigidbody>().centerOfMass.y - wheelRelativeBody.y + wheel.radius;
            wheel.forceAppPointDistance = distance - carSettings.forceShift;
        }

        private bool CalculateForwardSlip(WheelCollider rearRight, WheelCollider rearLeft, WheelCollider frontRight, WheelCollider frontLeft)
        {
            bool frontRightSlipping = CalculateForwardSlipping(frontRight);
            bool frontLeftSlipping = CalculateForwardSlipping(frontLeft);
            bool rearLeftSlipping = CalculateForwardSlipping(rearLeft);
            bool rearRightSlipping = CalculateForwardSlipping(rearRight);

            return (frontRightSlipping && frontLeftSlipping) || (rearLeftSlipping && rearRightSlipping);
        }

        private bool CalculateForwardSlipping(WheelCollider wheelCollider)
        {
            WheelHit wheelHit;
            if (wheelCollider.GetGroundHit(out wheelHit))
            {
                float slip = wheelHit.forwardSlip;
                if (Mathf.Abs(slip) - wheelCollider.forwardFriction.extremumSlip > 0.15f)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CalculateSideWaysSlip(WheelCollider rearRight, WheelCollider rearLeft, WheelCollider frontRight, WheelCollider frontLeft)
        {
            bool frontRightSlipping = CalculateSideWaysSlipping(frontRight);
            bool frontLeftSlipping = CalculateSideWaysSlipping(frontLeft);
            bool rearLeftSlipping = CalculateSideWaysSlipping(rearLeft);
            bool rearRightSlipping = CalculateSideWaysSlipping(rearRight);

            return (frontRightSlipping && frontLeftSlipping) || (rearLeftSlipping && rearRightSlipping);
        }

        private bool CalculateSideWaysSlipping(WheelCollider wheelCollider)
        {
            WheelHit wheelHit;
            if (wheelCollider.GetGroundHit(out wheelHit))
            {
                float slip = wheelHit.sidewaysSlip;
                if (Mathf.Abs(slip) > wheelCollider.sidewaysFriction.extremumSlip)
                {
                    return true;
                }
            }
            return false;
        }
        public void UpdateWheelPose(WheelCollider rightWheel, WheelCollider leftWheel)
        {
            if (rightWheel.transform.childCount > 0)
            {
                Vector3 rightPosition;
                Quaternion rightRotation;
                rightWheel.GetWorldPose(out rightPosition, out rightRotation);

                for (int i = 0; i < rightWheel.transform.childCount; i++) 
                {
                    Transform shapeTransform = rightWheel.transform.GetChild(i);
                    shapeTransform.position = rightPosition;
                    rightWheel.transform.localRotation = Quaternion.Euler(0, rightWheel.steerAngle, 0);
                    if (shapeTransform.gameObject.GetComponentInChildren<Renderer>() != null)
                    {
                        shapeTransform.gameObject.GetComponentInChildren<Renderer>().gameObject.transform.Rotate(rightWheel.rpm * 6.6f * Time.deltaTime, 0, 0, Space.Self);
                    }
                    else
                    {
                        shapeTransform.rotation = rightRotation;
                    }
                }
            }
            if (leftWheel.transform.childCount > 0)
            {
                Vector3 leftPosition;
                Quaternion leftRotation;
                leftWheel.GetWorldPose(out leftPosition, out leftRotation);

                for (int i = 0; i < leftWheel.transform.childCount; i++)
                {
                    Transform shapeTransform = leftWheel.transform.GetChild(i);
                    shapeTransform.position = leftPosition;
                    leftWheel.transform.localRotation = Quaternion.Euler(0, leftWheel.steerAngle, 0);
                    if (shapeTransform.gameObject.GetComponentInChildren<Renderer>() != null)
                    {
                        shapeTransform.gameObject.GetComponentInChildren<Renderer>().gameObject.transform.Rotate(leftWheel.rpm * 6.6f * Time.deltaTime, 0, 0, Space.Self);
                    }
                    else
                    {
                        shapeTransform.rotation = leftRotation;
                    }
                }
            }
        }
        public void InitiateSkidmarks(WheelCollider wheel)
        {
            Vector3 wheelPos;
            Quaternion wheelRot;
            wheel.GetWorldPose(out wheelPos, out wheelRot);

            TrailRenderer trail = wheel.GetComponentInChildren<TrailRenderer>();
            trail.transform.position = wheelPos - (transform.up * wheel.radius);

            trail.transform.rotation = Quaternion.Euler(90,0,0);

            WheelHit hit;
            bool isGrounded = wheel.GetGroundHit(out hit);

            if (isGrounded && (driftOffset > 0.2f || isSlippingForward))
            {
                trail.emitting = true;
            }
            else
            {
                trail.emitting = false;
            }
            if (wheel.rpm < 100)
            {
                trail.emitting = false;
            }
        }

        public void InitiateSmoke(WheelCollider wheel)
        {
            Vector3 wheelPos;
            Quaternion wheelRot;
            wheel.GetWorldPose(out wheelPos, out wheelRot);

            wheel.GetComponentInChildren<ParticleSystem>().transform.position = wheelPos - (transform.up * wheel.radius);

            var particleSystem = wheel.GetComponentInChildren<ParticleSystem>();
            if (particleSystem != null)
            {
                var mainModule = particleSystem.main;
                if (visualModifications != null)
                {
                    mainModule.startColor = visualModifications.rearTireSmokeColor;
                    mainModule.startColor = visualModifications.frontTireSmokeColor;
                }
            }

            if (driftOffset > 0.2f || isSlippingForward)
            {
                wheel.GetComponentInChildren<ParticleSystem>().Play();
            }
            else
            {
                wheel.GetComponentInChildren<ParticleSystem>().Stop();
            }
            if(wheel.rpm < 100) 
            {
                wheel.GetComponentInChildren<ParticleSystem>().Stop();
            }
        }
        public void ControlLights()
        {
            if (carLights != null)
            {
                if (isEngineOn)
                {
                    if (inputSystem.controller.Driving.Lights.triggered)
                    {
                        isLightsOn = !isLightsOn;
                    }
                    if (carLights.leftPointBrake != null) carLights.brakeLightIntensity = brakeLightsIntensity;
                    if (carLights.rightPointBrake != null) carLights.brakeLightIntensity = brakeLightsIntensity;
                    if (carLights.leftPointBrake != null) carLights.leftPointBrake.enabled = true;
                    if (carLights.rightPointBrake != null) carLights.rightPointBrake.enabled = true;

                    if (isLightsOn)
                    {
                        if (carLights.leftFrontLight != null) carLights.leftFrontLight.enabled = true;
                        if (carLights.rightFrontLight != null) carLights.rightFrontLight.enabled = true;
                        if (carLights.leftFrontPointLight != null) carLights.leftFrontPointLight.enabled = true;
                        if (carLights.rightFrontPointLight != null) carLights.rightFrontPointLight.enabled = true;
                        if (carLights.interiorLight != null) carLights.interiorLight.enabled = true;
                        if (carLights.underglowLight != null) carLights.underglowLight.enabled = true;
                    }
                    else
                    {
                        if (carLights.leftFrontLight != null) carLights.leftFrontLight.enabled = false;
                        if (carLights.rightFrontLight != null) carLights.rightFrontLight.enabled = false;
                        if (carLights.leftFrontPointLight != null) carLights.leftFrontPointLight.enabled = false;
                        if (carLights.rightFrontPointLight != null) carLights.rightFrontPointLight.enabled = false;
                        if (carLights.interiorLight != null) carLights.interiorLight.enabled = false;
                        if (carLights.underglowLight != null) carLights.underglowLight.enabled = false;
                    }
                    if (inputSystem.brakeInput > 0)
                    {
                        if (!isReverse)
                        {
                            if (carLights.leftReversePointLight != null) carLights.leftReversePointLight.enabled = false;
                            if (carLights.rightReversePointLight != null) carLights.rightReversePointLight.enabled = false;
                        }
                        else
                        {
                            if (carLights.leftPointBrake != null) carLights.brakeLightIntensity =brakeLightsIntensity / 4;
                            if (carLights.rightPointBrake != null) carLights.brakeLightIntensity = brakeLightsIntensity / 4;

                            if (carLights.leftReversePointLight != null) carLights.leftReversePointLight.enabled = true;
                            if (carLights.rightReversePointLight != null) carLights.rightReversePointLight.enabled = true;
                        }
                    }
                    else
                    {
                        if (carLights.leftPointBrake != null) carLights.brakeLightIntensity = brakeLightsIntensity / 2;
                        if (carLights.rightPointBrake != null) carLights.brakeLightIntensity = brakeLightsIntensity / 2;

                        if (carLights.leftReversePointLight != null) carLights.leftReversePointLight.enabled = false;
                        if (carLights.rightReversePointLight != null) carLights.rightReversePointLight.enabled = false;
                    }
                }
                else
                {
                    if (carLights.leftFrontLight != null) carLights.leftFrontLight.enabled = false;
                    if (carLights.rightFrontLight != null) carLights.rightFrontLight.enabled = false;
                    if (carLights.leftFrontPointLight != null) carLights.leftFrontPointLight.enabled = false;
                    if (carLights.rightFrontPointLight != null) carLights.rightFrontPointLight.enabled = false;
                    if (carLights.interiorLight != null) carLights.interiorLight.enabled = false;
                    if (carLights.underglowLight != null) carLights.underglowLight.enabled = false;

                    if (carLights.leftPointBrake != null) carLights.leftPointBrake.enabled = false;
                    if (carLights.rightPointBrake != null) carLights.rightPointBrake.enabled = false;
                    if (carLights.leftReversePointLight != null) carLights.leftReversePointLight.enabled = false;
                    if (carLights.rightReversePointLight != null) carLights.rightReversePointLight.enabled = false;

                }
            }
        }
    }
}
