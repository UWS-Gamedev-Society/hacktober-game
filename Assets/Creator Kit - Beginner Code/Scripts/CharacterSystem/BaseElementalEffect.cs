using System;
using System.Collections;
using System.Collections.Generic;
using CreatorKitCode;
using TMPro;
using UnityEngine;

namespace CreatorKitCode 
{
    /// <summary>
    /// The base class to derive from to write you own custom Elemental effect that can be added to a StatsSystem. There
    /// is a default implementation called ElementalEffect that can be used to make Physical/Fire/Electrical/Cold damage
    /// across time.
    ///
    /// A derived class *must* implement the Equals function so we can check if 2 effects are the same (e.g. the default
    /// implementation ElementalEffect will consider 2 effect equal if they do the same DamageType).
    /// </summary>
    public abstract class BaseElementalEffect : IEquatable<BaseElementalEffect>
    {
        public bool Done => m_Timer <= 0.0f;
        public float CurrentTime => m_Timer;
        public float Duration => m_Duration;
        
        protected float m_Duration;
        protected float m_Timer;
        protected CharacterData m_Target;

        public BaseElementalEffect(float duration)
        {
            m_Duration = duration;
        }

        public virtual void Applied(CharacterData target)
        {
            m_Timer = m_Duration;
            m_Target = target;
        }

        public virtual void Removed()
        {
        
        }

        public virtual void Update(StatSystem statSystem)
        {
            m_Timer -= Time.deltaTime;
        }

        public abstract bool Equals(BaseElementalEffect other);
    }

    /// <summary>
    /// Default implementation of the BaseElementalEffect. The constructor allows the caller to specify what type of
    /// damage is done, how much is done and the speed (time) between each instance of damage (default 1 = every second).
    /// </summary>
    public class ElementalEffect : BaseElementalEffect
    {
        int m_Damage;
        StatSystem.DamageType m_DamageType;
        float m_DamageSpeed;
        float m_SinceLastDamage = 0.0f;

        VFXManager.VFXInstance m_FireInstance;

        public ElementalEffect(float duration, StatSystem.DamageType damageType, int damage, float speed = 1.0f) :
            base(duration)
        {
            m_Damage = damage;
            m_DamageType = damageType;
            m_DamageSpeed = speed;
        }
        
        public override void Update(StatSystem statSystem)
        {
            base.Update(statSystem);

            m_SinceLastDamage += Time.deltaTime;

            if (m_SinceLastDamage > m_DamageSpeed)
            {
                m_SinceLastDamage = 0;

                Weapon.AttackData data = new Weapon.AttackData(m_Target);

                data.AddDamage(m_DamageType, m_Damage);
                
                statSystem.Damage(data);
            }
            
            //we do not parent as if the original object is destroy it would destroy the instance
            m_FireInstance.Effect.transform.position = m_Target.transform.position + Vector3.up;
        }

        public override bool Equals(BaseElementalEffect other)
        {
            ElementalEffect eff = other as ElementalEffect;

            if (other == null)
                return false;

            return eff.m_DamageType == m_DamageType;
        }

        public override void Applied(CharacterData target)
        {
            base.Applied(target);

            //We use the fire effect as it's the only one existing in the project.
            //You can add new VFX and use an if or switch statement to use the right VFXType
            //depending on this effect m_DamageType
            m_FireInstance = VFXManager.GetVFX(VFXType.FireEffect);
            m_FireInstance.Effect.transform.position = target.transform.position + Vector3.up;
        }

        public override void Removed()
        {
            base.Removed();
            
            m_FireInstance.Effect.SetActive(false);
        }
    }
}