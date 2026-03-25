using KairaDigitalArts;
using UnityEngine;
using System.Collections.Generic;

namespace KairaDigitalArts
{
    public class UCC_ExtraCarOptions : MonoBehaviour
    {
        public UCC_CarController carController;
        public List<ParticleSystem> carExhaustIdle = new List<ParticleSystem>();
        public List<ParticleSystem> n2o = new List<ParticleSystem>();
        public List<ParticleSystem> exhaustFlames = new List<ParticleSystem>();

        private int lastGear;

        void Start()
        {
            lastGear = carController.currentGear;
        }

        void Update()
        {
            HandleCarExhaustIdle();
            HandleExhaustFlames();
            CheckN2O();
        }

        void HandleCarExhaustIdle()
        {
            bool shouldPlay = Mathf.Abs(carController.currentSpeed) < 10;

            foreach (ParticleSystem exhaust in carExhaustIdle)
            {
                if (exhaust == null) continue;

                if (exhaust.isPlaying && !shouldPlay)
                {
                    exhaust.Stop();
                }
                else if (!exhaust.isPlaying && shouldPlay)
                {
                    exhaust.Play();
                }
            }
        }

        void HandleExhaustFlames()
        {
            int currentGear = carController.currentGear;

            if (currentGear < lastGear)
            {
                foreach (ParticleSystem flame in exhaustFlames)
                {
                    if (flame == null) continue;

                    if (!flame.isPlaying)
                    {
                        flame.Play();
                    }
                }
            }

            lastGear = currentGear;
        }

        void CheckN2O()
        {
            foreach (ParticleSystem nitro in n2o)
            {
                if (nitro == null) continue;

                if (carController.usingN2o)
                {
                    if (!nitro.isPlaying)
                    {
                        nitro.Play();
                    }
                }
                else
                {
                    if (nitro.isPlaying)
                    {
                        nitro.Stop();
                    }
                }
            }
        }
    }
}
