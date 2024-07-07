using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New CompositeItemSet", menuName = "Common/Composite Item Set")]
public sealed class CompositeItemsSet : ItemsSet
{
    [SerializeField]
    private ItemsSet _itemsSet;

    protected override Dictionary<string, Item> ToDictionary()
    {
        var dictionary = base.ToDictionary();

        foreach (var key in _itemsSet.Keys)
        {
            dictionary.Add(key, _itemsSet[key]);
        }

        return dictionary;
    }

    public override IEnumerable<Item> Take(int n)
    {
        var extras = n - _itemsSet.Count;
        var enumerable = extras >= 0 ? _itemsSet.Values : _itemsSet.Take(n);

        if (extras > 0) enumerable = enumerable.Concat(base.Take(extras));

        return enumerable;
    }
}