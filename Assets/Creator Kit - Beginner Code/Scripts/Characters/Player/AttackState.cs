using System.Collections;
using System.Collections.Generic;
using CreatorKitCode;
using UnityEngine;

namespace CreatorKitCodeInternal {
    public class AttackState : SceneLinkedSMB<CharacterData>
    {
        CharacterAudio m_Audio;
    
        public override void OnStart(Animator animator)
        {
            m_Audio = m_MonoBehaviour.GetComponentInChildren<CharacterAudio>();
        }

        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_Audio != null)
            {
                Vector3 position = m_MonoBehaviour.transform.position;

                m_Audio.Attack(position);
                SFXManager.PlaySound(m_Audio.UseType, new SFXManager.PlayData()
                {
                    Clip = m_MonoBehaviour.Equipment.Weapon.GetSwingSound(),
                    Position = position
                });
            }
        }
    }
}