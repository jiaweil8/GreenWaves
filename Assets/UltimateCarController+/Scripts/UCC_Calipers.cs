using UnityEngine;

namespace KairaDigitalArts
{
    [ExecuteInEditMode]
    public enum CaliperPosition { Front, Rear }
    public class RCC_Calipers : MonoBehaviour
    {
        public CaliperPosition caliperPosition = CaliperPosition.Front;
        public GameObject steerWheelObject;

        public UCC_VisualModifications visualModifications;
        private void Start()
        {
            transform.parent = steerWheelObject.transform;
        }

        private void Update()
        {
            if (visualModifications != null)
            {
                if (Mathf.Approximately(visualModifications.frontWheelToeAngle, visualModifications.lastFrontWheelToeAngle) &&
                    Mathf.Approximately(visualModifications.frontWheelCamberAngle, visualModifications.lastFrontWheelCamberAngle) &&
                    Mathf.Approximately(visualModifications.rearWheelToeAngle, visualModifications.lastRearWheelToeAngle) &&
                    Mathf.Approximately(visualModifications.rearWheelCamberAngle, visualModifications.lastRearWheelCamberAngle))
                {
                    return;
                }
                else
                {
                    Vector3 currentRotation = transform.localEulerAngles;

                    if (caliperPosition == CaliperPosition.Front) 
                    {
                        currentRotation.x = 0;
                        currentRotation.y = visualModifications.frontWheelToeAngle;
                        currentRotation.z = visualModifications.frontWheelCamberAngle;
                    }
                    else
                    {
                        currentRotation.x = 0;
                        currentRotation.y = visualModifications.rearWheelToeAngle;
                        currentRotation.z = visualModifications.rearWheelCamberAngle;
                    }
                    transform.localEulerAngles = currentRotation;
                }
            }
        }
    }
}
