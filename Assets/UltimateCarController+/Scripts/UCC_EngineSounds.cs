using System.Collections;
using UnityEngine;

namespace KairaDigitalArts
{
    public class UCC_EngineSound : MonoBehaviour
    {
        [Header("Idle Audio Settings")]
        public AudioSource idleAudioSource;
        public float idleVolume = 1.0f;

        [Header("Drive Audio Settings")]
        public AudioSource driveAudioSourceLow;
        public AudioSource driveAudioSourceHigh;
        [Range(0, 1)]
        public float driveVolumeLow = 1.0f;
        [Range(0, 5)]
        public float driveMinPitchLow = 0.5f;
        [Range(0, 5)]
        public float driveMaxPitchLow = 2.0f;
        [Range(0, 1)]
        public float driveVolumeHigh = 1.0f;
        [Range(0, 5)]
        public float driveMinPitchHigh = 0.5f;
        [Range(0, 5)]
        public float driveMaxPitchHigh = 2.0f;
        [Header("Reverse Audio Settings")]
        public AudioSource reverseAudioSource;
        [Range(0, 1)]
        public float reverseVolume = 1.0f;
        [Range(0, 5)]
        public float reverseMinPitch = 0.5f;
        [Range(0, 5)]
        public float reverseMaxPitch = 2.0f;

        public AudioSource maxRpmSound;
        public float maxRpmSoundVolume = 1.0f;

        public AudioSource suspensionAudioSource;
        public float suspensionSoundVolume = 1.0f;
        public AudioSource gearShiftAudioSource;
        public float gearShiftVolume = 1.0f;
        public AudioSource engineStartAudioSource;
        public float engineStartVolume = 1.0f;
        public AudioSource engineShutdownAudioSource;
        public float engineShutdownVolume = 1.0f;
        public AudioSource n20AudioSource;
        public float n2oVolume = 1.0f;
        public AudioSource exhaustFlameAudioSource;
        public float exhaustFlameVolume = 1.0f;
        public AudioSource turboAudioSource;
        public float turboVolume = 1.0f;
        public AudioSource turboDownShiftAudioSource;
        public float turboDownShiftVolume = 1.0f;
        float turboPitch;

        [Header("Tire Screech Audio Settings")]
        public AudioSource tireScreechAudioSource;
        [Range(0, 1)]
        public float tireScreechVolume = 1.0f;
        [Range(0, 5)]
        public float tireScreechMinPitch = 1.0f;
        [Range(0, 5)]
        public float tireScreechMaxPitch = 1.5f;

        public float volumeTransitionSpeed = 2.0f;

        private UCC_CarController carController;
        private UCC_CarSettings carSettings;
        private float targetIdleVolume;

        private int lastGear;
        private float lastSuspensionCompression;
        public bool usingN2O;

        void Start()
        {
            carController = GetComponent<UCC_CarController>();
            carSettings = GetComponent<UCC_CarSettings>();
            if (!carController || !carSettings)
            {
                Debug.LogError("CarController or CarSettings not found on " + gameObject.name);
                return;
            }
            lastGear = carController.currentGear;
            lastSuspensionCompression = carSettings.rearSuspensionLength;
            turboPitch = turboAudioSource.pitch;
        }

        void Update()
        {
            if (carController.isEngineOn)
            {
                if (!driveAudioSourceLow.isPlaying) { driveAudioSourceLow.Play(); }
                HandleDriveLowSound();
                HandleDriveHighSound();
                HandleReverseSound();
                CheckTireScreech();
                CheckSuspensionAudio(carController.frontLeftWheel);
                CheckSuspensionAudio(carController.rearLeftWheel);
                CheckSuspensionAudio(carController.frontRightWheel);
                CheckSuspensionAudio(carController.rearRightWheel);
                CheckGearShiftAudio();
                CheckN2OSound();
                n20AudioSource.volume = n2oVolume;
                suspensionAudioSource.volume = suspensionSoundVolume;
                gearShiftAudioSource.volume = gearShiftVolume;
                engineStartAudioSource.volume = engineStartVolume;
                engineShutdownAudioSource.volume = engineShutdownVolume;
                if (carController.isEngineOn)
                {
                    if (!idleAudioSource.isPlaying) 
                    {
                        idleAudioSource.volume = Mathf.Lerp(idleVolume,0,((carController.engineRPM * 2) / carSettings.maxRPM));
                    }
                }
            }
        }

