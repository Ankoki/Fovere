using System.Collections.Generic;

namespace Fovere
{
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
}