using KairaDigitalArts;
using TMPro;
using UnityEngine;

namespace KairaDigitalArts
{
    public class UCC_Gauge : MonoBehaviour
    {
       [SerializeField] public UCC_CarController carController;

        public TextMeshProUGUI speedometer;
        public TextMeshProUGUI gear;
        public Transform rpmNeedle;
        public float minNeedleRotation;
        public float maxNeedleRotation;
        public GameObject n20Indicator;
        void Update()
        {
            speedometer.text = carController.currentSpeed.ToString();
            gear.text = carController.currentGear.ToString();
        }
    }
}