using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

/// <summary>
/// Utility class to create probability based type generation where necessary.
/// Each type contains a rarity, with higher rarity being more common.
/// </summary>
public class WeightedTable
{
    private readonly Random _random = new();

    private readonly Dictionary<ItemType, int> _weightMap = new();
    private readonly Dictionary<int, ItemType> _cycle = new();

    /// <summary>
    /// Places a type into the current table with the given weight.
    /// </summary>
    /// <param name="type">The type object to add to the table.</param>
    /// <param name="weight">The weight of the type in the table.</param>
    public void Place(ItemType type, int weight)
    {
        _weightMap.Add(type, weight);
    }

    /// <summary>
    /// Revokes a type from this table.
    /// </summary>
    /// <param name="type">The type to remove.</param>
    public void Revoke(ItemType type)
    {
        _weightMap.Remove(type);
    }

    /// <summary>
    /// Gets the next type of the session in this table.
    /// </summary>
    /// <returns></returns>
    public ItemType Next()
    {
        if (_cycle.Count == 0) // Fills the cycle map for this session if not already completed.
        {
            var total = 0;
            foreach (var (type, weight) in _weightMap.Select(x => (x.Key, x.Value)))
            {
                var range = Enumerable.Range(total, weight);
                total += weight;
                foreach (var i in range)
                    _cycle.Add(i, type);
            }
        }
        return _cycle[_random.Next(0, _cycle.Count)]; // Fallback just in case.
    }

    /// <summary>
    /// Clears the type table. Should be called after every use cycle.
    /// </summary>
    public void Clear()
    {
        _cycle.Clear();
    }
    
}