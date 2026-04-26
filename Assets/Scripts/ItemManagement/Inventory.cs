using System.Collections.Generic;
using System.Linq;

public abstract class Inventory : DataStorage
{

    public static Inventory Deserialize(Dictionary<string, object> data)
    {
        var type = data.GetValueOrDefault("inventory_type", "player");
        Inventory result;
        if ((string) type == "player")
        {
            result = new PlayerInventory();
        }
        else return null;
        return result;
    }
    
    private Item[] _items;

    private void Awake()
    {
        _items = new Item[GetInventorySize()];
        Enumerable.Range(0, GetInventorySize()).ToList().ForEach(i => _items[i] = null);
    }

    public bool AddItem(ItemType itemType)
    {
        var result = false;
        for (var i = 0; i < _items.Length; i++)
        {
            var item = _items[i];
            if (item == null || item.GetItemType().key != itemType.key || item.GetAmount() >= item.GetItemType().maxStackSize)
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
            if (item == null || item.GetItemType().key != itemType.key)
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

    public bool ContainsItem(ItemType itemType, int amount = 1)
    {
        for (var i = 0; i < _items.Length; i++)
        {
            var item = _items[i];
            if (item == null || item.GetItemType().key != itemType.key)
                continue;
            amount -= item.GetAmount();
        }
        return amount <= 0;
    }

    public bool CanHold(ItemType itemType, int amount = 1)
    {
        return true; // TODO
    }

    public Item[] GetItems()
    {
        return _items;
    }

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
        var data = new Dictionary<string, object>();
        return data;
    }

    public abstract int GetInventorySize();
    
}