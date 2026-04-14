using UnityEngine;

/// <summary>
/// Utility class to handle different types of world generation.
/// Allows for different worlds to use different generators with easily defined parameters.
/// </summary>
public abstract class Generator : MonoBehaviour
{
    
    public ItemType TopBlock;
    protected WeightedTable SubsurfaceTable;
    
    public int worldXLength = 300;
    public int worldZLength = 300;
    public int worldYLength = 32;
    public float noiseDetail = 8f;
    
    /// <summary>
    /// Generates terrain with the given attributes for the world provided.
    /// </summary>
    /// <param name="world">The world to generate this terrain for.</param>
    public abstract void Generate(World world);
    
} 