using System;
using UnityEngine;

namespace Silentor.CheatPanel.DevProject
{
    public class HeavyLoadCheats: MonoBehaviour, ICheats
    {
        [Range(1, 40)]
        public  float LoadMS { get; set; } = 1f;

        public float LoadMS_RO => LoadMS;

        [Range(0, 15)]
        public float Variance { get; set; } = 1f;

        public float Variance_RO => Variance;

        private void Update( )
        {
            var endTime = Time.realtimeSinceStartup + (LoadMS + UnityEngine.Random.Range( -Variance, Variance )) / 1000;
            while( Time.realtimeSinceStartup < endTime )
            {
                // Do nothing
            }
        }
    }
}