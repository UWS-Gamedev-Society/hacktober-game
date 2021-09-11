using System;
using System.Collections.Generic;
using CreatorKitCode;
using CreatorKitCodeInternal;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreatorKitCode 
{
    /// <summary>
    /// Handles the stats of a CharacterData. It stores the health and strength/agility/defense stats.
    /// This class contains various functions for interacting with stats, by adding stat modifications, elemental
    /// effects or damage.
    /// </summary>
    [System.Serializable]
    public class StatSystem
    {
        /// <summary>
        /// The type of damage that exist, each will have their own boost/protection in stats, only the main, Physical,
        /// is influence by strength and defense stats.
        /// </summary>
        public enum DamageType
        {
            Physical,
            Fire,
            Cold,
            Electric
            //ADD YOUR CUSTOM TYPE AFTER
        }
    
        /// <summary>
        /// Store the stats, which are composed of 4 values : health, strength, agility and defense.
        /// It also contains elemental protections and boost (1 for each elements defined by the DamageType enum)
        /// </summary>
        [System.Serializable]
        public class Stats
        {
            //Integer for simplicity, may switch to float later on. For now everything is integer
            public int health;
            public int strength;
            public int defense;
            public int agility;

            //use an array indexed by the DamageType enum for easy extensibility
            public int[] elementalProtection = new int[Enum.GetValues(typeof(DamageType)).Length];   
            public int[] elementalBoosts = new int[Enum.GetValues(typeof(DamageType)).Length];
        
            public void Copy(Stats other)
            {
                health = other.health;
                strength = other.strength;
                defense = other.defense;
                agility = other.agility;
            
                Array.Copy(other.elementalProtection, elementalProtection, other.elementalProtection.Length);
                Array.Copy(other.elementalBoosts, elementalBoosts, other.elementalBoosts.Length);
            }

            /// <summary>
            /// Will modify that Stat by the given StatModifier (see StatModifier documentation for how to use them)
            /// </summary>
            /// <param name="modifier"></param>
            public void Modify(StatModifier modifier)
            {
                //bit convoluted, but allow to reuse the normal int stat system for percentage change
                if (modifier.ModifierMode == StatModifier.Mode.Percentage)
                {
                    health += Mathf.FloorToInt(health * (modifier.Stats.health / 100.0f));
                    strength += Mathf.FloorToInt(strength * (modifier.Stats.strength / 100.0f));
                    defense += Mathf.FloorToInt(defense * (modifier.Stats.defense / 100.0f));
                    agility += Mathf.FloorToInt(agility * (modifier.Stats.agility / 100.0f));
                
                    for(int i = 0; i < elementalProtection.Length; ++i)
                        elementalProtection[i] += Mathf.FloorToInt(elementalProtection[i] * (modifier.Stats.elementalProtection[i] / 100.0f));
                
                    for(int i = 0; i < elementalBoosts.Length; ++i)
                        elementalBoosts[i] += Mathf.FloorToInt(elementalBoosts[i] * (modifier.Stats.elementalBoosts[i]/100.0f));
                }
                else
                {
                    health += modifier.Stats.health;
                    strength += modifier.Stats.strength;
                    defense += modifier.Stats.defense;
                    agility += modifier.Stats.agility;
                
                    for(int i = 0; i < elementalProtection.Length; ++i)
                        elementalProtection[i] += modifier.Stats.elementalProtection[i];
                
                    for(int i = 0; i < elementalBoosts.Length; ++i)
                        elementalBoosts[i] += modifier.Stats.elementalBoosts[i];
                }
            }
        }
    
        /// <summary>
        /// Can be added to a stack of modifiers on the StatSystem to modify the value of the base stats
        /// e.g. a weapon adding +2 strength will push a modifier on the top of the stack.
        ///
        /// They have 2 modes : Absolute, where values are added as is, and Percentage, where values are converted to
        /// percentage (e.g. a value of 50 in strength in a Percentage modifier will increase the strength by 50%).
        /// </summary>
        [System.Serializable]
        public class StatModifier
        {
            /// <summary>
            /// The mode of the modifier : Percentage will divide the value by 100 to get a percentage, absolute use the
            /// value as is.
            /// </summary>
            public enum Mode
            {
                Percentage,
                Absolute
            }

            public Mode ModifierMode = Mode.Absolute;
            public Stats Stats = new Stats();
        }
        
        /// <summary>
        /// This is a special StatModifier, that gets added to the TimedStatModifier stack, that will be automatically
        /// removed when its timer reaches 0. Contains a StatModifier that controls the actual modification.
        /// </summary>
        [System.Serializable]
        public class TimedStatModifier
        {
            public string Id;
            public StatModifier Modifier;

            public Sprite EffectSprite;
        
            public float Duration;
            public float Timer;

            public void Reset()
            {
                Timer = Duration;
            }
        }
    
        public Stats baseStats;
        public Stats stats { get; set; } = new Stats();
    

        public int CurrentHealth { get; private set; }
        public List<BaseElementalEffect> ElementalEffects => m_ElementalEffects;
        public List<TimedStatModifier> TimedModifierStack => m_TimedModifierStack;

        CharacterData m_Owner;
    
        List<StatModifier> m_ModifiersStack = new List<StatModifier>();
        List<TimedStatModifier> m_TimedModifierStack = new List<TimedStatModifier>();
        List<BaseElementalEffect> m_ElementalEffects = new List<BaseElementalEffect>();
    
        public void Init(CharacterData owner)
        {
            stats.Copy(baseStats);
            CurrentHealth = stats.health;
            m_Owner = owner;
        }
    
        /// <summary>
        /// Add a modifier to the end of the stack. This will recompute the Stats so it now include the new modifier.
        /// </summary>
        /// <param name="modifier"></param>
        public void AddModifier(StatModifier modifier)
        {
            m_ModifiersStack.Add(modifier);    
            UpdateFinalStats();
        }

        /// <summary>
        /// Remove a modifier from the stack. This modifier need to already be on the stack. e.g. used by the equipment
        /// effect that store the modifier they add on equip and remove it when unequipped.
        /// </summary>
        /// <param name="modifier"></param>
        public void RemoveModifier(StatModifier modifier)
        {
            m_ModifiersStack.Remove(modifier);
            UpdateFinalStats();
        }

        /// <summary>
        /// Add a Timed modifier. Timed modifier does not stack and instead re-adding the same type of modifier will just
        /// reset the already existing one timer to the given duration. That the use of the id parameter : it need to be
        /// shared by all timed effect that are the "same type". i.e. an effect that add strength can use "StrengthTimed"
        /// as id, so if 2 object try to add that effect, they won't stack but instead just refresh the timer.
        /// </summary>
        /// <param name="modifier">A StatModifier container the wanted modification</param>
        /// <param name="duration">The time during which that modification will be active.</param>
        /// <param name="id">A name that identify that type of modification. Adding a timed modification with an id that already exist reset the timer instead of adding a new one to the stack</param>
        /// <param name="sprite">The sprite used to display the time modification above the player UI</param>
        public void AddTimedModifier(StatModifier modifier, float duration, string id, Sprite sprite)
        {
            bool found = false;
            int index = m_TimedModifierStack.Count;
            for (int i = 0; i < m_TimedModifierStack.Count; ++i)
            {
                if (m_TimedModifierStack[i].Id == id)
                {
                    found = true;
                    index = i;
                }
            }

            if (!found)
            {
                m_TimedModifierStack.Add(new TimedStatModifier(){ Id = id});
            }

            m_TimedModifierStack[index].EffectSprite = sprite;
            m_TimedModifierStack[index].Duration = duration;
            m_TimedModifierStack[index].Modifier = modifier;
            m_TimedModifierStack[index].Reset();
        
            UpdateFinalStats();
        }

        /// <summary>
        /// Add an elemental effect to the StatSystem. Elemental Effect does not stack, adding the same type (the Equals
        /// return true) will instead replace the old one with the new one.
        /// </summary>
        /// <param name="effect"></param>
        public void AddElementalEffect(BaseElementalEffect effect)
        {
            effect.Applied(m_Owner);
        
            bool replaced = false;
            for (int i = 0; i < m_ElementalEffects.Count; ++i)
            {
                if (effect.Equals(m_ElementalEffects[i]))
                {
                    replaced = true;
                    m_ElementalEffects[i].Removed();
                    m_ElementalEffects[i] = effect;
                }
            }
        
            if(!replaced)
                m_ElementalEffects.Add(effect);
        }

        public void Death()
        {
            foreach(var e in ElementalEffects)
                e.Removed();
        
            ElementalEffects.Clear();
            TimedModifierStack.Clear();
        
            UpdateFinalStats();
        }

        public void Tick()
        {
            bool needUpdate = false;
        
            for (int i = 0; i < m_TimedModifierStack.Count; ++i)
            {
                //permanent modifier will have a timer == -1.0f, so jump over them
                if (m_TimedModifierStack[i].Timer > 0.0f)
                {
                    m_TimedModifierStack[i].Timer -= Time.deltaTime;
                    if (m_TimedModifierStack[i].Timer <= 0.0f)
                    {//modifier finished, so we remove it from the stack
                        m_TimedModifierStack.RemoveAt(i);
                        i--;
                        needUpdate = true;
                    }
                }
            }
        
            if(needUpdate)
                UpdateFinalStats();

            for (int i = 0; i < m_ElementalEffects.Count; ++i)
            {
                var effect = m_ElementalEffects[i];
                effect.Update(this);
            
                if (effect.Done)
                {
                    m_ElementalEffects[i].Removed();
                    m_ElementalEffects.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Change the health by the given amount : negative amount damage, positive amount heal. The function will
        /// take care of clamping the value in the range [0...MaxHealth]
        /// </summary>
        /// <param name="amount"></param>
        public void ChangeHealth(int amount)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, stats.health);
        }

        void UpdateFinalStats()
        {
            bool maxHealthChange = false;
            int previousHealth = stats.health;
        
            stats.Copy(baseStats);

            foreach (var modifier in m_ModifiersStack)
            {
                if (modifier.Stats.health != 0)
                    maxHealthChange = true;
            
                stats.Modify(modifier);
            }

            foreach (var timedModifier in m_TimedModifierStack)
            {
                if (timedModifier.Modifier.Stats.health != 0)
                    maxHealthChange = true;
            
                stats.Modify(timedModifier.Modifier);
            }

            //if we change the max health we update the current health to it's new value
            if (maxHealthChange)
            {
                float percentage = CurrentHealth / (float)previousHealth;
                CurrentHealth = Mathf.RoundToInt(percentage * stats.health );
            }
        }

        /// <summary>
        /// Will damage (change negatively health) of the amount of damage stored in the attackData. If the damage are
        /// negative, this heal instead.
        ///
        /// This will also notify the DamageUI so a damage number is displayed.
        /// </summary>
        /// <param name="attackData"></param>
        public void Damage(Weapon.AttackData attackData)
        {
            int totalDamage = attackData.GetFullDamage();
        
            ChangeHealth(-totalDamage);
            DamageUI.Instance.NewDamage(totalDamage, m_Owner.transform.position);
        }
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(StatSystem.Stats))]
public class StatsDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int enumTypesCount = Enum.GetValues(typeof(StatSystem.DamageType)).Length;
        int lineCount = enumTypesCount + 7;
        float extraHeight = 6f;
        float propertyHeight = lineCount * EditorGUIUtility.singleLineHeight + extraHeight;

        return propertyHeight;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;

        var currentRect = position;
        currentRect.height = EditorGUIUtility.singleLineHeight;
        
        EditorGUI.DropShadowLabel(currentRect, property.displayName);
        
        currentRect.y += EditorGUIUtility.singleLineHeight + 6f;
        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative(nameof(StatSystem.Stats.health)));
        
        currentRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative(nameof(StatSystem.Stats.strength)));
        
        currentRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative(nameof(StatSystem.Stats.defense)));
        
        currentRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative(nameof(StatSystem.Stats.agility)));

        currentRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(currentRect, "Elemental Protection/Boost", style);
        
        currentRect.y += EditorGUIUtility.singleLineHeight;
        currentRect.width *= 0.3f;

        currentRect.x += currentRect.width;
        EditorGUI.LabelField(currentRect, "Protection (%)", style);
        currentRect.x += currentRect.width;
        EditorGUI.LabelField(currentRect, "Boost (%)", style);

        var names = Enum.GetNames(typeof(StatSystem.DamageType));

        var elementalProtectionProp = property.FindPropertyRelative(nameof(StatSystem.Stats.elementalProtection));
        var elementalBoostProp = property.FindPropertyRelative(nameof(StatSystem.Stats.elementalBoosts));
        
        for (int i = 0; i < names.Length; ++i)
        {
            currentRect.x -= currentRect.width * 2;
            currentRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(currentRect, names[i]);
            
            currentRect.x += currentRect.width;
            EditorGUI.PropertyField(currentRect, elementalProtectionProp.GetArrayElementAtIndex(i), GUIContent.none);
            
            currentRect.x += currentRect.width;
            EditorGUI.PropertyField(currentRect, elementalBoostProp.GetArrayElementAtIndex(i), GUIContent.none);
        }
        
        EditorGUI.EndProperty();
    }
}
#endif