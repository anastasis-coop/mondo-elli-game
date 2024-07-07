using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Common/Item")]
public class Item : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }

    [field: SerializeField] public GameObject Prefab { get; private set; }

    [field: SerializeField] public Sprite Sprite { get; private set; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(Name)) Name = name;
    }
#endif
}