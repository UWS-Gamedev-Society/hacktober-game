using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreatorKitCodeInternal 
{
    public class AmbiencePlayer : MonoBehaviour
    {
        static AmbiencePlayer s_Instance;
    
        public AudioSource FarAudioSource;
        public AudioSource CloseAudioSource;

        void Awake()
        {
            s_Instance = this;
        }

        public static void UpdateVolume(float zoomRatio)
        {
            s_Instance.CloseAudioSource.volume = 1.0f - zoomRatio;
            s_Instance.FarAudioSource.volume = zoomRatio;
        }
    }
}