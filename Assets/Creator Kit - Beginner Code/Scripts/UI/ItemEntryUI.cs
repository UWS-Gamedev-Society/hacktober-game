using System;
using System.Collections;
using System.Collections.Generic;
using CreatorKitCode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CreatorKitCodeInternal 
{
    public class ItemEntryUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, 
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {    
        public Image IconeImage;
        public Text ItemCount;

        public int InventoryEntry { get; set; } = -1;
        public EquipmentItem EquipmentItem { get; private set; }
    
        public InventoryUI Owner { get; set; }
        public int Index { get; set; }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount % 2 == 0)
            {
                if (InventoryEntry != -1)
                {
                    if(Owner.Character.Inventory.Entries[InventoryEntry] != null)
                        Owner.ObjectDoubleClicked(Owner.Character.Inventory.Entries[InventoryEntry]);
                }
                else
                {
                    Owner.EquipmentDoubleClicked(EquipmentItem);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Owner.ObjectHoveredEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Owner.ObjectHoverExited(this);
        }

        public void UpdateEntry()
        {
            var entry = Owner.Character.Inventory.Entries[InventoryEntry];
            bool isEnabled = entry != null;
        
            gameObject.SetActive(isEnabled);
        
            if (isEnabled)
            {
                IconeImage.sprite = entry.Item.ItemSprite;

                if (entry.Count > 1)
                {
                    ItemCount.gameObject.SetActive(true);
                    ItemCount.text = entry.Count.ToString();
                }
                else
                {
                    ItemCount.gameObject.SetActive(false);
                }
            }
        }

        public void SetupEquipment(EquipmentItem itm)
        {
            EquipmentItem = itm;

            enabled = itm != null;
            IconeImage.enabled = enabled;
            if (enabled)
                IconeImage.sprite = itm.ItemSprite;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(EquipmentItem != null)
                return;
        
            Owner.CurrentlyDragged = new InventoryUI.DragData();
            Owner.CurrentlyDragged.DraggedEntry = this;
            Owner.CurrentlyDragged.OriginalParent = (RectTransform)transform.parent;
        
            transform.SetParent(Owner.DragCanvas.transform, true);
        }
    
        public void OnDrag(PointerEventData eventData)
        {
            if(EquipmentItem != null)
                return;
        
            transform.localPosition = transform.localPosition + UnscaleEventDelta(eventData.delta);
        }
    
    
        Vector3 UnscaleEventDelta(Vector3 vec)
        {
            Vector2 referenceResolution = Owner.DragCanvasScaler.referenceResolution;
            Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
 
            float widthRatio = currentResolution.x / referenceResolution.x;
            float heightRatio = currentResolution.y / referenceResolution.y;
            float ratio = Mathf.Lerp(widthRatio, heightRatio,  Owner.DragCanvasScaler.matchWidthOrHeight);
 
            return vec / ratio;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(EquipmentItem != null)
                return;
        
            Owner.HandledDroppedEntry(eventData.position);
        
            RectTransform t = transform as RectTransform;
        
            transform.SetParent(Owner.CurrentlyDragged.OriginalParent, true);

            t.offsetMax = -Vector2.one * 4;
            t.offsetMin = Vector2.one * 4;
        }
    }
}