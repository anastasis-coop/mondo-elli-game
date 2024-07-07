using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using UnityEngine.UI;

public class CodingUI : MonoBehaviour
{
    [SerializeField]
    private CodingSlot[] poolSlots;

    [SerializeField]
    private CodingSlot[] listSlots;

    [SerializeField]
    private Transform dragRoot;

    [SerializeField]
    private Graphic playButtonGraphic;

    [SerializeField]
    private Graphic clearButtonGraphic;


    [Header("Interface with original logic")]
    [SerializeField]
    private UIHandler uiHandler;
    
    public CodingSlot[] ListSlots => listSlots;

    private void Awake()
    {
        foreach (CodingSlot slot in poolSlots)
        {
            slot.TileBeginDrag += OnTileBeginDrag;
            slot.TileDrag += OnTileDrag;
            slot.TileEndDrag += OnTileEndDrag;
            slot.TileClick += OnPoolTileClick;
        }

        foreach (CodingSlot slot in listSlots)
        {
            slot.TileBeginDrag += OnTileBeginDrag;
            slot.TileDrag += OnTileDrag;
            slot.TileEndDrag += OnTileEndDrag;
            slot.TileClick += OnListTileClick;

            slot.TileDrop += OnTileDrop;
        }
    }

    private void OnTileBeginDrag(CodingSlot slot)
    {
        CodingTile tile = slot.Tile;

        if (tile == null) return;

        tile.Slot = null;

        tile.transform.SetParent(dragRoot, true);
        tile.transform.position = Input.mousePosition;
    }

    private void OnTileDrag(CodingSlot slot)
    {
        CodingTile tile = slot.Tile;

        if (tile == null) return;

        tile.transform.position = Input.mousePosition;
    }

    private void OnTileDrop(CodingSlot from, CodingSlot to)
    {
        MoveTile(from, to);
    }

    private void OnTileEndDrag(CodingSlot slot)
    {
        CodingTile tile = slot.Tile;

        // If tile wasn't moved on drop and slot is null, this means drop failed
        if (tile != null && tile.Slot == null)
        {
            if (slot.Infinite) CloneTile(tile, slot); // TODO maybe optimize by just resetting tile
            else slot.Tile = null;

            Destroy(tile.gameObject);
            if (poolSlots.Contains(slot)) return; //Doesnt change the list if it comes from the pool
        }

        CompactList();

        uiHandler.CodingListChangedHandler();
    }

    private void OnListTileClick(CodingSlot slot)
    {
        if (slot.Tile == null) return;

        Destroy(slot.Tile.gameObject);
        slot.Tile = null;

        CompactList();

        uiHandler.CodingListChangedHandler();
    }

    private void OnPoolTileClick(CodingSlot slot)
    {
        CodingSlot freeSlot = GetFreeListSlot();

        if (freeSlot != null)
        {
            MoveTile(slot, freeSlot);
        }

        uiHandler.CodingListChangedHandler();
    }

    private CodingSlot GetFreeListSlot() => Array.Find(listSlots, slot => slot.Tile == null);

    private bool ValidateMove(CodingSlot from, CodingSlot to)
    {
        if (from.Tile == null || to.Infinite) return false;

        if (from.Tile.IsMultiplierArrow)
        {
            int index = Array.IndexOf(listSlots, to);

            if (index <= 0) return false;

            CodingSlot previous = listSlots[index - 1];

            if (previous.Tile != null && previous.Tile.IsMultiplierArrow)
                return false;
        }

        return true;
    }

    private void MoveTile(CodingSlot from, CodingSlot to)
    {
        if (!ValidateMove(from, to)) return;

        CodingTile tile = from.Tile;

        // Step 1 clear original slot

        if (from.Infinite)
        {
            CloneTile(tile, from);
        }
        else if (from != to)
        {
            from.Tile = null;
        }

        // Step 2 clear destination slot

        if (to.Tile != null && to.Tile != tile)
        {
            Destroy(to.Tile.gameObject);
            to.Tile = null;
        }

        // Step 3 move logically

        to.Tile = tile;
        tile.Slot = to;

        // Step 4 move objects

        tile.transform.SetParent(to.transform);

        tile.RectTransform.anchoredPosition = Vector2.zero;
        tile.RectTransform.offsetMin = Vector2.zero;
        tile.RectTransform.offsetMax = Vector2.zero;
    }

    private void CloneTile(CodingTile tile, CodingSlot slot)
    {
        CodingTile clone = Instantiate(tile, slot.transform.position, slot.transform.rotation, slot.transform);
        clone.name = tile.name;
        clone.Slot = slot;
        slot.Tile = clone;

        clone.RectTransform.anchoredPosition = Vector2.zero;
        clone.RectTransform.offsetMin = Vector2.zero;
        clone.RectTransform.offsetMax = Vector2.zero;
    }

