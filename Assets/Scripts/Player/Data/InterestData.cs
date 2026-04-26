using System.Collections.Generic;

public class InterestData : DataStorage
{

    public static InterestData Deserialize(Dictionary<string, object> data)
    {
        var result = new InterestData();
        return result;
    }
    
    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>();
    }
    
}