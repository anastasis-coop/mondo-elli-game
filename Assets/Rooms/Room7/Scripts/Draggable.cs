using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Room7
{

  public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
  {

    public bool canReposition = false;
    public List<DropRectangle> containers;

    private DropRectangle currentContainer = null;
    private Vector2 startPosition;
    private bool isDragging = false;
    private int lastSlotIndex;

    public void OnBeginDrag(PointerEventData eventData) {
      if (currentContainer == null || canReposition) {
        isDragging = true;
        startPosition = (transform as RectTransform).anchoredPosition;
        transform.SetAsLastSibling();
        SetDraggedPosition(eventData);
        if (currentContainer != null) {
          lastSlotIndex = currentContainer.removeObject(this);
        }
      }
    }

    public void OnDrag(PointerEventData data) {
      if (isDragging) {
        SetDraggedPosition(data);
      }
    }

    private void SetDraggedPosition(PointerEventData data) {
      Vector3 globalMousePos;
      if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, data.position, data.pressEventCamera, out globalMousePos)) {
        (transform as RectTransform).position = globalMousePos;
      }
    }

    public void OnEndDrag(PointerEventData data) {
      if (isDragging) {
        isDragging = false;
        bool dropped = false;
        if (containers != null) {
          DropRectangle droppingBox = containers.Find((DropRectangle box) => box.canAccept(this));
          if (droppingBox != null) {
            if (droppingBox.addObject(this)) {
              currentContainer = droppingBox;
              dropped = true;
            }
          }
        }
        if (!dropped) {
          (transform as RectTransform).anchoredPosition = startPosition;
          if (currentContainer != null && lastSlotIndex >= 0) {
            currentContainer.RestoreObject(this, lastSlotIndex);
          }
        }
      }
    }

  }

}