using UnityEngine;

/// <summary>
/// Class used to handle block positions and typings.
/// </summary>
public class Block
{

    /// <summary>
    /// Creates an air block for the given location.
    /// These do not contain icon sprites or prefab GameObjects, so such fields must be avoided.
    /// </summary>
    /// <param name="world">The world of this block.</param>
    /// <param name="x">The x-coord of this block.</param>
    /// <param name="y">The y-coord of this block.</param>
    /// <param name="z">The z-coord of this block.</param>
    /// <returns>An air block object.</returns>
    public static Block GetAirAt(World world, int x, int y, int z)
    {
        return GetAirAt(world, new Vector3Int(x, y, z));
    }

    /// <summary>
    /// Creates an air block for the given location.
    /// These do not contain icon sprites or prefab GameObjects, so such fields must be avoided.
    /// </summary>
    /// <param name="world">The world of this block.</param>
    /// <param name="pos">The position of this block.</param>
    /// <returns>An air block object.</returns>
    public static Block GetAirAt(World world, Vector3Int pos)
    {
        return new Block(pos, world, ItemType.Get(ItemType.Keys.Air));
    }

    public readonly World World;
    public readonly ItemType Type;
    public Vector3Int Position;

    /// <summary>
    /// Creates a new block for the given parameters.
    /// </summary>
    /// <param name="position">The position of the block.</param>
    /// <param name="world">The world of the block.</param>
    /// <param name="type">The blocks typing.</param>
    public Block(Vector3Int position, World world, ItemType type)
    {
        Position = position;
        World = world;
        Type = type;
    }

    public override string ToString()
    {
        return Type.DisplayName + "[" + Position.x + "," + Position.y + "," + Position.z + "," + World.label +"]";
    }
    
}