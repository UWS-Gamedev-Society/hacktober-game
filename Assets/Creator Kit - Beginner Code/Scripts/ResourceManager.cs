using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreatorKitCodeInternal 
{
    /// <summary>
    /// Use this class to list common resources used by systems, so you can define them in a single place
    /// (at the time of writing, only used to store the billboard material used by the Loot system)
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        public Material BillboardMaterial => m_BillboardMaterial;
    
#pragma warning disable CS0649  
        [SerializeField]
        Material m_BillboardMaterial;
#pragma warning restore CS0649 
    
        void Awake()
        {
            Instance = this;
        }
    }
}