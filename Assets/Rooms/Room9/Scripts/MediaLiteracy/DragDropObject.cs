using UnityEngine;

namespace Room9
{
    public class DragDropObject : MonoBehaviour
    {
        public DragDropSlot Slot { get; set; }
        public RectTransform RectTransform => (RectTransform)transform;
    }
}