        private void HandleReverseSound()
        {
            if (carController.currentGear < 0)
            {
                reverseAudioSource.volume = reverseVolume;
                reverseAudioSource.pitch = Mathf.Lerp(reverseMinPitch, reverseMaxPitch, carController.engineRPM / carSettings.maxRPM);
                if (!reverseAudioSource.isPlaying) reverseAudioSource.Play();
            }
            else
            {
                if (reverseAudioSource.isPlaying) reverseAudioSource.Stop();
            }
        }
        private void HandleDriveLowSound()
        {
                driveAudioSourceLow.volume = Mathf.Lerp(driveVolumeLow, 0, carController.engineRPM / carSettings.maxRPM);
                driveAudioSourceLow.pitch = Mathf.Lerp(driveMinPitchLow, driveMaxPitchLow, carController.engineRPM / carSettings.maxRPM);
        }
        private void HandleDriveHighSound()
        {
            if (carController.currentGear >= 0)
            {
                float highThreshold = carSettings.maxRPM * 0.1f;
                if (carController.engineRPM > highThreshold)
                {
                    driveAudioSourceHigh.volume = Mathf.Lerp(0, driveVolumeHigh, (carController.engineRPM - highThreshold) / (carSettings.maxRPM - highThreshold));
                    driveAudioSourceHigh.pitch = Mathf.Lerp(driveMinPitchHigh, driveMaxPitchHigh, carController.engineRPM / carSettings.maxRPM);
                    if (!driveAudioSourceHigh.isPlaying) driveAudioSourceHigh.Play();
                }
            }
            else
            {
                driveAudioSourceHigh.volume *= 0.5f;
            }
        }
        private void CheckTireScreech()
        {
            bool slip = carController.isSlippingForward;
            float driftOffset = carController.driftOffset;

            if (slip || driftOffset > 0.3f)
            {
                if (!tireScreechAudioSource.isPlaying)
                {
                    tireScreechAudioSource.Play();
                }

                float targetPitch = Mathf.Lerp(tireScreechMinPitch, tireScreechMaxPitch, driftOffset);
                tireScreechAudioSource.pitch = targetPitch;

                float targetVolume = Mathf.Min(Mathf.Lerp(0.5f, tireScreechVolume, driftOffset / 0.8f), 1.0f);
                tireScreechAudioSource.volume = targetVolume;
            }
            else
            {
                if (tireScreechAudioSource.isPlaying)
                {
                    tireScreechAudioSource.Stop();
                }
            }
            if(carController.currentSpeed < 10f)
            {
                tireScreechAudioSource.Stop();
            }
        }

        [SerializeField] private float suspensionThreshold = 0.5f;
        [SerializeField] private float suspensionvolumeMultiplier = 1.0f;
        private void CheckSuspensionAudio(WheelCollider wheelCollider)
        {
            WheelHit hit;
            if (wheelCollider.GetGroundHit(out hit))
            {
                float currentCompression = wheelCollider.suspensionDistance - (hit.force / wheelCollider.suspensionSpring.spring);

                float compressionDifference = Mathf.Abs(currentCompression - lastSuspensionCompression);

                if (compressionDifference > suspensionThreshold)
                {
                    if (!suspensionAudioSource.isPlaying)
                    {
                        suspensionAudioSource.Play();
                    }
                    suspensionAudioSource.volume = Mathf.Clamp(compressionDifference * suspensionvolumeMultiplier, 0, 1);
                }
                else
                {
                    suspensionAudioSource.volume = Mathf.Lerp(suspensionAudioSource.volume, 0, Time.deltaTime * 2);
                }

                lastSuspensionCompression = currentCompression;
            }
            else
            {
                if (!suspensionAudioSource.isPlaying)
                {
                    suspensionAudioSource.Play();
                }
                suspensionAudioSource.volume = Mathf.Lerp(suspensionAudioSource.volume, 1, Time.deltaTime * 2);
            }
        }
        private void CheckGearShiftAudio()
        {
            if (carController.currentGear != lastGear)
            {
                gearShiftAudioSource.Play();
            }
            if (carController.currentGear > lastGear)
            {
                turboAudioSource.volume = turboVolume;
                turboAudioSource.pitch = turboPitch;
                if (!turboAudioSource.isPlaying) turboAudioSource.Play();
            }
            else if (carController.currentGear < lastGear)
            {
                turboDownShiftAudioSource.pitch = turboPitch * 1.5f;
                turboDownShiftAudioSource.volume = turboDownShiftVolume / 2;
                if (!turboDownShiftAudioSource.isPlaying) turboDownShiftAudioSource.Play(); ;
                exhaustFlameAudioSource.volume = exhaustFlameVolume;
                exhaustFlameAudioSource.Play();
            }
            lastGear = carController.currentGear;
        }
        private void CheckN2OSound()
        {
            if (usingN2O)
            {
                if (!n20AudioSource.isPlaying)
                {
                    n20AudioSource.Play();
                }
            }
            else
            {
                n20AudioSource.Stop();
            }
        }

        public void PlayMaxRpm() 
        {
            if (!maxRpmSound.isPlaying)
            {
                maxRpmSound.Play();
            }
        }
        public void StopMaxRpm()
        {
            if (maxRpmSound.isPlaying)
            {
                maxRpmSound.Stop();
            }
        }
        public void StopAllSounds()
        {
            driveAudioSourceLow.Stop();
            driveAudioSourceHigh.Stop();
            reverseAudioSource.Stop();
            suspensionAudioSource.Stop();
            gearShiftAudioSource.Stop();
            tireScreechAudioSource.Stop();
            exhaustFlameAudioSource.Stop();
            turboDownShiftAudioSource.Stop();
            turboAudioSource.Stop();
            maxRpmSound.Stop();
        }
    }
}