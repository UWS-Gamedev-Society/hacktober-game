using UnityEngine;

namespace CreatorKitCode 
{
    /// <summary>
    /// This handles the inventory of our character. The inventory has a maximum of 32 slot, each slot can hold one
    /// TYPE of object, but those can be stacked without limit (e.g. 1 slot used by health potions, but contains 20
    /// health potions)
    /// </summary>
    public class InventorySystem
    {
        /// <summary>
        /// One entry in the inventory. Hold the type of Item and how many there is in that slot.
        /// </summary>
        public class InventoryEntry
        {
            public int Count;
            public Item Item;
        }

        //Only 32 slots in inventory
        public InventoryEntry[] Entries = new InventoryEntry[32];

        CharacterData m_Owner;
        
        public void Init(CharacterData owner)
        {
            m_Owner = owner;
        }
        
        /// <summary>
        /// Add an item to the inventory. This will look if this item already exist in one of the slot and increment the
        /// stack counter there instead of using another slot.
        /// </summary>
        /// <param name="item">The item to add to the inventory</param>
        public void AddItem(Item item)
        {
            bool found = false;
            int firstEmpty = -1;
            for (int i = 0; i < 32; ++i)
            {
                if (Entries[i] == null)
                {
                    if (firstEmpty == -1)
                        firstEmpty = i;
                }
                else if (Entries[i].Item == item)
                {
                    Entries[i].Count += 1;
                    found = true;
                }
            }

            if (!found && firstEmpty != -1)
            {
                InventoryEntry entry = new InventoryEntry();
                entry.Item = item;
                entry.Count = 1;

                Entries[firstEmpty] = entry;
            }
        }

        /// <summary>
        /// This will *try* to use the item. If the item return true when used, this will decrement the stack count and
        /// if the stack count reach 0 this will free the slot. If it return false, it will just ignore that call.
        /// (e.g. a potion will return false if the user is at full health, not consuming the potion in that case)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool UseItem(InventoryEntry item)
        {
            //true mean it get consumed and so would be removed from inventory.
            //(note "consumed" is a loose sense here, e.g. armor get consumed to be removed from inventory and added to
            //equipement by their subclass, and de-equiping will re-add the equipement to the inventory 
            if (item.Item.UsedBy(m_Owner))
            {
                item.Count -= 1;

                if (item.Count <= 0)
                {
                    //maybe store the index in the InventoryEntry to avoid having to find it again here
                    for (int i = 0; i < 32; ++i)
                    {
                        if (Entries[i] == item)
                        {
                            Entries[i] = null;
                            break;
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}