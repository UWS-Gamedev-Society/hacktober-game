using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreatorKitCode 
{
    /// <summary>
    /// Handles the equipment stored inside an instance of CharacterData. Will take care of unequipping the previous
    /// item when equipping a new one in the same slot.
    /// </summary>
    public class EquipmentSystem
    {
        public Weapon Weapon { get; private set; }

        public Action<EquipmentItem> OnEquiped { get; set; }
        public Action<EquipmentItem> OnUnequip { get; set; }

        CharacterData m_Owner;
        
        EquipmentItem m_HeadSlot;
        EquipmentItem m_TorsoSlot;
        EquipmentItem m_LegsSlot;
        EquipmentItem m_FeetSlot;
        EquipmentItem m_AccessorySlot;

        Weapon m_DefaultWeapon;

        public void Init(CharacterData owner)
        {
            m_Owner = owner;
        }
        
        public void InitWeapon(Weapon wep, CharacterData data)
        {
            m_DefaultWeapon = wep;
        }
    
        public EquipmentItem GetItem(EquipmentItem.EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentItem.EquipmentSlot.Head:
                    return m_HeadSlot;
                case EquipmentItem.EquipmentSlot.Torso:
                    return m_TorsoSlot;
                case EquipmentItem.EquipmentSlot.Legs:
                    return m_LegsSlot;
                case EquipmentItem.EquipmentSlot.Feet:
                    return m_FeetSlot;
                case EquipmentItem.EquipmentSlot.Accessory:
                    return m_AccessorySlot;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Equip the given item for the given user. This won't check about requirement, this should be done by the
        /// inventory before calling equip!
        /// </summary>
        /// <param name="item">Which item to equip</param>
        public void Equip(EquipmentItem item)
        {
            Unequip(item.Slot, true);

            OnEquiped?.Invoke(item);

            switch (item.Slot)
            {
                case EquipmentItem.EquipmentSlot.Head:
                {
                    m_HeadSlot = item;
                    m_HeadSlot.EquippedBy(m_Owner);
                }
                    break;
                case EquipmentItem.EquipmentSlot.Torso:
                {
                    m_TorsoSlot = item;
                    m_TorsoSlot.EquippedBy(m_Owner);
                }
                    break;
                case EquipmentItem.EquipmentSlot.Legs:
                {
                    m_LegsSlot = item;
                    m_LegsSlot.EquippedBy(m_Owner);
                }
                    break;
                case EquipmentItem.EquipmentSlot.Feet:
                {
                    m_FeetSlot = item;
                    m_FeetSlot.EquippedBy(m_Owner);
                }
                    break;
                case EquipmentItem.EquipmentSlot.Accessory:
                {
                    m_AccessorySlot = item;
                    m_AccessorySlot.EquippedBy(m_Owner);
                }
                    break;
                //special value for weapon
                case (EquipmentItem.EquipmentSlot)666:
                    Weapon = item as Weapon;
                    Weapon.EquippedBy(m_Owner);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Unequip the item in the given slot. isReplacement is used to tell the system if this unequip was called
        /// because we equipped something new in that slot or just unequip to empty slot. This is for the weapon : the
        /// weapon slot can't be empty, so if this is not a replacement, this will auto-requip the base weapon.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="isReplacement"></param>
        public void Unequip(EquipmentItem.EquipmentSlot slot, bool isReplacement = false)
        {
            switch (slot)
            {
                case EquipmentItem.EquipmentSlot.Head:
                    if (m_HeadSlot != null)
                    {
                        m_HeadSlot.UnequippedBy(m_Owner);
                        m_Owner.Inventory.AddItem(m_HeadSlot);
                        OnUnequip?.Invoke(m_HeadSlot);
                        m_HeadSlot = null;
                    }
                    break;
                case EquipmentItem.EquipmentSlot.Torso:
                    if (m_TorsoSlot != null)
                    {
                        m_TorsoSlot.UnequippedBy(m_Owner);
                        m_Owner.Inventory.AddItem(m_TorsoSlot);
                        OnUnequip?.Invoke(m_TorsoSlot);
                        m_TorsoSlot = null;
                    }
                    break;
                case EquipmentItem.EquipmentSlot.Legs:
                    if (m_LegsSlot != null)
                    {
                        m_LegsSlot.UnequippedBy(m_Owner);
                        m_Owner.Inventory.AddItem(m_LegsSlot);
                        OnUnequip?.Invoke(m_LegsSlot);
                        m_LegsSlot = null;
                    }
                    break;
                case EquipmentItem.EquipmentSlot.Feet:
                    if (m_FeetSlot != null)
                    {
                        m_FeetSlot.UnequippedBy(m_Owner);
                        m_Owner.Inventory.AddItem(m_FeetSlot);
                        OnUnequip?.Invoke(m_FeetSlot);
                        m_FeetSlot = null;
                    }
                    break;
                case EquipmentItem.EquipmentSlot.Accessory:
                    if (m_AccessorySlot != null)
                    {
                        m_AccessorySlot.UnequippedBy(m_Owner);
                        m_Owner.Inventory.AddItem(m_AccessorySlot);
                        OnUnequip?.Invoke(m_AccessorySlot);
                        m_AccessorySlot = null;
                    }
                    break;
                case (EquipmentItem.EquipmentSlot)666:
                    if (Weapon != null && 
                        (Weapon != m_DefaultWeapon || isReplacement)) // the only way to unequip the default weapon is through replacing it
                    {
                        Weapon.UnequippedBy(m_Owner);
                    
                        //the default weapon does not go back to the inventory
                        if(Weapon != m_DefaultWeapon)
                            m_Owner.Inventory.AddItem(Weapon);
                    
                        OnUnequip?.Invoke(Weapon);
                        Weapon = null;
                    
                        //reequip the default weapon if this is not an unequip to equip a new one
                        if(!isReplacement)
                            Equip(m_DefaultWeapon);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}