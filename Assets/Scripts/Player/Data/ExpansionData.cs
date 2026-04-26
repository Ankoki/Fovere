using System.Collections.Generic;

public class ExpansionData : DataStorage
{

    public static ExpansionData Deserialize(Dictionary<string, object> data)
    {
        var result = new ExpansionData();
        return result;
    }
    
    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>();
    }
    
}