    private void CompactList()
    {
        int index = 0;

        foreach (CodingSlot slot in listSlots)
        {
            if (slot.Tile == null) continue;

            if (!ValidateMove(slot, listSlots[index]))
            {
                Destroy(slot.Tile.gameObject);
                slot.Tile = null;
            }
            else
            {
                MoveTile(slot, listSlots[index]);
                ++index;
            }
        }
    }

    public void PlaySequence()
    {
        uiHandler.PlaySequence();
    }

    public List<ArrowState> GetArrowList()
    {
        List<ArrowState> arrows = new List<ArrowState>();

        foreach (CodingSlot listSlot in listSlots)
        {
            CodingTile tile = listSlot.Tile;

            if (tile == null) continue;

            arrows.Add(tile.Arrow);

        }
        return arrows;
    }

    public void ClearListSlots()
    {
        foreach (CodingSlot slot in listSlots)
        {
            if (slot.Tile == null) continue;

            Destroy(slot.Tile.gameObject);

            slot.Tile = null;
        }
    }

    private void OnDestroy()
    {
        foreach (CodingSlot slot in poolSlots)
        {
            if (slot == null) continue;

            slot.TileBeginDrag -= OnTileBeginDrag;
            slot.TileDrag -= OnTileDrag;
            slot.TileEndDrag -= OnTileEndDrag;
            slot.TileClick -= OnPoolTileClick;
        }

        foreach (CodingSlot slot in listSlots)
        {
            if (slot == null) continue;

            slot.TileBeginDrag -= OnTileBeginDrag;
            slot.TileDrag -= OnTileDrag;
            slot.TileEndDrag -= OnTileEndDrag;
            slot.TileClick -= OnListTileClick;

            slot.TileDrop -= OnTileDrop;
        }
    }

    public void SetInteractable(bool interactable)
    {
        SetPoolInteractable(interactable);
        SetListInteractable(interactable);
        SetPlayButtonInteractable(interactable);
        SetClearButtonInteractable(interactable);
    }

    public void SetPoolInteractable(bool interactable)
    {
        foreach (CodingSlot slot in poolSlots)
            slot.Interactable = interactable;
    }

    public void SetListInteractable(bool interactable)
    {
        foreach (CodingSlot slot in listSlots)
            slot.Interactable = interactable;
    }

    public void SetPoolSlotInteractable(ArrowState arrow, bool interactable)
    {
        foreach (CodingSlot slot in poolSlots)
        {
            if (slot.Tile != null && slot.Tile.Arrow == arrow)
            {
                slot.Interactable = interactable;
            }
        }
    }

    public void SetPoolVisible(bool visible)
    {
        foreach (CodingSlot slot in poolSlots)
        {
            if (slot.Tile != null)
                slot.Visible = visible;
        }
    }

    public void SetPoolSlotsVisible(IEnumerable<ArrowState> arrows, bool visible)
    {
        foreach (ArrowState arrow in arrows)
            SetPoolSlotVisible(arrow, visible);
    }

    public void SetPoolSlotVisible(ArrowState arrow, bool visible)
    {
        foreach (CodingSlot slot in poolSlots)
        {
            if (slot.Tile != null && slot.Tile.Arrow == arrow)
            {
                slot.Visible = visible;
            }
        }
    }

    public void SaveInteractable()
    {
        foreach (CodingSlot slot in poolSlots)
            slot.SaveInteractable();

        foreach (CodingSlot slot in listSlots)
            slot.SaveInteractable();
    }

    public void RestoreInteractable()
    {
        foreach (CodingSlot slot in poolSlots)
            slot.RestoreInteractable();

        foreach (CodingSlot slot in listSlots)
            slot.RestoreInteractable();
    }

    public void HighlightListSlots(bool value)
    {
        foreach (var slot in listSlots)
            slot.Highlighted = value;
    }

    public void HighlightListSlot(int index, bool value)
    {
        listSlots[index].Highlighted = value;
    }

    public void SetPlayButtonInteractable(bool interactable)
    {
        playButtonGraphic.raycastTarget = interactable;
    }

    public void SetClearButtonInteractable(bool interactable)
    {
        clearButtonGraphic.raycastTarget = interactable;
    }

    public bool TryAddTileToList(ArrowState arrow)
    {
        CodingSlot from = Array.Find(poolSlots, s => s.Tile != null && s.Tile.Arrow == arrow);
        CodingSlot to = GetFreeListSlot();

        if (from == null || to == null) return false;

        bool validMove = ValidateMove(from, to);

        if (!validMove) return false;

        MoveTile(from, to);

        return true;
    }
}
