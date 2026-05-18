using System.Collections.Generic;

namespace Fovere
{
    public class PlayerInventory : Inventory
    {

        private readonly PlayerData _playerData;

        public PlayerInventory()
        {
        }

        public override int GetInventorySize()
        {
            return 16; /*
            if (_playerData.HasExpansion(PlayerData.Expansion.InventoryRowFour))
                return 32;
            if (_playerData.HasExpansion(PlayerData.Expansion.InventoryRowThree))
                return 24;
            return _playerData.HasExpansion(PlayerData.Expansion.InventoryRowTwo) ? 16 : 8; */
        }

        public override Dictionary<string, object> Serialize()
        {
            // Adds the type of inventory on this child of Inventory.
            var data = base.Serialize();
            data.Add("inventoryType", "player");
            return data;
        }

    }
}