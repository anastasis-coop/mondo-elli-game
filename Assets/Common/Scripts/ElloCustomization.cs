using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElloCustomization : MonoBehaviour
{
    [SerializeField]
    private Renderer ello;

    [Header("Hair")]

    [SerializeField]
    private Color defaultHairColor;

    [System.Serializable]
    private class HairColor
    {
        [SerializeField]
        private Island area;
        public Island Area => area;

        // is there a nicer way to set color in material without editing the shared asset?
        [SerializeField]
        private int materialIndex;
        public int MaterialIndex => materialIndex;

        [SerializeField]
        private Color color;
        public Color Color => color;
    }

    [SerializeField]
    private HairColor[] hairColors;

    [SerializeField]
    private string hairColorProperty;

    [SerializeField]
    private Color defaultFeetColor;

    [SerializeField]
    private int feetMaterialIndex;

    [SerializeField]
    private string feetColorProperty;

    [Serializable]
    private class Accessory
    {
        public GameObject[] gameObjects;
        public Island island;

        public bool Unlocked { get; set; }
    }

    [Header("Accessories")]

    [SerializeField]
    private Accessory[] slot1Accessories;

    [SerializeField]
    private Accessory[] slot2Accessories;

    [SerializeField]
    private Accessory[] slot3Accessories;

    private int _slot1Index = 0;
    private int _slot2Index = 0;
    private int _slot3Index = 0;

    public int Slot1Index => _slot1Index;
    public int Slot2Index => _slot2Index;
    public int Slot3Index => _slot3Index;

    private void Awake()
    {
        // Instantiate materials for Ello Hair
        foreach (HairColor hairColor in hairColors)
        {
            ello.materials[hairColor.MaterialIndex]
                .SetColor(hairColorProperty, defaultHairColor);
        }
    }

    public void ToggleHairColors(IEnumerable<Island> areas, bool value)
    {
        foreach (Island area in areas)
            ToggleHairColor(area, value);
    }

    public void ToggleHairColor(Island area, bool value)
    {
        foreach (HairColor hairColor in hairColors)
        {
            if (area == hairColor.Area)
            {
                Color color = value ? hairColor.Color : defaultHairColor;
                ello.materials[hairColor.MaterialIndex].SetColor(hairColorProperty, color);
                break;
            }
        }
    }

    public void SetAccessories(int slot1, int slot2, int slot3)
    {
        SetSlotAccessory(slot1Accessories, ref _slot1Index, slot1);
        SetSlotAccessory(slot2Accessories, ref _slot2Index, slot2);
        SetSlotAccessory(slot3Accessories, ref _slot3Index, slot3);

        UpdateFeetColor();
    }

    private void SetSlotAccessory(Accessory[] slotAccessories, ref int indexField, int index)
    {
        indexField = index;

        for (int i = 0; i < slotAccessories.Length; i++)
        {
            foreach (GameObject gameObject in slotAccessories[i].gameObjects)
            {
                gameObject.SetActive(i == index);
            }
        }
    }

    public void NextSlot1Accessory() => LoopSlotAccessory(slot1Accessories, ref _slot1Index, 1);
    public void NextSlot2Accessory() => LoopSlotAccessory(slot2Accessories, ref _slot2Index, 1);
    public void NextSlot3Accessory()
    {
        LoopSlotAccessory(slot3Accessories, ref _slot3Index, 1);

        UpdateFeetColor();
    }

    public void PreviousSlot1Accessory() => LoopSlotAccessory(slot1Accessories, ref _slot1Index, -1);
    public void PreviousSlot2Accessory() => LoopSlotAccessory(slot2Accessories, ref _slot2Index, -1);
    public void PreviousSlot3Accessory()
    {
        LoopSlotAccessory(slot3Accessories, ref _slot3Index, -1);

        UpdateFeetColor();
    }

    private void LoopSlotAccessory(Accessory[] slotAccessories, ref int indexField, int delta)
    {
        int nextIndex = (int)Mathf.Repeat(indexField + delta, slotAccessories.Length);

        if (slotAccessories[nextIndex].Unlocked)
        {
            SetSlotAccessory(slotAccessories, ref indexField, nextIndex);
        }
        else
        {
            // If the accessory is locked loop to the next one in the same direction
            LoopSlotAccessory(slotAccessories, ref indexField, delta + (int)Mathf.Sign(delta));
        }
    }

    public void UnlockAccessories(Island island, bool value)
    {
        UnlockSlotAccessories(slot1Accessories, island, value);
        UnlockSlotAccessories(slot2Accessories, island, value);
        UnlockSlotAccessories(slot3Accessories, island, value);
    }

    private void UnlockSlotAccessories(Accessory[] accessories, Island island, bool value)
    {
        foreach(Accessory accessory in accessories)
        {
            if (accessory.island == island)
                accessory.Unlocked = value;
        }
    }

    private void UpdateFeetColor()
    {
        // Making feet invisible if shoes are on
        ello.materials[feetMaterialIndex].SetColor(feetColorProperty, _slot3Index == 0 ? defaultFeetColor : Color.clear);
    }
}
