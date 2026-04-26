using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Should be used on any class that is classed as storage for any sort of data.
/// This shall provide utility methods for serializing commonly used classes.
///
/// This will also require the class to be serializable, and will require two methods.
/// The first being `Serialize()`, which is abstract and required by the class.
/// The second being a `public static [Class] Deserialize(Dictionary[string, object])`, which is
/// required. This should be used by the parent data holder and used to easily generate serailized data to send
/// to the database.
/// </summary>
public abstract class DataStorage
{
    protected static Vector3 ToVector(string vector)
    {
        var split = vector.Split(',');
        return new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
    }
    
    protected static string FromVector(Vector3 vector)
    {
        return $"{vector.x},{vector.y},{vector.z}";
    }

    protected static string FromVector(Vector2 vector)
    {
        return $"{vector.x},{vector.y}";
    }

    protected static string FromVector(Vector3 vector, World world)
    {
        return $"{vector.x},{vector.y},{vector.z},{world.label}";
    }
    
    public abstract Dictionary<string, object> Serialize();
    
}