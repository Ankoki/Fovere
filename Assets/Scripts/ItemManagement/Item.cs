using JetBrains.Annotations;

/// <summary>
/// Class used to programatically handle a collective amount of items. Mainly for Inventory usage.
/// </summary>
[PublicAPI]
public class Item
{

    /// <summary>
    /// Creates a new Item.
    /// </summary>
    /// <param name="itemType">The base ItemType of this Item.</param>
    /// <param name="amount">The amount of the ItemType that are in this stack. Defaults to 1.</param>
    public Item(ItemType itemType, int amount = 1):this(itemType, itemType.displayName, itemType.description, amount) {}
    
    /// <summary>
    /// Creates a new Item.
    /// </summary>
    /// <param name="itemType">The base ItemType of this Item.</param>
    /// <param name="displayName">The desired display name of this item.</param>
    /// <param name="description">The desired description of this item.</param>
    /// <param name="amount">The amount of the ItemType that are in this stack. Defaults to 1.</param>
    public Item(ItemType itemType, string displayName, string description, int amount = 1)
    {
        _itemType = itemType;
        _displayName = displayName;
        _description = description;
        _amount = amount;
    }

    private readonly ItemType _itemType;
    private string _displayName;
    private string _description;
    private int _amount;

    /// <summary>
    /// Gets the base ItemType of this stack.
    /// </summary>
    /// <returns>The ItemType.</returns>
    public ItemType GetItemType()
    {
        return _itemType;
    }

    /// <summary>
    /// Gets the display name of this stack.
    /// </summary>
    /// <returns>The display name.</returns>
    public string GetDisplayName()
    {
        return _displayName;
    }

    /// <summary>
    /// Sets the display name of this stack.
    /// </summary>
    /// <param name="displayName">The new display name.</param>
    public void SetDisplayName(string displayName)
    {
        _displayName = displayName;
    }

    /// <summary>
    /// Gets the description of this stack.
    /// </summary>
    /// <returns>The description.</returns>
    public string GetDescription()
    {
        return _description;
    }

    /// <summary>
    /// Sets the description of this stack.
    /// </summary>
    /// <param name="description">The new description.</param>
    public void SetDescription(string description)
    {
        _description = description;
    }
    
    /// <summary>
    /// Gets the amount of the base ItemType in this stack.
    /// Will not be larger than Item#GetItemType()#maxStackSize.
    /// </summary>
    /// <returns>The amount.</returns>
    public int GetAmount()
    {
        return _amount;
    }
    
    /// <summary>
    /// Sets the amount of the base ItemType in this stack.
    /// </summary>
    /// <param name="amount">The new amount.</param>
    public void SetAmount(int amount)
    {
        _amount = amount;
    }

}
