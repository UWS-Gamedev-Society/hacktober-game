using System;
using CreatorKitCodeInternal;
using UnityEngine;

using Random = UnityEngine.Random;

namespace CreatorKitCode 
{
    /// <summary>
    /// This defines a character in the game. The name Character is used in a loose sense, it just means something that
    /// can be attacked and have some stats including health. It could also be an inanimate object like a breakable box.
    /// </summary>
    public class CharacterData : HighlightableObject
    {
        public string CharacterName;

        public StatSystem Stats;
        /// <summary>
        /// The starting weapon equipped when the Character is created. Set through the Unity Editor.
        /// </summary>
        public Weapon StartingWeapon;
        public InventorySystem Inventory = new InventorySystem();
        public EquipmentSystem Equipment = new EquipmentSystem();

        public AudioClip[] HitClip;
    
        /// <summary>
        /// Callback for when that CharacterData receive damage. E.g. used by the player character to trigger the right
        /// animation
        /// </summary>
        public Action OnDamage { get; set; }

        /// <summary>
        /// Will return true if the attack cooldown have reached 0. False otherwise.
        /// </summary>
        public bool CanAttack
        {
            get { return m_AttackCoolDown <= 0.0f; }
        }

        float m_AttackCoolDown;

        public void Init()
        {
            Stats.Init(this);
            Inventory.Init(this);
            Equipment.Init(this);

            if (StartingWeapon != null)
            {
                StartingWeapon.UsedBy(this);
                Equipment.InitWeapon(StartingWeapon, this);
            }
        }

        void Awake()
        {
            Animator anim = GetComponentInChildren<Animator>();
            if(anim != null)
                SceneLinkedSMB<CharacterData>.Initialise(anim, this);
        }

        // Update is called once per frame
        void Update()
        {
            Stats.Tick();

            if (m_AttackCoolDown > 0.0f)
                m_AttackCoolDown -= Time.deltaTime;
        }

        /// <summary>
        /// Will check if that CharacterData can reach the given target with its currently equipped weapon. Will rarely
        /// be called, as the function CanAttackTarget will call this AND also check if the cooldown is finished.
        /// </summary>
        /// <param name="target">The CharacterData you want to reach</param>
        /// <returns>True if you can reach the target, False otherwise</returns>
        public bool CanAttackReach(CharacterData target)
        {
            return Equipment.Weapon.CanHit(this, target);
        }

        /// <summary>
        /// Will check if the target is attackable. This in effect check :
        /// - If the target is in range of the weapon
        /// - If this character attack cooldown is finished
        /// - If the target isn't already dead
        /// </summary>
        /// <param name="target">The CharacterData you want to reach</param>
        /// <returns>True if the target can be attacked, false if any of the condition isn't met</returns>
        public bool CanAttackTarget(CharacterData target)
        {
            if (target.Stats.CurrentHealth == 0)
                return false;
        
            if (!CanAttackReach(target))
                return false;

            if (m_AttackCoolDown > 0.0f)
                return false;

            return true;
        }

        /// <summary>
        /// Call when the character die (health reach 0).
        /// </summary>
        public void Death()
        {
            Stats.Death();
        }

        /// <summary>
        /// Attack the given target. NOTE : this WON'T check if the target CAN be attacked, you should make sure before
        /// with the CanAttackTarget function.
        /// </summary>
        /// <param name="target">The CharacterData you want to attack</param>
        public void Attack(CharacterData target)
        {
            Equipment.Weapon.Attack(this, target);
        }

        /// <summary>
        /// This need to be called as soon as an attack is triggered, it will start the cooldown. This is separate from
        /// the actual Attack function as AttackTriggered will be called at the beginning of the animation while the
        /// Attack function (doing the actual attack and damage) will be called by an animation event to match the animation
        /// </summary>
        public void AttackTriggered()
        {
            //Agility reduce by 0.5% the cooldown to attack (e.g. if agility = 50, 25% faster to attack)
            m_AttackCoolDown = Equipment.Weapon.Stats.Speed - (Stats.stats.agility * 0.5f * 0.001f * Equipment.Weapon.Stats.Speed);
        }

        /// <summary>
        /// Damage the Character by the AttackData given as parameter. See the documentation for that class for how to
        /// add damage to that attackData. (this will be done automatically by weapons, but you may need to fill it
        /// manually when writing special elemental effect)
        /// </summary>
        /// <param name="attackData"></param>
        public void Damage(Weapon.AttackData attackData)
        {
            if (HitClip.Length != 0)
            {
                SFXManager.PlaySound(SFXManager.Use.Player, new SFXManager.PlayData()
                {
                    Clip = HitClip[Random.Range(0, HitClip.Length)],
                    PitchMax =  1.1f,
                    PitchMin =  0.8f,
                    Position = transform.position
                });
            }
        
            Stats.Damage(attackData);
            
            OnDamage?.Invoke();
        }
    }
}