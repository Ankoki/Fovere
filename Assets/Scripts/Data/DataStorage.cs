using System.Collections.Generic;

/// <summary>
/// Should be used on any class that is classed as storage for any sort of data.
/// 
/// This will also require the class to be serializable, and will require two methods.
/// The first being `Serialize()`, which is abstract and required by the class.
/// The second being a `public static [Class] Deserialize(Dictionary[string, object])`, which is
/// required. This should be used by the parent data holder and used to easily generate serailized data to send
/// to the database.
/// </summary>
public abstract class DataStorage
{
    
    /// <summary>
    /// Serializes the current object into a dictionary for json storage.
    /// </summary>
    /// <returns>The current object represented as a dictionary.</returns>
    public abstract Dictionary<string, object> Serialize();
    
}