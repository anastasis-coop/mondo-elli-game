using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New ItemSet", menuName = "Common/Item Set")]
public class ItemsSet : ScriptableObject, IReadOnlyDictionary<string, Item>
{
    [SerializeField]
    private List<Item> _entries;

    private Dictionary<string, Item> _dict;

    private readonly List<Item> _shuffleCopy = new();

    private Dictionary<string, Item> Dictionary => _dict ??= ToDictionary();

    protected virtual Dictionary<string, Item> ToDictionary()
    {
        return _entries.ToDictionary(item => item.Name, item => item);
    }

    public Item this[string key] => Dictionary[key];

    public IEnumerable<string> Keys => Dictionary.Keys;

    public IEnumerable<Item> Values => Dictionary.Values;

    public int Count => Dictionary.Count;

    public bool Contains(KeyValuePair<string, Item> item) => Dictionary.Contains(item);

    public void CopyTo(KeyValuePair<string, Item>[] array, int arrayIndex)
        => ((IDictionary<string, Item>)Dictionary).CopyTo(array, arrayIndex);

    public Item PickRandom() => PickRandom((ICollection<string>)null);

    public Item PickRandom(ICollection<string> except)
    {
        if (_shuffleCopy.Count == 0) _shuffleCopy.AddRange(Dictionary.Values);

        if (except != null)
        {
            foreach (var value in except)
            {
                if (!Dictionary.TryGetValue(value, out var entry)) continue;
                _shuffleCopy.Remove(entry);
            }
        }

        var element = _shuffleCopy[Random.Range(0, _shuffleCopy.Count)];

        if (except != null)
        {
            foreach (var value in except)
            {
                if (!Dictionary.TryGetValue(value, out var entry)) continue;
                _shuffleCopy.Add(entry);
            }
        }

        return element;
    }

    public Item PickRandom(ICollection<Item> except)
    {
        if (_shuffleCopy.Count == 0) _shuffleCopy.AddRange(Dictionary.Values);

        if (except != null)
        {
            foreach (var value in except)
                _shuffleCopy.Remove(value);
        }

        var element = _shuffleCopy[Random.Range(0, _shuffleCopy.Count)];

        if (except != null)
        {
            foreach (var value in except)
            {
                if (!Dictionary.ContainsValue(value)) continue;
                _shuffleCopy.Add(value);
            }
        }

        return element;
    }

    public bool ContainsKey(string key) => Dictionary.ContainsKey(key);

    public bool TryGetValue(string key, out Item value) => Dictionary.TryGetValue(key, out value);

    public virtual IEnumerable<Item> Take(int n) => _entries.Take(n);

    public IEnumerator<KeyValuePair<string, Item>> GetEnumerator() => Dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}