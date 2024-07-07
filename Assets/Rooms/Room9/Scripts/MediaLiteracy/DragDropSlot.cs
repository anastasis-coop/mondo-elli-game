using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Room9
{
    public class DragDropSlot : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private bool infinite;
        public bool Infinite => infinite;

        [SerializeField]
        private bool resetOnInvalidDrop;
        public bool ResetOnInvalidDrop => resetOnInvalidDrop;

        [SerializeField]
        private bool clickDestination;
        public bool ClickDestination => clickDestination;

        public event Action<DragDropSlot> BeginDrag;
        public event Action<DragDropSlot> Drag;
        public event Action<DragDropSlot> EndDrag;
        public event Action<DragDropSlot> Click;
        public event Action<DragDropSlot, DragDropSlot> Drop;

        [SerializeField]
        private Graphic graphic;

        public bool Interactable
        {
            set => graphic.raycastTarget = value;
        }

        public bool Visible
        {
            set
            {
                graphic.enabled = value;

                if (Object != null)
                    Object.gameObject.SetActive(value);
            }
        }

        public DragDropObject Object;

        private bool invalidClick;

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Edge case: dragging a slot on itself doesn't count as a click
            invalidClick = eventData.dragging && eventData.pointerDrag.GetComponent<DragDropSlot>() != null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (invalidClick)
            {
                invalidClick = false;
                return;
            }

            Click?.Invoke(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            DragDropSlot slot = eventData.pointerDrag.GetComponent<DragDropSlot>();

            if (slot != null) Drop?.Invoke(slot, this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Drag?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDrag?.Invoke(this);
        }
    }
}