using System;
using System.Collections.Generic;
using UnityEngine;

namespace Room9
{
    public class DragDrop : MonoBehaviour
    {
        [SerializeField]
        private List<DragDropSlot> slots;

        [SerializeField]
        private Transform dragRoot;

        public event Action Changed;

        private void OnEnable()
        {
            foreach (DragDropSlot slot in slots)
            {
                slot.BeginDrag += OnSlotBeginDrag;
                slot.Drag += OnSlotDrag;
                slot.Drop += OnSlotDrop;
                slot.EndDrag += OnSlotEndDrag;
                slot.Click += OnSlotClick;
            }
        }

        public void AddSlot(DragDropSlot slot)
        {
            slots.Add(slot);

            if (isActiveAndEnabled)
            {
                slot.BeginDrag += OnSlotBeginDrag;
                slot.Drag += OnSlotDrag;
                slot.Drop += OnSlotDrop;
                slot.EndDrag += OnSlotEndDrag;
                slot.Click += OnSlotClick;
            }
        }

        private void OnSlotBeginDrag(DragDropSlot slot)
        {
            DragDropObject dragged = slot.Object;

            if (dragged == null) return;

            dragged.Slot = null;

            dragged.transform.SetParent(dragRoot, true);
            dragged.transform.position = Input.mousePosition;
        }

        private void OnSlotDrag(DragDropSlot slot)
        {
            DragDropObject dragged = slot.Object;

            if (dragged == null) return;

            dragged.transform.position = Input.mousePosition;
        }

        private void OnSlotDrop(DragDropSlot from, DragDropSlot to)
        {
            MoveObject(from, to);
        }

        private void OnSlotEndDrag(DragDropSlot slot)
        {
            DragDropObject dragged = slot.Object;

            // If tile wasn't moved on drop and slot is null, this means drop failed
            if (dragged != null && dragged.Slot == null)
            {
                if (slot.Infinite || slot.ResetOnInvalidDrop)
                {
                    CloneObject(dragged, slot); // TODO maybe optimize by just resetting tile
                }
                else
                {
                    slot.Object = null;
                }

                Destroy(dragged.gameObject);
            }
        }

        //TODO change behavior here
        private void OnSlotClick(DragDropSlot slot)
        {
            foreach (DragDropSlot listSlot in slots)
            {
                if (listSlot.Object == null && listSlot.ClickDestination)
                {
                    MoveObject(slot, listSlot);
                    break;
                }
            }
        }

        private bool ValidateMove(DragDropSlot from, DragDropSlot to)
        {
            return from.Object != null && !to.Infinite;
        }

        private void MoveObject(DragDropSlot from, DragDropSlot to)
        {
            if (!ValidateMove(from, to)) return;

            DragDropObject moved = from.Object;

            // Step 1 clear original slot

            if (from.Infinite)
            {
                CloneObject(moved, from);
            }
            else if (from != to)
            {
                from.Object = null;
            }

            // Step 2 clear destination slot

            if (to.Object != null && to.Object != moved)
            {
                if (ValidateMove(to, from))
                {
                    // recursion wcgw
                    MoveObject(to, from);
                }
                else
                {
                    Destroy(to.Object.gameObject);
                    to.Object = null;
                }
            }

            // Step 3 move logically

            to.Object = moved;
            moved.Slot = to;

            // Step 4 move objects

            moved.transform.SetParent(to.transform);

            moved.RectTransform.anchoredPosition = Vector2.zero;
            moved.RectTransform.offsetMin = Vector2.zero;
            moved.RectTransform.offsetMax = Vector2.zero;

            Changed?.Invoke();
        }

        private void CloneObject(DragDropObject original, DragDropSlot slot)
        {
            DragDropObject clone = Instantiate(original, slot.transform.position, slot.transform.rotation, slot.transform);
            clone.name = original.name;
            clone.Slot = slot;
            slot.Object = clone;

            clone.RectTransform.anchoredPosition = Vector2.zero;
            clone.RectTransform.offsetMin = Vector2.zero;
            clone.RectTransform.offsetMax = Vector2.zero;
        }

        public List<DragDropObject> GetObjectList()
        {
            List<DragDropObject> objectList = new();

            foreach (DragDropSlot listSlot in slots)
            {
                DragDropObject dragDropObject = listSlot.Object;

                if (dragDropObject == null) continue;

                objectList.Add(dragDropObject);

            }
            return objectList;
        }

        private void OnDisable()
        {
            foreach (DragDropSlot slot in slots)
            {
                if (slot == null) continue;

                slot.BeginDrag -= OnSlotBeginDrag;
                slot.Drag -= OnSlotDrag;
                slot.Drop -= OnSlotDrop;
                slot.EndDrag -= OnSlotEndDrag;
                slot.Click -= OnSlotClick;
            }
        }

        public void SetInteractable(bool interactable)
        {
            foreach (DragDropSlot slot in slots)
                slot.Interactable = interactable;
        }

        public void SetPoolVisible(bool visible)
        {
            foreach (DragDropSlot slot in slots)
            {
                if (slot.Object != null)
                    slot.Visible = visible;
            }
        }
    }
}