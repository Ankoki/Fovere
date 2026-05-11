using System;
using System.Collections;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Handles generation of basic terrain for our village world.
/// </summary>
public class VillageGenerator : Generator
{

    private Random _random;
    private World _currentWorld;
    private int _seed;

    private bool _awake;

    private void Awake()
    {
        if (_awake)
            return;
        RegisterGenerator("fovere:village_generator", this);
        SubsurfaceTable = new SubsurfaceTerrainTable();
        _random = new Random(Guid.NewGuid().GetHashCode());
        _seed = _random.Next();
        _awake = true;
    }
    
    public override void Generate(World world)
    {
        if (!_awake)
            Awake();
        
        var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Debug.Log("Generating terrain, start: " + milliseconds);
        if (TopBlock == null || !TopBlock.IsBlock)
            TopBlock = ItemType.Get(ItemType.Keys.GrassBlock); 
        _currentWorld = world;
        for (var x = _currentWorld.worldPos.x; x < worldXLength; x++)
        {
            for (var z = _currentWorld.worldPos.z; z < worldZLength; z++)
            {
                var height = GenerateNoise(x, z);
                for (var y = 0; y <= height; y++)
                {
                    var pos = new Vector3Int(x, y, z);
                    _currentWorld.SetUnprocessedBlock(y == height ? TopBlock : SubsurfaceTable.Next(), pos);
                }
            }
        }
        Debug.Log("Terrain generated, time elapsed in millis: " + (DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds));
        Debug.Log("Chunks rebuilt async, time elapsed in millis: " + (DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds));
        Debug.Log("Total Chunks made and built: " + _currentWorld.GetChunks().Length);
        _currentWorld.SetLoaded();
    }

    private const float PerlinScale = 15f;
    private const int HeightScale = 10;
    private const int WorldHeight = 1;
    private const int MaxHeight = 256;

    /// <summary>
    /// Generates noise with the given parameters.
    /// </summary>
    /// <param name="x">The x-coordinates.</param>
    /// <param name="z">The z-coordinates.</param>
    /// <returns>The noise generated for the given coordinates.</returns>
    private int GenerateNoise(int x, int z)
    {
        var y = (int) (Mathf.PerlinNoise((x + _seed) / PerlinScale, (z + _seed) / PerlinScale) * HeightScale) + WorldHeight;
        y = Mathf.Clamp(y, 0, MaxHeight - 1);
        return y;

        /*
        var noiseX = x + _seed;
        var noiseZ = z + _seed;
        var max = Mathf.Max(octaveFrequencies.Count, octaveAmplitudes.Count);
        var perlin = 0.0f;
        for (var i = 0; i < max; i++)
            perlin += octaveAmplitudes[i] * Mathf.PerlinNoise(octaveFrequencies[i] * noiseX + 20,
                                                         octaveFrequencies[i] * noiseZ + 20);

        return Mathf.FloorToInt(perlin);*/
    }
    
}