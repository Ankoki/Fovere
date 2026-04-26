using System.Collections.Generic;

public class EconomyData : DataStorage
{

    public static EconomyData Deserialize(Dictionary<string, object> data)
    {
        var result = new EconomyData();
        return result;
    }
    
    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>();
    }
    
}