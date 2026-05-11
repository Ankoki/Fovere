using System.Collections.Generic;

public class QuestData : DataStorage
{

    public static QuestData Deserialize(Dictionary<string, object> data)
    {
        var result = new QuestData();
        return result;
    }

    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>();
    }
    
}