using System;
using Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodingSlot : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private bool infinite;
    public bool Infinite => infinite;

    public event Action<CodingSlot> TileBeginDrag;
    public event Action<CodingSlot> TileDrag;
    public event Action<CodingSlot> TileEndDrag;
    public event Action<CodingSlot> TileClick;
    public event Action<CodingSlot, CodingSlot> TileDrop;

    [SerializeField]
    private Graphic graphic;

    [SerializeField]
    private Color defaultColor = Color.gray;

    [SerializeField]
    private Color highlightColor = Color.white; 

    public bool Interactable
    {
        set => graphic.raycastTarget = value;
    }

    public bool Visible
    {
        set
        {
            graphic.enabled = value;

            if (Tile != null)
                Tile.gameObject.SetActive(value);
        }
    }

    public bool Highlighted
    {
        set
        {
            if (Tile == null) return;

            Tile.Image.color = value ? highlightColor : defaultColor;
        }
    }

    public CodingTile Tile;

    private bool savedInteractable;
    private bool invalidClick;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Edge case: dragging a slot on itself doesn't count as a click
        invalidClick = eventData.dragging && eventData.pointerDrag.GetComponent<CodingSlot>() != null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (invalidClick)
        {
            invalidClick = false;
            return;
        }

        TileClick?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        CodingSlot slot = eventData.pointerDrag.GetComponent<CodingSlot>();

        if (slot != null) TileDrop?.Invoke(slot, this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        TileBeginDrag?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        TileDrag?.Invoke(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        TileEndDrag?.Invoke(this);
    }

    public void SaveInteractable()
        => savedInteractable = graphic.raycastTarget;

    public void RestoreInteractable()
        => graphic.raycastTarget = savedInteractable;
}
