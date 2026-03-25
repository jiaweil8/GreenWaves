using UnityEditor;
using UnityEngine;

namespace KairaDigitalArts
{
    [CustomEditor(typeof(UCC_CarSettings))]
    public class UCC_CarSettingsEditor : Editor
    {
        private bool advancedFoldout = true;
        public override void OnInspectorGUI()
        {
            UCC_CarSettings carSettings = (UCC_CarSettings)target;
            serializedObject.Update();

            // Drivetrain Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Drivetrain Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("driveTrain"), new GUIContent("Drive Train"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("transmissionType"), new GUIContent("Transmission Type"));

            // Gear Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gear Settings", EditorStyles.boldLabel);
            carSettings.useClutch = EditorGUILayout.Toggle("Use Clutch", carSettings.useClutch);
            carSettings.maxGear = EditorGUILayout.IntField("Max Gear", carSettings.maxGear);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gearRatios"), new GUIContent("Gear Ratios"), true);
            carSettings.differantial = EditorGUILayout.Slider("Differential", carSettings.differantial, 1f, 5f);
            carSettings.minRPM = EditorGUILayout.FloatField("Minimum RPM", carSettings.minRPM);
            carSettings.maxRPM = EditorGUILayout.FloatField("Maximum RPM", carSettings.maxRPM);

            //Suspensions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Suspension", EditorStyles.boldLabel);
            carSettings.rearSuspensionLength = EditorGUILayout.FloatField("Rear Suspension Lenght", carSettings.rearSuspensionLength);
            carSettings.frontSuspensionLength = EditorGUILayout.FloatField("Front Suspension Lenght", carSettings.frontSuspensionLength);
            carSettings.dampingRatio = EditorGUILayout.FloatField("Damping Ratio", carSettings.dampingRatio);
            carSettings.naturalFrequency = EditorGUILayout.FloatField("Natural Frequency", carSettings.naturalFrequency);
            // Dimensions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dimensions", EditorStyles.boldLabel);
            carSettings.height = EditorGUILayout.FloatField("Height (m)", carSettings.height);
            carSettings.width = EditorGUILayout.FloatField("Width (m)", carSettings.width);
            carSettings.wheelBase = EditorGUILayout.FloatField("Wheel Base (m)", carSettings.wheelBase);
            carSettings.trackWidth = EditorGUILayout.FloatField("Track Width (m)", carSettings.trackWidth);

            // Steering
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Steering", EditorStyles.boldLabel);
            carSettings.maxSteerAngle = EditorGUILayout.FloatField("Max Steer Angle", carSettings.maxSteerAngle);
            carSettings.minSteerAngle = EditorGUILayout.FloatField("Min Steer Angle", carSettings.minSteerAngle);
            carSettings.maxSteeringWheelRotation = EditorGUILayout.FloatField("Steering Wheel Angle", carSettings.maxSteeringWheelRotation);

            // Wheel Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wheel Settings", EditorStyles.boldLabel);
            carSettings.rearWheelTorqueDistribution = EditorGUILayout.Slider("Rear Wheel Torque Distribution", carSettings.rearWheelTorqueDistribution, 0.1f, 0.9f);
            carSettings.rearWheelBrakeDistribution = EditorGUILayout.Slider("Rear Wheel Brake Distribution", carSettings.rearWheelBrakeDistribution, 0f, 1f);
            carSettings.rearWheelRadius = EditorGUILayout.Slider("Rear Wheel Radius", carSettings.rearWheelRadius, 0.1f, 2f);
            carSettings.rearWheelColliderRadius = EditorGUILayout.Slider("Rear Wheel Collider Radius", carSettings.rearWheelColliderRadius, 0.1f, 2f);
            carSettings.frontWheelRadius = EditorGUILayout.Slider("Front Wheel Radius", carSettings.frontWheelRadius, 0.1f, 2f);
            carSettings.frontWheelColliderRadius = EditorGUILayout.Slider("Front Wheel Collider Radius", carSettings.frontWheelColliderRadius, 0.1f, 2f);

            // Torque and Brakes
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Torque and Brakes", EditorStyles.boldLabel);
            carSettings.shiftDelayDuration = EditorGUILayout.Slider("Shift Delay Duration", carSettings.shiftDelayDuration, 0.1f, 1f);
            carSettings.torqueCurve = EditorGUILayout.CurveField("Torque Curve", carSettings.torqueCurve);
            carSettings.maxTorque = EditorGUILayout.FloatField("Max Torque", carSettings.maxTorque);
            carSettings.brakeTorque = EditorGUILayout.FloatField("Brake Torque", carSettings.brakeTorque);
            carSettings.handbrakeTorque = EditorGUILayout.FloatField("Handbrake Torque", carSettings.handbrakeTorque);

            // Downforce
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Downforce Settings", EditorStyles.boldLabel);
            carSettings.forceShift = EditorGUILayout.FloatField("Force Shift", carSettings.forceShift);
            carSettings.aerodynamicEfficiency = EditorGUILayout.Slider("Aerodynamic Efficiency", carSettings.aerodynamicEfficiency, 0.1f, 1f);

            // N2O Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nitrous Oxide (N2O) Settings", EditorStyles.boldLabel);
            carSettings.useN2O = EditorGUILayout.Toggle("Use N2O", carSettings.useN2O);
            if (carSettings.useN2O)
            {
                carSettings.n2oMaxDuration = EditorGUILayout.FloatField("Max. N2O Duration", carSettings.n2oMaxDuration);
                carSettings.n2oPower = EditorGUILayout.Slider("N2O Power", carSettings.n2oPower, 1.1f, 2.5f);
                carSettings.n2oCooldownMultiplier = EditorGUILayout.Slider("N2O Cooldown Multiplier", carSettings.n2oCooldownMultiplier, 0f, 2f);
            }

            // Setup Presets
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Setup Presets", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Drift")) SetDriftSettings(carSettings);
            if (GUILayout.Button("Racing")) SetRacingSettings(carSettings);
            if (GUILayout.Button("Drag")) SetDragSettings(carSettings);
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
            // Advanced Settings
            EditorGUILayout.Space();
            advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced Settings", true);
            if (advancedFoldout)
            {
                DrawAdvancedSettings(carSettings);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(carSettings);
            }
        }

        private void DrawAdvancedSettings(UCC_CarSettings carSettings)
        {
            EditorGUILayout.LabelField("Tire Stiffness", EditorStyles.boldLabel);
            carSettings.frontTireStifness = EditorGUILayout.Slider("Front Tire Stiffness", carSettings.frontTireStifness, 0.1f, 2.5f);
            carSettings.rearTireStifness = EditorGUILayout.Slider("Rear Tire Stiffness", carSettings.rearTireStifness, 0.1f, 2.5f);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Front Wheel Friction", EditorStyles.boldLabel);
            carSettings.frontForwardExtremumSlip = EditorGUILayout.FloatField("Forward Extremum Slip", carSettings.frontForwardExtremumSlip);
            carSettings.frontForwardExtremumValue = EditorGUILayout.FloatField("Forward Extremum Value", carSettings.frontForwardExtremumValue);
            carSettings.frontSidewaysExtremumSlip = EditorGUILayout.FloatField("Sideways Extremum Slip", carSettings.frontSidewaysExtremumSlip);
            carSettings.frontSidewaysExtremumValue = EditorGUILayout.FloatField("Sideways Extremum Value", carSettings.frontSidewaysExtremumValue);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rear Wheel Friction", EditorStyles.boldLabel);
            carSettings.rearForwardExtremumSlip = EditorGUILayout.FloatField("Forward Extremum Slip", carSettings.rearForwardExtremumSlip);
            carSettings.rearForwardExtremumValue = EditorGUILayout.FloatField("Forward Extremum Value", carSettings.rearForwardExtremumValue);
            carSettings.rearSidewaysExtremumSlip = EditorGUILayout.FloatField("Sideways Extremum Slip", carSettings.rearSidewaysExtremumSlip);
            carSettings.rearSidewaysExtremumValue = EditorGUILayout.FloatField("Sideways Extremum Value", carSettings.rearSidewaysExtremumValue);
        }
        private void SetDriftSettings(UCC_CarSettings carSettings)
        {
            carSettings.frontTireStifness = 1.4f;
            carSettings.rearTireStifness = 1.2f;

            carSettings.frontForwardExtremumSlip = 1.5f;
            carSettings.frontForwardExtremumValue = 0.8f;
            carSettings.frontForwardAsymptoteSlip = 2.0f;
            carSettings.frontForwardAsymptoteValue = 0.5f;

            carSettings.frontSidewaysExtremumSlip = 1.3f;
            carSettings.frontSidewaysExtremumValue = 0.7f;
            carSettings.frontSidewaysAsymptoteSlip = 1.8f;
            carSettings.frontSidewaysAsymptoteValue = 0.6f;

            carSettings.rearForwardExtremumSlip = 1f;
            carSettings.rearForwardExtremumValue = 0.9f;
            carSettings.rearForwardAsymptoteSlip = 1.5f;
            carSettings.rearForwardAsymptoteValue = 0.6f;

            carSettings.rearSidewaysExtremumSlip = 1.5f;
            carSettings.rearSidewaysExtremumValue = 0.6f;
            carSettings.rearSidewaysAsymptoteSlip = 2.0f;
            carSettings.rearSidewaysAsymptoteValue = 0.4f;
        }
        private void SetRacingSettings(UCC_CarSettings carSettings)
        {
            carSettings.frontTireStifness = 2.2f;
            carSettings.rearTireStifness = 2.4f;

            carSettings.frontForwardExtremumSlip = 1.0f;
            carSettings.frontForwardExtremumValue = 1.0f;
            carSettings.frontForwardAsymptoteSlip = 0.5f;
            carSettings.frontForwardAsymptoteValue = 0.8f;

            carSettings.frontSidewaysExtremumSlip = 0.2f;
            carSettings.frontSidewaysExtremumValue = 1.0f;
            carSettings.frontSidewaysAsymptoteSlip = 1.5f;
            carSettings.frontSidewaysAsymptoteValue = 2f;

            carSettings.rearForwardExtremumSlip = 1.0f;
            carSettings.rearForwardExtremumValue = 1f;
            carSettings.rearForwardAsymptoteSlip = 0.5f;
            carSettings.rearForwardAsymptoteValue = 0.8f;

            carSettings.rearSidewaysExtremumSlip = 0.2f;
            carSettings.rearSidewaysExtremumValue = 1f;
            carSettings.rearSidewaysAsymptoteSlip = 1.5f;
            carSettings.rearSidewaysAsymptoteValue = 2f;
        }
        private void SetDragSettings(UCC_CarSettings carSettings)
        {
            carSettings.frontTireStifness = 2.5f;
            carSettings.rearTireStifness = 2.5f;

            carSettings.frontForwardExtremumSlip = 2f;
            carSettings.frontForwardExtremumValue = 1.5f;
            carSettings.frontForwardAsymptoteSlip = 1.2f;
            carSettings.frontForwardAsymptoteValue = 0.6f;

            carSettings.frontSidewaysExtremumSlip = 0.7f;
            carSettings.frontSidewaysExtremumValue = 0.6f;
            carSettings.frontSidewaysAsymptoteSlip = 0.25f;
            carSettings.frontSidewaysAsymptoteValue = 1.2f;

            carSettings.rearForwardExtremumSlip = 2f;
            carSettings.rearForwardExtremumValue = 2.25f;
            carSettings.rearForwardAsymptoteSlip = 1.2f;
            carSettings.rearForwardAsymptoteValue = 2.5f;

            carSettings.rearSidewaysExtremumSlip = 0.5f;
            carSettings.rearSidewaysExtremumValue = 1.8f;
            carSettings.rearSidewaysAsymptoteSlip = 0.5f;
            carSettings.rearSidewaysAsymptoteValue = 1.5f;
        }
    }
}
