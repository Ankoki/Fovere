public class PlayerInventory : Inventory
{

    private readonly PlayerData _playerData;

    public PlayerInventory(PlayerData playerData)
    {
        _playerData = playerData;
    }
    
    public override int GetInventorySize()
    {
        if (_playerData.HasExpansion(PlayerData.Expansion.InventoryRowFour))
            return 32;
        if (_playerData.HasExpansion(PlayerData.Expansion.InventoryRowThree))
            return 24;
        return _playerData.HasExpansion(PlayerData.Expansion.InventoryRowTwo) ? 16 : 8;
    }

}