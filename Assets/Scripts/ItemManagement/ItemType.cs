using System;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Class used to retrieve all items that can be carried, placed or generated.
/// Contains all essential data about an item.
/// </summary>
[PublicAPI]
public class ItemType
{

    // Used only for empty space in the world. Not to be interacted with as a proper block.
    public static ItemType Air = Create("fovere:air",
                                "Air",
                                            "N/A",
                                0,
                                    true,
                                    null);
    public static ItemType GrassBlock = Create("fovere:grass_block",
        "Grass Block",
        "A resource commonly found at the surface.",
        99,
        true,
        Resources.Load<Sprite>("Icons/Block/GrassBlock"),
        true);
    public static ItemType Dirt = Create("fovere:dirt",
        "Dirt",
        "A resource commonly found just under the surface.",
        99,
        true,
        Resources.Load<Sprite>("Icons/Block/Dirt"));
    public static ItemType Stone = Create("fovere:stone",
        "Stone",
        "A resource commonly found just under the surface.",
        99,
        true,
        Resources.Load<Sprite>("Icons/Block/Stone"));
    public static ItemType ExpraDeposit = Create("fovere:expra_deposit",
        "Expra Deposit",
        "A resource that has many interesting properties...",
        99,
        true,
        Resources.Load<Sprite>("Icons/Block/ExpraDeposit"));

    // For controlled access through the Create method.
    private ItemType() {}

    /// <summary>
    /// Creates a new ItemType for usage throughout Fovēre.
    /// Please note all ItemType keys are essential to be in the format "hook:block_name".
    /// This is important as the block atlas pointer is set based on the name of the block's key.
    /// </summary>
    /// <param name="key">The item's key. It is important that this is unique. All base game ItemTypes will be prefixed with "fovere:".</param>
    /// <param name="displayName">The default display name this item will have.</param>
    /// <param name="description">The default description this item will have.</param>
    /// <param name="maxStackSize">The max stack size of this item.</param>
    /// <param name="isBlock">Whether this ItemType is a block or not.</param>
    /// <param name="icon">The icon to be displayed in inventories for this item.</param>
    /// <returns>The newly created ItemType.</returns>
    public static ItemType Create(string key, string displayName, string description, int maxStackSize,
        bool isBlock, Sprite icon)
    {
        return Create(key, displayName, description, maxStackSize, isBlock, icon, false);
    }

    /// <summary>
    /// Creates a new ItemType for usage throughout Fovēre.
    /// Please note all ItemType keys are essential to be in the format "hook:block_name".
    /// This is important as the block atlas pointer is set based on the name of the block's key.
    /// </summary>
    /// <param name="key">The item's key. It is important that this is unique. All base game ItemTypes will be prefixed with "fovere:".</param>
    /// <param name="displayName">The default display name this item will have.</param>
    /// <param name="description">The default description this item will have.</param>
    /// <param name="maxStackSize">The max stack size of this item.</param>
    /// <param name="isBlock">Whether this ItemType is a block or not.</param>
    /// <param name="icon">The icon to be displayed in inventories for this item.</param>
    /// <param name="hasUniqueSides">Should be true if isBlock is true and has different textures for the top and bottom, else false.</param>
    /// <returns>The newly created ItemType.</returns>
    public static ItemType Create(string key, string displayName, string description, int maxStackSize,
        bool isBlock, Sprite icon, bool hasUniqueSides)
    {
        var itemType = new ItemType();
        itemType.key = key;
        itemType.displayName = displayName;
        itemType.description = description;
        itemType.maxStackSize = maxStackSize;
        itemType.isBlock = isBlock;
        itemType.icon = icon;
        var splitKey = key.Split(':')[1].ToLower();
        itemType._top = splitKey + (hasUniqueSides ? "_top" : "");
        itemType._side = splitKey + (hasUniqueSides ? "_side" : "");
        itemType._bottom = splitKey + (hasUniqueSides ? "_bottom" : "");
        return itemType;
    }

    public string key;
    public string displayName;
    public string description;
    public int maxStackSize;
    public bool isBlock;
    public Sprite icon;
    private string _top;
    private string _side;
    private string _bottom;

    public int GetTextureIndex(int axis, int direction)
    {
        var atlas = TextureArrayBuilder.Instance;
        if (!isBlock) return 0;
        if (axis == 1 && direction == 1)
            return atlas.GetIndex(_top);
        if (axis == 1 && direction == -1)
            return atlas.GetIndex(_bottom);
        return atlas.GetIndex(_side);
    }
    
}