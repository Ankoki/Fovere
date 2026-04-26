using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : DataStorage
{
    
    public static PlayerData Deserialize(Dictionary<string, object> data)
    {
        var playerData = new PlayerData((string) data.GetValueOrDefault("user_id", null));
        var lastPos = (string) data.GetValueOrDefault("last_position", "0,0,0,Village");
        var world = lastPos.Split(",")[3];
        playerData._lastWorld = world;
        playerData._lastPosition = ToVector(lastPos.Replace("," + world, ""));
        playerData._inventory = Inventory.Deserialize(data);
        playerData._professionData = ProfessionData.Deserialize(data);
        playerData._abilitiesData  = AbilitiesData.Deserialize(data);
        playerData._interestData = InterestData.Deserialize(data);
        playerData._economyData = EconomyData.Deserialize(data);
        playerData._expansionData = ExpansionData.Deserialize(data);
        return playerData;
    }
    
    // Data
    private string _playerId;
    private string _lastWorld;
    private Vector3 _lastPosition;
    private Inventory _inventory;
    private ProfessionData _professionData;
    private AbilitiesData _abilitiesData;
    private InterestData _interestData;
    private EconomyData _economyData;
    private ExpansionData _expansionData;

    public PlayerData(string identifier)
    {
        _playerId = identifier;
    }

    /// <summary>
    /// Used to update the inventory in the player data. Should be called before calling Serialize().
    /// </summary>
    /// <param name="inventory">The inventory to save.</param>
    public void PushInventory(Inventory inventory)
    {
        _inventory = inventory;
    }

    /// <summary>
    /// Pushes the last position of the player to the data. Should be called before calling Serialize().
    /// </summary>
    /// <param name="vector">The vector position of the player.</param>
    /// <param name="world">The last seen world.</param>
    public void PushLastPosition(Vector3 vector, World world)
    {
        _lastPosition = vector;
        _lastWorld = world.label;
    }

    public override Dictionary<string, object> Serialize()
    {
        var data = new Dictionary<string, object>();
        data.Add("user_id", _playerId);
        data.Add("last_position", FromVector(_lastPosition) + "," + _lastWorld);
        data.Add("inventory_data", _inventory);
        data.Add("profession_data", _professionData);
        data.Add("abilities_data", _abilitiesData);
        data.Add("interest_data", _interestData);
        data.Add("economy_data", _economyData);
        data.Add("expansion_data", _expansionData);
        return data;
    }
    
}