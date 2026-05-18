using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Fovere
{
    /// <summary>
    /// Class used to retrieve all items that can be carried, placed or generated.
    /// Contains all essential data about an item.
    /// </summary>
    [PublicAPI]
    public class ItemType : Comparer<ItemType>
    {

        public static class Keys
        {
            public const string Air = "fovere:air";
            public const string GrassBlock = "fovere:grass_block";
            public const string Dirt = "fovere:dirt";
            public const string Stone = "fovere:stone";
            public const string ExpraDeposit = "fovere:expra_deposit";
            public const string UnknownBlock = "fovere:unknown_block";
        }

        private static bool initialised;

        /// <summary>
        /// Create all Fovere ItemTypes. Should only be called once by the system.
        /// </summary>
        public static void Initialize()
        {
            if (initialised)
                return;
            initialised = true;
            Create(Keys.Air,
                "Air",
                "N/A",
                1,
                true,
                null);
            Create(Keys.GrassBlock,
                "Grass Block",
                "A resource commonly found at the surface.",
                99,
                true,
                Resources.Load<Sprite>("Icons/Block/GrassBlock"),
                true);
            Create(Keys.Dirt,
                "Dirt",
                "A resource commonly found just under the surface.",
                99,
                true,
                Resources.Load<Sprite>("Icons/Block/Dirt"));
            Create(Keys.Stone,
                "Stone",
                "A resource commonly found just under the surface.",
                99,
                true,
                Resources.Load<Sprite>("Icons/Block/Stone"));
            Create(Keys.ExpraDeposit,
                "Expra Deposit",
                "A resource that has many interesting properties...",
                99,
                true,
                Resources.Load<Sprite>("Icons/Block/ExpraDeposit"));
            Create(Keys.UnknownBlock,
                "Unknown Block",
                "An issue most likely occured during inventory deserialization. The item is unobtainable at this time.",
                99,
                true,
                Resources.Load<Sprite>("Icons/Block/UnknownBlock"));
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
            var itemType = new ItemType(key)
            {
                DisplayName = displayName,
                Description = description,
                MaxStackSize = maxStackSize,
                IsBlock = isBlock,
            };
            if (icon != null)
                itemType.Icon = icon;
            itemType._top = key + (hasUniqueSides ? "_top" : "");
            itemType._side = key + (hasUniqueSides ? "_side" : "");
            itemType._bottom = key + (hasUniqueSides ? "_bottom" : "");
            keyMap.TryAdd(itemType.Key, itemType);
            return itemType;
        }

        // For controlled access through the Create method.
        protected ItemType(string key)
        {
            Key = key;
        }

        private static Dictionary<string, ItemType> keyMap = new();

        /// <summary>
        /// Gets the item type with the given key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The item type with the given key, or Air if not found.</returns>
        public static ItemType Get(string key)
        {
            if (key.EndsWith("_top") || key.EndsWith("_side") || key.EndsWith("_bottom"))
            {
                var split = key.Split('_');
                var str = string.Empty;
                for (var i = 0; i < split.Length - 1; i++)
                    str += split[i];
                key = str;
            }

            var backup = keyMap.GetValueOrDefault(Keys.UnknownBlock);
            return keyMap.GetValueOrDefault(key, backup);
        }

        public readonly string Key;
        public string DisplayName;
        public string Description;
        public int MaxStackSize;
        public bool IsBlock;
        public Sprite Icon;
        private string _top;
        private string _side;
        private string _bottom;

        public int GetTextureIndex(int axis, int direction)
        {
            var atlas = TextureAtlas.GetAtlas();
            if (!IsBlock) return 0;
            if (axis == 1 && direction == 1)
                return atlas.GetIndex(_top).x;
            if (axis == 1 && direction == -1)
                return atlas.GetIndex(_bottom).x;
            return atlas.GetIndex(_side).x; // TODO migrate to new procedural generation model created externally.
        }

        public override int Compare(ItemType x, ItemType y)
        {
            switch (x)
            {
                case null when y == null:
                    return 0;
                case null:
                    return -1;
            }

            if (y == null)
                return 1;
            return x.Key == y.Key ? 0 : -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is not ItemType type)
                return false;
            return type.Key == Key;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

    }
}