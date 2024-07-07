using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Room9
{
    public class OutlineDragDrop : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private RectTransform _current;
        private GameObject _root;
        private Vector3 _position;

        private int totalChild;

        public event Action OrderChanged;

        public void OnPointerDown(PointerEventData eventData)
        {
            _current = null;

            foreach (RectTransform child in transform)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(child, eventData.position))
                {
                    _current = child;
                    break;
                }
            }

            if (_current == null) return;

            _position = _current.position;
            _root = _current.parent.gameObject;
            totalChild = _root.transform.childCount;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_current == null) return;

            _current.position =
                new Vector3(_current.position.x, eventData.position.y, _current.position.z);

            for (int i = 0; i < totalChild; i++)
            {
                if (i != _current.GetSiblingIndex())
                {
                    Transform otherTransform = _root.transform.GetChild(i);

                    int distance = (int)Vector3.Distance(_current.position,
                        otherTransform.position);
                    if (distance <= 30)
                    {
                        Vector3 otherTransformOldPosition = otherTransform.position;
                        otherTransform.position = new Vector3(otherTransform.position.x, _position.y,
                            otherTransform.position.z);
                        _current.position = new Vector3(_current.position.x, otherTransformOldPosition.y,
                            _current.position.z);
                        _current.SetSiblingIndex(otherTransform.GetSiblingIndex());
                        _position = _current.position;

                        OrderChanged?.Invoke();
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_current == null) return;
            _current.position = _position;
        }
    }

}
