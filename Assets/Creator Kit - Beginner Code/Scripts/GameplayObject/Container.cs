using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreatorKitCode 
{
    
    /// <summary>
    /// Special InteractableObject that will trigger the LootSpawner on it when interacted with, and delete itself
    /// (a container can only be looted once).
    /// </summary>
    [RequireComponent(typeof(LootSpawner))]
    public class Container : InteractableObject
    {
        LootSpawner m_LootSpawner;
    
        public override bool IsInteractable => true;

        protected override void Start()
        {
            base.Start();

            m_LootSpawner = GetComponent<LootSpawner>();
        }

        public override void InteractWith(CharacterData target)
        {
            m_LootSpawner.SpawnLoot();
            Destroy(this);
        }
    }
}