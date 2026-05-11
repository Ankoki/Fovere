using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class to handle different types of world generation.
/// Allows for different worlds to use different generators with easily defined parameters.
/// </summary>
public abstract class Generator : MonoBehaviour
{

    private static readonly Dictionary<string, Generator> Generators = new();

    /// <summary>
    /// Gets a terrain generator by the name.
    /// The generator must be loaded as an object.
    /// </summary>
    /// <param name="name">The name of the generator to find.</param>
    /// <returns>The generator, or null if not found.</returns>
    public static Generator GetGenerator(string name)
    {
        return Generators.GetValueOrDefault(name, null);
    }

    /// <summary>
    /// Gets the name of a generator if registered.
    /// </summary>
    /// <param name="generator">The generator to search for.</param>
    /// <returns>The name of the generator, or an empty string.</returns>
    public static string GetName(Generator generator)
    {
        foreach (var pair in Generators)
        {
            if (pair.Value == generator)
                return pair.Key;
        }
        return string.Empty;
    }

    /// <summary>
    /// Registers a generator to be found by the game.
    /// </summary>
    /// <param name="name">The name of the generator to register.</param>
    /// <param name="generator">The generator.</param>
    protected static void RegisterGenerator(string name, Generator generator)
    {
        Generators[name] = generator;
    }
    
    // TODO change how this works. This is for prototype terrain generation.
    protected ItemType TopBlock;
    protected WeightedTable SubsurfaceTable;
    
    public int worldXLength = 250;
    public int worldZLength = 250;
    [Header("Noise Octave Settings")]
    [SerializeField] [Range(0.0f, 10.0f)] protected List<float> octaveFrequencies = new() { 1.0f, 1.5f, 2.0f, 2.5f };
    [SerializeField] [Range(0.0f, 1.0f)] protected List<float> octaveAmplitudes = new () { 1.0f, 0.9f, 0.7f, 0.1f };
    
    
    /// <summary>
    /// Generates terrain with the given attributes for the world provided.
    /// </summary>
    /// <param name="world">The world to generate this terrain for.</param>
    public abstract void Generate(World world);

    public override string ToString()
    {
        return $"Generator[worldXLength={worldXLength}," +
               $"worldZLength={worldZLength}," +
               $"octaveFrequencies=[{string.Join(",", octaveFrequencies)}]," +
               $"octaveAmplitudes=[{string.Join(",", octaveAmplitudes)}]]";
    }
    
} 