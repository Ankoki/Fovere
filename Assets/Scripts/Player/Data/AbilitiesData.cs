using System.Collections.Generic;

public class AbilitiesData : DataStorage
{

    public static AbilitiesData Deserialize(Dictionary<string, object> data)
    {
        var result = new AbilitiesData();
        return result;
    }
    
    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>();
    }
    
}