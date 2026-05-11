using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Inventory : DataStorage
{

    private static readonly string[] InventoryStructure =
    {
        "inventoryType",
        "inventorySize"
    };

    public static Inventory Deserialize(Dictionary<string, object> data)
    {
        if (!DataHelpers.ValidateKeys(data, InventoryStructure))
        {
            Debug.Log("Malformed inventory data, empty player inventory given.");
            return new PlayerInventory();
        }
        var type = data.GetValueOrDefault("inventoryType", "player");
        var result = new PlayerInventory(); // Currently only type of inventory. To be expanded on when other types are introduced.
        var size = (int) data["inventorySize"];
        for (var i = 0; i < size; i++)
            result.SetItem(i, Item.Deserialize((Dictionary<string, object>) data[$"{i}"]));
        return result;
    }
    
    private Item[] _items;

    private void Awake()
    {
        _items = new Item[GetInventorySize()];
        Enumerable.Range(0, GetInventorySize()).ToList().ForEach(i => _items[i] = null);
    }

    /// <summary>
    /// Sets an item in the inventory.
    /// </summary>
    /// <param name="index">The index to set.</param>
    /// <param name="item">The item to set it too.</param>
    public void SetItem(int index, Item item)
    {
        _items[index] = item;
        UpdateInventory();
    }
    
    /// <summary>
    /// Adds an item to the inventory.
    /// </summary>
    /// <param name="itemType">The item.</param>
    /// <returns>True if successful and was space, else false.</returns>
    public bool AddItem(ItemType itemType)
    {
        var result = false;
        for (var i = 0; i < _items.Length; i++)
        {
            var item = _items[i];
            if (item == null || item.GetItemType().Key != itemType.Key || item.GetAmount() >= item.GetItemType().MaxStackSize)
                continue;
            item.SetAmount(item.GetAmount() + 1);
            result = true;
        }
        if (result)
            return true;
        for (var i = 0; i < _items.Length; i++)
        {
            var item = _items[i];
            if (item != null)
                continue;
            _items[i] = new Item(itemType);
            result = true;
        }
        return result;
    }

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    /// <param name="itemType">The item to remove.</param>
    /// <param name="amount">The amount to remove.</param>
    /// <returns>True if successful, else false.</returns>
    public bool RemoveItem(ItemType itemType, int amount = 1)
    {
        if (amount <= 0)
            return false;
        var toUpdate = new Item[GetInventorySize()];
        var originalAmount = amount;
        var index = 0;
        for (var i = 0; i < _items.Length; i++)
        {
            var item = _items[i];
            if (item == null || item.GetItemType().Key != itemType.Key)
                continue;
            var result = item.GetAmount() - amount;
            amount -= result;
            toUpdate[index] = item;
            index++;
        }
        if (amount > 0)
            return false;
        for (var i = 0; i < toUpdate.Length; i++)
        {
            var item = toUpdate[i];
            if (item == null || originalAmount <= 0)
                continue;
            item.SetAmount(item.GetAmount() - originalAmount);
            originalAmount = -item.GetAmount();
        }
        UpdateInventory();
        return true;
    }

    /// <summary>
    /// Checks if the inventory contains a given item.
    /// </summary>
    /// <param name="itemType">The item to search for.</param>
    /// <param name="amount">The amount.</param>
    /// <returns>True if found, else false.</returns>
    public bool ContainsItem(ItemType itemType, int amount = 1)
    {
        for (var i = 0; i < _items.Length; i++)
        {
            var item = _items[i];
            if (item == null || item.GetItemType().Key != itemType.Key)
                continue;
            amount -= item.GetAmount();
        }
        return amount <= 0;
    }

    /// <summary>
    /// Checks if the inventory can hold the given item.
    /// </summary>
    /// <param name="itemType">The item to check for.</param>
    /// <param name="amount">The amount.</param>
    /// <returns>True if can hold, else false.</returns>
    public bool CanHold(ItemType itemType, int amount = 1)
    {
        return true; // TODO
    }

    /// <summary>
    /// Gets the array of items making up this inventory.
    /// </summary>
    /// <returns>The items.</returns>
    public Item[] GetItems()
    {
        return _items;
    }

    /// <summary>
    /// Updates the inventory to remove any sub-zero values.
    /// </summary>
    public void UpdateInventory()
    {
        var index = 0;
        _items.ToList().ForEach(i =>
        {
            if (i != null && i.GetAmount() <= 0)
                _items[index] = null;
            index++;
        });
    }

    public override Dictionary<string, object> Serialize()
    {
        if (_items == null) // Created for serialization purposes, give a default minimum inventory.
        {
            return new Dictionary<string, object>
            {
                { "inventorySize", 16 }
            };
        }
        var data = new Dictionary<string, object>
        {
            { "inventorySize", GetInventorySize() }
        };
        if (_items == null)
            return data;
        for (var i = 0; i < _items.Length; i++)
            if (_items[i] != null)
                data.Add($"{i}", _items[i].Serialize());
        return data;
    }

    /// <summary>
    /// Gets the size of the current inventory.
    /// </summary>
    /// <returns>The inventory size.</returns>
    public abstract int GetInventorySize();
    
}