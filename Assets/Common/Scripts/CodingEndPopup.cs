using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CodingEndPopup : MonoBehaviour
{
    [SerializeField]
    private GameObject starPrefab;

    [SerializeField]
    private GameObject slotPrefab;

    [SerializeField]
    private Transform codingSlotsRoot;

    [SerializeField]
    private Transform roomSlotsRoot;

    private List<GameObject> _stars = new();
    private List<GameObject> _slots = new();

    private Action _callback;

    public void Show(Action callback)
    {
        gameObject.SetActive(true);
        _callback = callback;

        GameState gameState = GameState.Instance;
        bool mlIsland = gameState.levelBackend.island == Island.MEDIA_LITERACY;
        bool mlEnabled = gameState.levelBackend.MediaLiteracyEnabled && gameState.levelBackend.island > Island.CONTROLLO_INTERFERENZA;

        int codingSlotCount = mlIsland ? 1 : 3;
        SpawnStars(gameState.CodingStars, codingSlotCount, codingSlotsRoot);

        int roomSlotCount = mlIsland ? 9 : mlEnabled ? 15 : 12; // 6 per normal + 3 per ml 
        SpawnStars(gameState.RoomStars, roomSlotCount, roomSlotsRoot);
    }

    public void OnContinueButtonPressed()
    {
        gameObject.SetActive(false);
        _callback?.Invoke();
    }

    private void SpawnStars(int starCount, int slotCount, Transform root)
    {
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, root);
            _slots.Add(slot);

            if (i >= starCount) continue;

            GameObject star = Instantiate(starPrefab, slot.transform);
            _stars.Add(star);

            star.transform.DOScale(1, 1).From(0).SetEase(Ease.OutBack).SetDelay(i / 3f);
        }
    }

    private void OnDisable()
    {
        foreach (GameObject slot in _slots)
            Destroy(slot);

        _slots.Clear();

        foreach (GameObject star in _stars)
            Destroy(star);

        _stars.Clear();
    }
}
