using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;

namespace KairaDigitalArts
{
    class CarEditor : EditorWindow
    {
        private float mass = 1500;
        private float carLength = 4;
        private float carWidth = 1.5f;
        private float carHeight = 1.5f;
        private float axleShift = -0.5f;
        private float wheelRadius = 1f;

        private float maxTorque = 500f;
        private float differentialLength = 3.0f;

        private GameObject car;

        private AnimationCurve torqueCurve = new AnimationCurve(
            new Keyframe(0, 0.3f),
            new Keyframe(0.2f, 0.7f),
            new Keyframe(0.3f, 1f),
            new Keyframe(0.5f, 0.8f),
            new Keyframe(1, 0.3f)
        );

        private float maxBrakeForce = 3000f;
        private float handbrakeForce = 5000f;

        private bool useVisualModification = false;
        private bool useGauge = false;
        private bool useElectronicSystems = false;
        private bool useDynamicCamera = false;
        private bool useCinemachineCamera = false;
        private bool useCarLights = false;
        private bool useExtras = false;
        private bool useCollisionDeformation = false;

        private GameObject selectedModel;
        private List<GameObject> colliderObjects = new List<GameObject>();
        private GameObject frontLeftWheel;
        private GameObject frontRightWheel;
        private GameObject rearLeftWheel;
        private GameObject rearRightWheel;
        private GameObject steeringWheel;
        private GameObject cockpitNeedle;

