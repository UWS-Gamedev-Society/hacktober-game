using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreatorKitCode 
{
    /// <summary>
    /// Allows you to define a list of VFX prefabs each with a name.
    /// An editor script takes care of generating a C# file containing an enum with the given name to index the Entries
    /// array. This allows you to call methods such as VFXManager.GetVFX(VFXType.MyEffectName). See the VFXManager class
    /// for more details.
    /// </summary>
    public class VFXDatabase : ScriptableObject
    {
        /// <summary>
        /// An entry in the VFXDatabase, storing all the data needed to create the pools of instances of VFX.
        /// </summary>
        [System.Serializable]
        public class VFXDBEntry
        {
            public string Name;
            public GameObject Prefab;
            public int PoolSize = 6;
        }

        public VFXDBEntry[] Entries;
    }
}