using System;
using UnityEngine;

/// <summary>
/// Handles generation of basic terrain for the worlds.
/// </summary>
public class TerrainGenerator : Generator
{
    
    private World _currentWorld;

    private void Start()
    {
        SubsurfaceTable = new SubsurfaceTerrainTable();
        TopBlock = ItemType.GrassBlock;
    }

    public override void Generate(World world)
    {
        var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Debug.Log("Generating terrain, start: " + milliseconds);
        if (!TopBlock.isBlock)
            throw new ArgumentException("TopBlock must return true for ItemType#IsBlock");
        _currentWorld = world;
        for (var x = _currentWorld.worldPos.x; x < worldXLength; x++)
        {
            for (var z = _currentWorld.worldPos.z; z < worldZLength; z++)
            {
                var height = GenerateNoise(x, z, noiseDetail);
                for (var y = 0; y < height; y++)
                {
                    var pos = new Vector3Int(x, y, z);
                    _currentWorld.SetBlock(y == height - 1 ? TopBlock : SubsurfaceTable.Next(), pos);
                }
            }
        }
        Debug.Log("Terrain generated, time elapsed in millis: " + (DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds));
        foreach (var chunk in world.GetChunks())
            _ = world.RebuildChunkAsync(chunk);
        Debug.Log("Chunks rebuilt async, time elapsed in millis: " + (DateTimeOffset.Now.ToUnixTimeMilliseconds() - milliseconds));
        Debug.Log("Total Chunks made and built: " + _currentWorld.GetChunks().Length);
    }

    /// <summary>
    /// Generates noise with the given parameters.
    /// </summary>
    /// <param name="x">The x-coordinates.</param>
    /// <param name="z">The z-coordinates.</param>
    /// <param name="detailScale">The detail scale of the noise.</param>
    /// <returns></returns>
    private int GenerateNoise(int x, int z, float detailScale)
    {
        var xNoise = x / detailScale;
        var zNoise = z / detailScale;
        var perlin = Mathf.PerlinNoise(xNoise, zNoise);
        return Mathf.Max(1, Mathf.FloorToInt(perlin * worldYLength));
    }
    
}