        private Vector2 scrollPosition;
        [MenuItem("Tools/Car Editor")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CarEditor));
        }
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            mass = EditorGUILayout.FloatField("Mass:* ", mass);
            carLength = EditorGUILayout.FloatField("Car Length: ", carLength);
            carWidth = EditorGUILayout.FloatField("Car Width: ", carWidth);
            carHeight = EditorGUILayout.FloatField("Car Height: ", carHeight);
            axleShift = EditorGUILayout.FloatField("Axle Shift: ", axleShift);
            wheelRadius = EditorGUILayout.FloatField("Wheel Radius: ", wheelRadius);

            differentialLength = EditorGUILayout.Slider("Differential Length:* ", differentialLength, 2.5f, 5.0f);

            maxTorque = EditorGUILayout.FloatField("Max Torque:* ", maxTorque);
            torqueCurve = EditorGUILayout.CurveField("Torque Curve*", torqueCurve);
            maxBrakeForce = EditorGUILayout.FloatField("Max Brake Force:* ", maxBrakeForce);
            handbrakeForce = EditorGUILayout.FloatField("Handbrake Force:* ", handbrakeForce);
            selectedModel = (GameObject)EditorGUILayout.ObjectField("Car Model*", selectedModel, typeof(GameObject), true);
            EditorGUILayout.LabelField("Collider Objects:*");
            for (int i = 0; i < colliderObjects.Count; i++)
            {
                colliderObjects[i] = (GameObject)EditorGUILayout.ObjectField("Collider Object " + (i + 1), colliderObjects[i], typeof(GameObject), true);
            }

            if (GUILayout.Button("Add Collider Object"))
            {
                colliderObjects.Add(null);
            }

            if (colliderObjects.Count > 0 && GUILayout.Button("Remove Collider Object"))
            {
                colliderObjects.RemoveAt(colliderObjects.Count - 1);
            }

            frontLeftWheel = (GameObject)EditorGUILayout.ObjectField("Front Left Wheel*", frontLeftWheel, typeof(GameObject), true);
            frontRightWheel = (GameObject)EditorGUILayout.ObjectField("Front Right Wheel*", frontRightWheel, typeof(GameObject), true);
            rearLeftWheel = (GameObject)EditorGUILayout.ObjectField("Rear Left Wheel*", rearLeftWheel, typeof(GameObject), true);
            rearRightWheel = (GameObject)EditorGUILayout.ObjectField("Rear Right Wheel*", rearRightWheel, typeof(GameObject), true);
            steeringWheel = (GameObject)EditorGUILayout.ObjectField("Steering Wheel", steeringWheel, typeof(GameObject), true);
            cockpitNeedle = (GameObject)EditorGUILayout.ObjectField("Cockpit Rpm Needle", cockpitNeedle, typeof(GameObject), true);
            useVisualModification = EditorGUILayout.Toggle("Use Visual Modification", useVisualModification);
            useGauge = EditorGUILayout.Toggle("Use Gauge", useGauge);
            useElectronicSystems = EditorGUILayout.Toggle("Use Electronic Systems", useElectronicSystems);
            useDynamicCamera = EditorGUILayout.Toggle("Use Dynamic Camera", useDynamicCamera);
            useCinemachineCamera = EditorGUILayout.Toggle("Use Cinemachine Camera", useCinemachineCamera);
            useCarLights = EditorGUILayout.Toggle("Use Car Lights", useCarLights);
            useExtras = EditorGUILayout.Toggle("Use Extra Options", useExtras);
            useCollisionDeformation = EditorGUILayout.Toggle("Collision Deformation", useCollisionDeformation);

            if (useCinemachineCamera)
            {
                useDynamicCamera = false;
            }
            else if (useDynamicCamera)
            {
                useCinemachineCamera = false;
            }

            if (GUILayout.Button("Generate"))
            {
                CreateCar();
            }

            EditorGUILayout.EndScrollView();
        }
        void CreateCar()
        {
            car = new GameObject("Player Car");
            var rootBody = car.AddComponent<Rigidbody>();
            rootBody.mass = mass;
            rootBody.interpolation = RigidbodyInterpolation.Interpolate;
            rootBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rootBody.drag = 0.035f;
            var carController = car.AddComponent<UCC_CarController>();

            if (selectedModel != null)
            {
                selectedModel.transform.parent = car.transform;
                selectedModel.transform.position = Vector3.zero;
            }

            if (selectedModel != null && colliderObjects != null)
            {
                foreach (var obj in colliderObjects)
                {
                    var meshCollider = obj.gameObject.AddComponent<MeshCollider>();
                    meshCollider.convex = true;
                }
            }
            GameObject frontLeftWheelCollider = new GameObject("Front Left Wheel");
            frontLeftWheelCollider.transform.localPosition = frontLeftWheel.transform.position;
            frontLeftWheelCollider.AddComponent<WheelCollider>();


            GameObject frontLeftWheelSteer = new GameObject("Steer Object");
            frontLeftWheelSteer.transform.localPosition = frontLeftWheel.transform.position;

            CreateSkidmarksAndSmoke(frontLeftWheelCollider);
            carController.frontLeftWheel = frontLeftWheelCollider.GetComponent<WheelCollider>();

            frontLeftWheelSteer.transform.parent = frontLeftWheelCollider.transform;
            frontLeftWheel.transform.parent = frontLeftWheelSteer.transform;

            GameObject frontRightWheelCollider = new GameObject("Front Right Wheel");
            frontRightWheelCollider.transform.localPosition = frontRightWheel.transform.position;
            frontRightWheelCollider.AddComponent<WheelCollider>();

            GameObject frontRightWheelSteer = new GameObject("Steer Object");
            frontRightWheelSteer.transform.localPosition = frontRightWheel.transform.position;

            CreateSkidmarksAndSmoke(frontRightWheelCollider);
            carController.frontRightWheel = frontRightWheelCollider.GetComponent<WheelCollider>();

            frontRightWheelSteer.transform.parent = frontRightWheelCollider.transform;
            frontRightWheel.transform.parent = frontRightWheelSteer.transform;

            GameObject rearLeftWheelCollider = new GameObject("Rear Left Wheel");
            rearLeftWheelCollider.transform.localPosition = rearLeftWheel.transform.position;
            rearLeftWheelCollider.AddComponent<WheelCollider>();

            GameObject rearLeftWheelSteer = new GameObject("Steer Object");
            rearLeftWheelSteer.transform.localPosition = rearLeftWheel.transform.position;

            CreateSkidmarksAndSmoke(rearLeftWheelCollider);
            carController.rearLeftWheel = rearLeftWheelCollider.GetComponent<WheelCollider>();

            rearLeftWheelSteer.transform.parent = rearLeftWheelCollider.transform;
            rearLeftWheel.transform.parent = rearLeftWheelSteer.transform;

            GameObject rearRightWheelCollider = new GameObject("Rear Right Wheel");
            rearRightWheelCollider.transform.localPosition = rearRightWheel.transform.position;
            rearRightWheelCollider.AddComponent<WheelCollider>();

            GameObject rearRightWheelSteer = new GameObject("Steer Object");
            rearRightWheelSteer.transform.localPosition = rearRightWheel.transform.position;

            CreateSkidmarksAndSmoke(rearRightWheelCollider);
            carController.rearRightWheel = rearRightWheelCollider.GetComponent<WheelCollider>();

            rearRightWheelSteer.transform.parent = rearRightWheelCollider.transform;
            rearRightWheel.transform.parent = rearRightWheelSteer.transform;

            car.AddComponent<UCC_CarSettings>();
            car.AddComponent<UCC_EngineSound>();
            car.AddComponent<UCC_InputSystem>();

            var inputSystem = car.GetComponent<UCC_InputSystem>();
            var engineSound = car.GetComponent<UCC_EngineSound>();
            GameObject audioSources = new GameObject("Audio Sources");
            audioSources.transform.parent = car.transform;

            GameObject idleObj = new GameObject("Idle Sound");
            idleObj.transform.parent = audioSources.transform;
            AudioSource idleSound = idleObj.AddComponent<AudioSource>();
            idleSound.loop = true;
            idleSound.playOnAwake = false;

            GameObject driveSoundLowObj = new GameObject("Drive Sound Low");
            driveSoundLowObj.transform.parent = audioSources.transform;
            AudioSource driveSoundLow = driveSoundLowObj.AddComponent<AudioSource>();
            driveSoundLow.loop = true;
            driveSoundLow.playOnAwake = false;

            GameObject driveSoundHighObj = new GameObject("Drive Sound High");
            driveSoundHighObj.transform.parent = audioSources.transform;
            AudioSource driveSoundHigh = driveSoundHighObj.AddComponent<AudioSource>();
            driveSoundHigh.loop = true;
            driveSoundHigh.playOnAwake = false;

            GameObject maxRpmObj = new GameObject("Max RPM Sound");
            maxRpmObj.transform.parent = audioSources.transform;
            AudioSource maxRpm = maxRpmObj.AddComponent<AudioSource>();
            maxRpm.loop = true;
            maxRpm.playOnAwake = false;

            GameObject suspensionSoundObj = new GameObject("Suspension Sound");
            suspensionSoundObj.transform.parent = audioSources.transform;
            AudioSource suspensionSound = suspensionSoundObj.AddComponent<AudioSource>();
            suspensionSound.loop = false;
            suspensionSound.playOnAwake = false;

            GameObject tireSoundObj = new GameObject("Tire Sound");
            tireSoundObj.transform.parent = audioSources.transform;
            AudioSource tireSound = tireSoundObj.AddComponent<AudioSource>();
            tireSound.loop = true;
            tireSound.playOnAwake = false;

            GameObject gearShiftObj = new GameObject("Gear Shift Sound");
            gearShiftObj.transform.parent = audioSources.transform;
            AudioSource gearShift = gearShiftObj.AddComponent<AudioSource>();
            gearShift.loop = false;
            gearShift.playOnAwake = false;

            GameObject reverseSoundObj = new GameObject("Reverse Sound");
            reverseSoundObj.transform.parent = audioSources.transform;
            AudioSource reverseSound = reverseSoundObj.AddComponent<AudioSource>();
            reverseSound.loop = true;
            reverseSound.playOnAwake = false;

            GameObject engineOnSoundObj = new GameObject("Engine Start Sound");
            engineOnSoundObj.transform.parent = audioSources.transform;
            AudioSource engineOnSound = engineOnSoundObj.AddComponent<AudioSource>();
            engineOnSound.loop = false;
            engineOnSound.playOnAwake = false;

            GameObject engineOffSoundObj = new GameObject("Engine Shutdown Sound");
            engineOffSoundObj.transform.parent = audioSources.transform;
            AudioSource engineOffSound = engineOffSoundObj.AddComponent<AudioSource>();
            engineOffSound.loop = false;
            engineOffSound.playOnAwake = false;

            GameObject nitroSoundObj = new GameObject("N2O Sound");
            nitroSoundObj.transform.parent = audioSources.transform;
            AudioSource nitroSound = nitroSoundObj.AddComponent<AudioSource>();
            nitroSound.loop = true;
            nitroSound.playOnAwake = false;

            GameObject turboSoundObj = new GameObject("Turbo Sound");
            turboSoundObj.transform.parent = audioSources.transform;
            AudioSource turboSound = turboSoundObj.AddComponent<AudioSource>();
            turboSound.loop = false;
            turboSound.playOnAwake = false;

            GameObject turboDownSoundObj = new GameObject("Turbo Downshift Sound");
            turboDownSoundObj.transform.parent = audioSources.transform;
            AudioSource turboDownSound = turboDownSoundObj.AddComponent<AudioSource>();
            turboDownSound.loop = false;
            turboDownSound.playOnAwake = false;

            GameObject exhaustFlamesObj = new GameObject("Exhaust Flames Sound");
            exhaustFlamesObj.transform.parent = audioSources.transform;
            AudioSource exhaustFlamesSound = exhaustFlamesObj.AddComponent<AudioSource>();
            exhaustFlamesSound.loop = false;
            exhaustFlamesSound.playOnAwake = false;

            engineSound.driveAudioSourceLow = driveSoundLow;
            engineSound.driveAudioSourceHigh = driveSoundHigh;
            engineSound.reverseAudioSource = reverseSound;
            engineSound.engineStartAudioSource = engineOnSound;
            engineSound.engineShutdownAudioSource = engineOffSound;
            engineSound.suspensionAudioSource = suspensionSound;
            engineSound.gearShiftAudioSource = gearShift;
            engineSound.tireScreechAudioSource = tireSound;
            engineSound.n20AudioSource = nitroSound;
            engineSound.exhaustFlameAudioSource = exhaustFlamesSound;
            engineSound.turboAudioSource = turboSound;
            engineSound.turboDownShiftAudioSource = turboDownSound;
            engineSound.idleAudioSource = idleSound;
            engineSound.maxRpmSound = maxRpm;

            var carSettings = car.GetComponent<UCC_CarSettings>();
            carSettings.carController = carController;

            carSettings.differantial = differentialLength;

            if (useVisualModification)
            {
                var visualModifications = car.AddComponent<UCC_VisualModifications>();
                car.GetComponent<UCC_CarController>().visualModifications = visualModifications;
                visualModifications.rearTireSmokeColor = Color.gray;
                visualModifications.frontTireSmokeColor = Color.gray;
                visualModifications.carController = carController;
            }

            if (useGauge)
            {
                var gauge = car.AddComponent<UCC_Gauge>();
                car.GetComponent<UCC_Gauge>().carController = carController;
                carController.gauge = gauge;
                carController.cockpitGagueNeedle = cockpitNeedle;
            }

            if (useElectronicSystems)
            {
                var electronicSystems = car.AddComponent<UCC_ElectronicSystems>();
                car.GetComponent<UCC_CarController>().electronicSystems = electronicSystems;
                car.GetComponent<UCC_ElectronicSystems>().carController = carController;
                car.GetComponent<UCC_ElectronicSystems>().carSettings = carSettings;
            }

            if (useDynamicCamera)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    GameObject cameraObject = new GameObject("Main Camera");
                    mainCamera = cameraObject.AddComponent<Camera>();

                    cameraObject.gameObject.tag = "MainCamera";
                    cameraObject.gameObject.AddComponent<AudioListener>();
                }
                GameObject farCamera = new GameObject("Far Camera");
                farCamera.transform.parent = car.transform;
                farCamera.transform.position = new Vector3(0,2,-10);
                GameObject cockpitCamera = new GameObject("Cockpit Camera");
                cockpitCamera.transform.parent = car.transform;
                cockpitCamera.transform.position = Vector3.zero;

                var dynamicCamera = mainCamera.AddComponent<UCC_DynamicCamera>();
                dynamicCamera.carController = carController;
                dynamicCamera.inputSystem = inputSystem;

                dynamicCamera.car = car.transform;
                dynamicCamera.chaseCameraLocation = farCamera.transform;
                dynamicCamera.actionCameraLocation = farCamera.transform;
                dynamicCamera.cockpitCameraLocation = cockpitCamera.transform;

                dynamicCamera.smoothTime = 10;

                dynamicCamera.inputSystem = inputSystem;
            }

            if (useCarLights)
            {
                var carLights = car.AddComponent<UCC_CarLights>();
                car.GetComponent<UCC_CarController>().carLights = carLights;
                SetupCarLights();
            }

            if (useCinemachineCamera)
            {
                Camera mainCamera = Camera.main;

                if (mainCamera == null)
                {
                    GameObject cameraObject = new GameObject("Main Camera");
                    mainCamera = cameraObject.AddComponent<Camera>();

                    cameraObject.gameObject.AddComponent<CinemachineBrain>();

                    cameraObject.gameObject.tag = "MainCamera";
                    cameraObject.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    if (mainCamera.GetComponent<CinemachineBrain>() == null)
                    {
                        mainCamera.gameObject.AddComponent<CinemachineBrain>();
                        mainCamera.gameObject.AddComponent<UCC_CinemachineCamera>();
                    }
                    if (mainCamera.GetComponent<AudioListener>() == null)
                    {
                        mainCamera.gameObject.AddComponent<AudioListener>();
                    }
                }
                UCC_CinemachineCamera cinemamachinCam = mainCamera.AddComponent< UCC_CinemachineCamera>();
                cinemamachinCam.carController = carController;
                cinemamachinCam.playerCar = car;
                cinemamachinCam.inputSystem = inputSystem;
                cinemamachinCam.mainCam = mainCamera;
                CreateCinemachineCameras();
            }

            if (useExtras) 
            {
                var carExtras = car.AddComponent<UCC_ExtraCarOptions>();
                carExtras.carController = carController;
                carController.carExtras = carExtras;

                GameObject n2oPrefab = null;
                GameObject exhaustFlame = null;
                GameObject idleExhaust = null;

                if (GraphicsSettings.currentRenderPipeline == null)
                {
                    n2oPrefab = CreatePrefab("N2O");
                    exhaustFlame = CreatePrefab("ExhaustFlame");
                    idleExhaust = CreatePrefab("ExhaustIdle");
                }
                else if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
                {
                   n2oPrefab = CreatePrefab("N2OUrp");
                   exhaustFlame = CreatePrefab("ExhaustFlameUrp");
                   idleExhaust = CreatePrefab("ExhaustIdleUrp");
                }

                n2oPrefab.transform.rotation = Quaternion.Euler(n2oPrefab.transform.rotation.eulerAngles.x, 180, n2oPrefab.transform.rotation.eulerAngles.z);
                exhaustFlame.transform.rotation = Quaternion.Euler(exhaustFlame.transform.rotation.eulerAngles.x, 180, exhaustFlame.transform.rotation.eulerAngles.z);
                idleExhaust.transform.rotation = Quaternion.Euler(idleExhaust.transform.rotation.eulerAngles.x, 180, idleExhaust.transform.rotation.eulerAngles.z);

                n2oPrefab.transform.parent = car.transform;
                n2oPrefab.transform.position = Vector3.zero;
                carExtras.n2o.Add(n2oPrefab.GetComponent<ParticleSystem>());

                exhaustFlame.transform.parent = car.transform;
                exhaustFlame.transform.position = Vector3.zero;
                carExtras.exhaustFlames.Add(exhaustFlame.GetComponent<ParticleSystem>());

                idleExhaust.transform.parent = car.transform;
                idleExhaust.transform.position = Vector3.zero;
                carExtras.carExhaustIdle.Add(idleExhaust.GetComponent<ParticleSystem>());
            }
            carSettings.rearWheelRadius = wheelRadius;
            carSettings.frontWheelRadius = wheelRadius;
            carSettings.frontWheelColliderRadius = wheelRadius / 2;
            carSettings.rearWheelColliderRadius = wheelRadius / 2;
            carSettings.maxTorque = maxTorque;
            carSettings.torqueCurve = torqueCurve;
            carSettings.brakeTorque = maxBrakeForce;
            carSettings.handbrakeTorque = handbrakeForce;
            carSettings.frontTireStifness = 1.2f;
            carSettings.rearTireStifness = 1.2f;
            carSettings.height = carHeight;
            carSettings.width = carWidth;

            carController.carRigidbody = rootBody;
            carController.carSettings = carSettings;
            carController.engineSound = engineSound;
            carController.inputSystem = inputSystem;

            if (steeringWheel == null)
            {
                GameObject steeringWheel = new GameObject("Steering Wheel");
                steeringWheel.transform.parent = car.transform;
                carController.steeringWheel = steeringWheel;
            }
            else
            {
                carController.steeringWheel = steeringWheel;
            }
            if (useCollisionDeformation)
            {
                List<MeshFilter> meshFilters = new List<MeshFilter>();
                var collisionHandler =  car.AddComponent<UCC_CollisionHandler>();
                foreach (var collider in colliderObjects)
                {
                    meshFilters.Add(collider.gameObject.GetComponent<MeshFilter>());
                }
                collisionHandler.meshFilter = meshFilters.ToArray();
            }
        }
        private void CreateCinemachineCameras()
        {
            Camera.main.GetComponent<UCC_CinemachineCamera>().virtualCameras = new CinemachineVirtualCamera[4];
            CreateCockpitCamera();
            CreateActionCamera();
            CreateChaseCamera();
            CreateTopCamera();
        }
        void CreateSkidmarksAndSmoke(GameObject wheelCollider)
        {
            wheelCollider.GetComponent<WheelCollider>().radius = wheelRadius;
            wheelCollider.transform.parent = car.transform;

            var skidmark = new GameObject("Skidmark");
            skidmark.transform.parent = wheelCollider.transform;
            skidmark.transform.localPosition = Vector3.down * wheelRadius;

            GameObject tireSmoke = null;
           if (GraphicsSettings.currentRenderPipeline == null)
           {
                tireSmoke = CreatePrefab("TireSmoke");
           }
           else if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
           {
               tireSmoke = CreatePrefab("TireSmokeUrp");
           }

            if (tireSmoke != null)
            {
                tireSmoke.transform.parent = wheelCollider.transform;
                tireSmoke.transform.localPosition = Vector3.down * wheelRadius;
            }
            else
            {
                Debug.LogError("TireSmoke prefab not found.");
            }
            TrailRenderer trail = skidmark.AddComponent<TrailRenderer>();

            Material skidmarkMaterial = Resources.Load<Material>("Skidmark");

            if (skidmarkMaterial != null)
            {
                trail.material = skidmarkMaterial;
            }
            else
            {
                Debug.LogError("Skidmark material not found in Resources folder.");
            }
            Gradient grayGradient = new Gradient();
            grayGradient.colorKeys = new GradientColorKey[]
            {
    new GradientColorKey(Color.gray, 0.0f), 
    new GradientColorKey(Color.black, 0.5f),
    new GradientColorKey(Color.black, 1.0f) 
            };

            grayGradient.alphaKeys = new GradientAlphaKey[]
            {
    new GradientAlphaKey(0.3f, 0.0f), 
    new GradientAlphaKey(0.4f, 0.2f), 
    new GradientAlphaKey(0.6f, 1.0f)  
            };

            trail.colorGradient = grayGradient;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.startWidth = 0.3f;
            trail.time = 20f;
            trail.endWidth = 0.2f;
            trail.alignment = LineAlignment.TransformZ;
            trail.textureMode = LineTextureMode.Static;
            trail.textureScale = new Vector2(1, 0.1f);
        }
        private void CreateCockpitCamera()
        {
            GameObject vCamObject = new GameObject("Cockpit Camera");
            CinemachineVirtualCamera virtualCamera = vCamObject.AddComponent<CinemachineVirtualCamera>();

            virtualCamera.Follow = car.transform;

            var thirdPersonFollow = virtualCamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();

            thirdPersonFollow.CameraDistance = 0f;
            thirdPersonFollow.VerticalArmLength = 0f;
            thirdPersonFollow.CameraSide = 0f;
            thirdPersonFollow.Damping = new Vector3(0.02f, 0.05f, 0.02f);
            thirdPersonFollow.ShoulderOffset = new Vector3(-0.5f, 0.5f, 0.5f);
            Camera.main.GetComponent<UCC_CinemachineCamera>().virtualCameras[1] = virtualCamera;
            virtualCamera.Priority = 0;
        }
        private void CreateActionCamera()
        {
            GameObject vCamObject = new GameObject("Action Camera");
            CinemachineVirtualCamera virtualCamera = vCamObject.AddComponent<CinemachineVirtualCamera>();

            virtualCamera.Follow = car.transform;
            virtualCamera.LookAt = car.transform;

            var thirdPersonFollow = virtualCamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
            thirdPersonFollow.CameraDistance = 10f;
            thirdPersonFollow.VerticalArmLength = 2.5f;
            thirdPersonFollow.CameraSide = 0.5f; 
            thirdPersonFollow.Damping = new Vector3(1f, 0.2f, 0.4f);
            thirdPersonFollow.ShoulderOffset = new Vector3(0,0,0);
            var hardLook = virtualCamera.AddCinemachineComponent<CinemachineHardLookAt>();
            Camera.main.GetComponent<UCC_CinemachineCamera>().virtualCameras[0] = virtualCamera;
            virtualCamera.Priority = 10;
        }
        private void CreateChaseCamera()
        {
            GameObject vCamObject = new GameObject("Chase Camera");
            CinemachineVirtualCamera virtualCamera = vCamObject.AddComponent<CinemachineVirtualCamera>();

            virtualCamera.Follow = car.transform;
            virtualCamera.LookAt = car.transform;

            var thirdPersonFollow = virtualCamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
            thirdPersonFollow.CameraDistance = 10f;
            thirdPersonFollow.CameraSide = 0.5f;
            thirdPersonFollow.VerticalArmLength = 2f;
            thirdPersonFollow.Damping = new Vector3(0.1f, 0f, 0f);
            var hardLook = virtualCamera.AddCinemachineComponent<CinemachineHardLookAt>();
            Camera.main.GetComponent<UCC_CinemachineCamera>().virtualCameras[2] = virtualCamera;
            virtualCamera.Priority = 0;
        }
        private void CreateTopCamera()
        {
            GameObject vCamObject = new GameObject("Top Camera");
            CinemachineVirtualCamera virtualCamera = vCamObject.AddComponent<CinemachineVirtualCamera>();

            virtualCamera.Follow = car.transform;
            virtualCamera.LookAt = car.transform;

            var transposer = virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            transposer.m_FollowOffset = new Vector3(0f, 40f, 0f);
            transposer.m_XDamping = 5f;
            transposer.m_YDamping = 5f;
            transposer.m_ZDamping = 5f;
            transposer.m_YawDamping = 5f;
            Camera.main.GetComponent<UCC_CinemachineCamera>().virtualCameras[3] = virtualCamera;
            var hardLook = virtualCamera.AddCinemachineComponent<CinemachineHardLookAt>();
            virtualCamera.Priority = 0;
        }
        private void SetupCarLights()
        {
            Transform carTransform = car.transform;
            
            UCC_CarLights carLights = car.GetComponent<UCC_CarLights>();
            if (carLights == null)
            {
                Debug.LogError("UCC_CarLights component not found on the car.");
                return;
            }
        
            CreateSpotLight(1,carTransform, "Front Left Light", new Vector3(-0.5f, 0.5f, 3f), Color.white, 0f, 15f, 15f, 5f, 60, true);
            CreateSpotLight(2,carTransform, "Front Right Light", new Vector3(0.5f, 0.5f, 3f), Color.white, 0f, 15f, 15f, 5f, 60, true);
            carLights.frontLightPointIntensity = 5;
            carLights.frontLightPointRange = 0.35f;

            CreateSpotLight(3,carTransform, "Underglow Light", new Vector3(0f, -0.5f, 0f), Color.blue, 0f, 90f, 3f, 5f, 179f, false);
            carLights.interiorLight = CreatePointLight(carTransform, "Interior Light", Vector3.zero, Color.white, 2f, 5f);

            carLights.leftPointBrake = CreatePointLight(carTransform, "Left Brake Light", new Vector3(-0.75f, 0.75f, -2.5f), Color.red, 2f, 5f);
            carLights.rightPointBrake = CreatePointLight(carTransform, "Right Brake Light", new Vector3(0.75f, 0.75f, -2.5f), Color.red, 2f, 5f);
            carLights.brakeLightColor = Color.red;
            carLights.brakeLightIntensity = 1.5f;
            carLights.brakeLightRange = 3f;

            carLights.leftReversePointLight = CreatePointLight(carTransform, "Left Reverse Light", new Vector3(-0.5f, 0.75f, -2.5f), Color.white, 2f, 5f);
            carLights.rightReversePointLight = CreatePointLight(carTransform, "Right Reverse Light", new Vector3(0.5f, 0.75f, -2.5f), Color.white, 2f, 5f);
            carLights.reverseLightColor = Color.white;
            carLights.reverseLightIntensity = 1f;
            carLights.reverseLightRange = 3f;
        }

        private void CreateSpotLight(int id,Transform parent, string name, Vector3 position, Color color, float rotationY, float rotationX = 0f, float range = 10f, float intensity = 7f, float angle = 60, bool havePointLight = true)
        {
            GameObject spotLightObject = new GameObject(name);
            spotLightObject.transform.parent = parent;
            spotLightObject.transform.localPosition = position;
            spotLightObject.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        
            Light spotLight = spotLightObject.AddComponent<Light>();
            spotLight.type = UnityEngine.LightType.Spot;
            spotLight.color = color;
            spotLight.intensity = intensity;
            spotLight.shadows = LightShadows.Soft;
            spotLight.range = range;
            spotLight.spotAngle = angle;
        
            Light  pointLight = null;

            if (havePointLight)
            {
                pointLight = CreatePointLight(spotLightObject.transform, name + " Point", Vector3.zero, color, 5f, 0.35f);
            }

            var carLights = car.GetComponent<UCC_CarLights>(); 

            switch (id)
            {
                case 1:
                    carLights.leftFrontLight = spotLight;
                    carLights.frontLightColor = color;
                    carLights.frontLightIntensity = intensity;
                    carLights.frontLightRange = range;
                    carLights.frontLightSpotAngle = angle;
                    carLights.leftFrontPointLight = pointLight;
                    break;
                case 2:
                    carLights.rightFrontLight = spotLight;
                    carLights.frontLightColor = color;
                    carLights.frontLightIntensity = intensity;
                    carLights.frontLightRange = range;
                    carLights.frontLightSpotAngle = angle;
                    carLights.rightFrontPointLight = pointLight;
                    break;
                case 3:
                    carLights.underglowLight = spotLight;
                    carLights.underglowLightColor = color;
                    carLights.underglowLightIntensity = intensity;
                    carLights.underglowLightRange = range;
                    carLights.underglowLightSpotAngle = angle;
                    break;
            }
        }
        private Light CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity = 7f, float range = 5f)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.parent = parent;
            lightObject.transform.localPosition = position;
        
            Light light = lightObject.AddComponent<Light>();
            light.type = UnityEngine.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            light.range = range;
            return light;
        }
        GameObject CreatePrefab(string prefabName)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            var instantiatedPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instantiatedPrefab.name = prefabName;

            return instantiatedPrefab;
        }
    }
}
