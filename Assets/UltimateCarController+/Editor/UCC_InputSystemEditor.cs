using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
namespace KairaDigitalArts { 
[CustomEditor(typeof(UCC_InputSystem))]

public class UCC_InputSystemEditor : Editor
{
        public override void OnInspectorGUI()
        {
            UCC_InputSystem inputSystem = (UCC_InputSystem)target;

            DrawDefaultInspector();
            if (inputSystem.mobileControllerEnabled)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mobile Controller Settings", EditorStyles.boldLabel);

                inputSystem.gasButton = (Button)EditorGUILayout.ObjectField("Gas Button", inputSystem.gasButton, typeof(Button), true);
                inputSystem.brakeButton = (Button)EditorGUILayout.ObjectField("Brake Button", inputSystem.brakeButton, typeof(Button), true);
                inputSystem.handbrakeButton = (Button)EditorGUILayout.ObjectField("Handbrake Button", inputSystem.handbrakeButton, typeof(Button), true);
                inputSystem.nitroButton = (Button)EditorGUILayout.ObjectField("Nitro Button", inputSystem.nitroButton, typeof(Button), true);
                inputSystem.steerleft = (Button)EditorGUILayout.ObjectField("Steer Left Button", inputSystem.steerleft, typeof(Button), true);
                inputSystem.steerRight = (Button)EditorGUILayout.ObjectField("Steer Right Button", inputSystem.steerRight, typeof(Button), true);
            }
            else
            {
                EditorGUILayout.HelpBox("Mobile Controller is disabled. Enable it to configure buttons.", MessageType.Info);
            }
            if (GUI.changed)
            {
                EditorUtility.SetDirty(inputSystem);
            }
        }
    }
}
