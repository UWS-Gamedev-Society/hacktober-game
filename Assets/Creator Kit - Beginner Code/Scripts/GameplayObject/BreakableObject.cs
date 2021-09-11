using System.Collections;
using System.Collections.Generic;
using CreatorKitCode;
using UnityEngine;

namespace CreatorKitCodeInternal 
{
    /// <summary>
    /// Small class that will handle replacing a GameObject with another when the CharacterData on the same GameObject
    /// reach health = 0. Used in game for barrels, swapping a normal model for a prefab made of multiple part with
    /// physics to simulate the barrel breaking.
    /// </summary>
    [RequireComponent(typeof(CharacterData))]
    public class BreakableObject : MonoBehaviour
    {
        public GameObject DestroyedChild;
        public AudioClip BreakingAudioClip;
    
    
        CharacterData m_CharacterData;
        LootSpawner m_LootSpawner;
    
        // Start is called before the first frame update
        void Start()
        {
            m_CharacterData = GetComponent<CharacterData>();
            m_CharacterData.Init();

            m_LootSpawner = GetComponent<LootSpawner>();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_CharacterData.Stats.CurrentHealth == 0)
            {
                m_LootSpawner.SpawnLoot();
            
                DestroyedChild.transform.SetParent(null);
                DestroyedChild.gameObject.SetActive(true);
            
                SFXManager.PlaySound(SFXManager.Use.WorldSound, new SFXManager.PlayData() { Clip = BreakingAudioClip });
            
                Destroy(gameObject);
            }
        }
    }
}