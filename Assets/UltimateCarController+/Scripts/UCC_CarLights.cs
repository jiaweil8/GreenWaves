using UnityEngine;

namespace KairaDigitalArts
{
    [ExecuteInEditMode]
    public class UCC_CarLights : MonoBehaviour
    {
        [Header("Brake Lights")]
        public Light leftPointBrake;
        public Light rightPointBrake;
        public float brakeLightIntensity;
        public float brakeLightRange;
        public Color brakeLightColor;

        [Header("Front Lights")]
        public Light leftFrontLight;
        public Light leftFrontPointLight;
        public Light rightFrontLight;
        public Light rightFrontPointLight;
        public float frontLightIntensity;
        public float frontLightPointIntensity;
        public float frontLightPointRange;
        public float frontLightSpotAngle;
        public float frontLightRange;
        public Color frontLightColor;

        [Header("Reverse Lights")]
        public Light leftReversePointLight;
        public Light rightReversePointLight;
        public float reverseLightRange;
        public float reverseLightIntensity;
        public Color reverseLightColor;

        [Header("Interior Light")]
        public Light interiorLight;
        public float interiorLightIntensity;
        public Color interiorLightColor;

        [Header("Underglow Light")]
        public Light underglowLight;
        public float underglowLightIntensity;
        public float underglowLightSpotAngle;
        public float underglowLightRange;
        public Color underglowLightColor;

        void Update()
        {
            leftFrontPointLight.renderMode = LightRenderMode.ForcePixel;
            rightFrontPointLight.renderMode = LightRenderMode.ForcePixel;
            leftFrontLight.renderMode = LightRenderMode.ForcePixel;
            rightFrontLight.renderMode = LightRenderMode.ForcePixel;
            rightPointBrake.renderMode = LightRenderMode.ForcePixel;
            leftPointBrake.renderMode = LightRenderMode.ForcePixel;
            leftReversePointLight.renderMode = LightRenderMode.ForcePixel;
            rightReversePointLight.renderMode = LightRenderMode.ForcePixel;
            underglowLight.renderMode = LightRenderMode.ForcePixel;
            interiorLight.renderMode = LightRenderMode.ForcePixel;
            // Update brake lights
            leftPointBrake.intensity = brakeLightIntensity;
            leftPointBrake.color = brakeLightColor;
            rightPointBrake.intensity = brakeLightIntensity;
            rightPointBrake.color = brakeLightColor;

            // Update front lights
            leftFrontLight.intensity = frontLightIntensity;
            rightFrontLight.intensity = frontLightIntensity;
            leftFrontLight.spotAngle = frontLightSpotAngle;
            rightFrontLight.spotAngle = frontLightSpotAngle;
            leftFrontLight.range = frontLightRange;
            rightFrontLight.range = frontLightRange;
            leftFrontLight.color = frontLightColor;
            leftFrontPointLight.intensity = frontLightPointIntensity;
            leftFrontPointLight.color = frontLightColor;
            leftFrontPointLight.range = frontLightPointRange;
            rightFrontLight.color = frontLightColor;
            rightFrontPointLight.intensity = frontLightPointIntensity;
            rightFrontPointLight.color = frontLightColor;
            rightFrontPointLight.range = frontLightPointRange;

            // Update reverse lights
            leftReversePointLight.intensity = reverseLightIntensity;
            leftReversePointLight.color = reverseLightColor;
            leftReversePointLight.range = reverseLightRange;
            rightReversePointLight.intensity = reverseLightIntensity;
            rightReversePointLight.color = reverseLightColor;
            rightReversePointLight.range = reverseLightRange;

            // Update interior light
            interiorLight.intensity = interiorLightIntensity;
            interiorLight.color = interiorLightColor;

            // Update underglow light
            underglowLight.intensity = underglowLightIntensity;
            underglowLight.color = underglowLightColor;
            underglowLight.range = underglowLightRange;
            underglowLight.spotAngle = underglowLightSpotAngle;
        }
    }
}