using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace CreatorKitCodeInternal 
{
    /// <summary>
    /// Control the camera, mainly used as a reference to the main camera through the singleton instance, and to handle
    /// mouse wheel zooming
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; set; }

        public Camera GameplayCamera;

        /// <summary>
        /// Angle in degree (down compared to horizon) the camera will look at when at the closest of the character
        /// </summary>
        public float MinAngle = 5.0f;
        /// <summary>
        /// Angle in degree (down compared to horizon) the camera will look at when at the farthest of the character
        /// </summary>
        public float MaxAngle = 45.0f;
        /// <summary>
        /// Distance at which the camera is from the character when at the closest zoom level
        /// </summary>
        public float MinDistance = 5.0f;
        /// <summary>
        /// Distance at which the camera is from the character when at the max zoom level
        /// </summary>
        public float MaxDistance = 45.0f;
    
        public CinemachineVirtualCamera Camera { get; protected set; }
   
        protected float m_CurrentDistance = 1.0f;
        protected CinemachineFramingTransposer m_FramingTransposer;
        
        void Awake()
        {
            Instance = this;
            Camera = GetComponent<CinemachineVirtualCamera>();
            m_FramingTransposer = Camera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

        void Start()
        {
            Zoom(0);
        }

        /// <summary>
        /// Zoom of the given distance. Note that distance need to be a param between 0...1,a d the distance is a ratio
        /// </summary>
        /// <param name="distance">The distance to zoom, need to be in range [0..1] (will be clamped) </param>
        public void Zoom(float distance)
        {
            m_CurrentDistance = Mathf.Clamp01(m_CurrentDistance + distance);

            Vector3 rotation = transform.rotation.eulerAngles;
            rotation.x = Mathf.LerpAngle(MinAngle, MaxAngle, m_CurrentDistance);
            transform.rotation = Quaternion.Euler(rotation);

            m_FramingTransposer.m_CameraDistance = Mathf.Lerp(MinDistance, MaxDistance, m_CurrentDistance);
        
            AmbiencePlayer.UpdateVolume(m_CurrentDistance);
        }
    }
}