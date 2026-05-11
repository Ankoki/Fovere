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
        playerData.LastWorld = world;
        playerData.LastPosition = DataHelpers.ToVector3(lastPos.Replace("," + world, ""));
        playerData.Inventory = Inventory.Deserialize(data);
        playerData.QuestData = QuestData.Deserialize(data);
        playerData.ProfessionData = ProfessionData.Deserialize(data);
        playerData.AbilitiesData  = AbilitiesData.Deserialize(data);
        playerData.InterestData = InterestData.Deserialize(data);
        playerData.EconomyData = EconomyData.Deserialize(data);
        playerData.ExpansionData = ExpansionData.Deserialize(data);
        return playerData;
    }
    
    // Data
    public string PlayerId;
    public string LastWorld;
    public Vector3 LastPosition;
    public Inventory Inventory;
    public QuestData QuestData;
    public ProfessionData ProfessionData;
    public AbilitiesData AbilitiesData;
    public InterestData InterestData;
    public EconomyData EconomyData;
    public ExpansionData ExpansionData;

    private PlayerData(string identifier)
    {
        PlayerId = identifier;
    }

    /// <summary>
    /// Used to update the inventory in the player data. Should be called before calling Serialize().
    /// </summary>
    /// <param name="inventory">The inventory to save.</param>
    public void PushInventory(Inventory inventory)
    {
        Inventory = inventory;
    }

    /// <summary>
    /// Pushes the last position of the player to the data. Should be called before calling Serialize().
    /// </summary>
    /// <param name="vector">The vector position of the player.</param>
    /// <param name="world">The last seen world.</param>
    public void PushLastPosition(Vector3 vector, World world)
    {
        LastPosition = vector;
        LastWorld = world.label;
    }

    public override Dictionary<string, object> Serialize()
    {
        var data = new Dictionary<string, object>
        {

            { "user_id", PlayerId },
            { "last_position", $"{DataHelpers.FromVector3(LastPosition)},{LastWorld}" },
            { "inventory_data", Inventory },
            { "quest_data", QuestData },
            { "profession_data", ProfessionData },
            { "abilities_data", AbilitiesData },
            { "interest_data", InterestData },
            { "economy_data", EconomyData },
            { "expansion_data", ExpansionData }
        };
        return data;
    }
    
}