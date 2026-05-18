using System.Collections.Generic;

namespace Fovere
{
    public class ProfessionData : DataStorage
    {

        public static ProfessionData Deserialize(Dictionary<string, object> data)
        {
            var result = new ProfessionData();
            return result;
        }

        public override Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>();
        }

    }
}