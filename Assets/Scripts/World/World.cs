using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Class to store all block and procedural generation data for given worlds.
/// Multiple worlds can be used and isolated, with different locations for each.
/// </summary>
[PublicAPI]
public class World : MonoBehaviour
{
    private static readonly Dictionary<string, World> Worlds = new();

    public static World Deserialize(Dictionary<string, object> data, Vector3 lastPosition) // Getting the player's last position allows us to only render chunks around the player.
    {
        var environmentObject = GameObject.Find("Environment");
        var gameObject = new GameObject("Village");
        gameObject.transform.SetParent(environmentObject.transform); // For better formatting within the scene.
        var world = gameObject.AddComponent<World>();
        if (data.ContainsKey("createWorld"))
        {
            world.label = "Village";
            world._start = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Debug.Log($"createWorld found. Generating starter village@{world._start}ms.");
            var generator = gameObject.AddComponent<VillageGenerator>();
            world.generator = generator;
            world.SetupChunks();
            Debug.Log($"Generator[{generator}]");
            generator.Generate(world);
            var end = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Debug.Log($"World generation finished at {end}, took {end - world._start}ms.");
        }
        else
        {
            world.label = data["label"] as string;
            world._seed = (int) data["seed"];
            var rawWorldPos = data["worldPosition"] as string;
            var split = rawWorldPos.Split(","); // We can ignore these warnings as these values are validated earlier.
            world.worldPos = new Vector3Int(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
            world.generator = Generator.GetGenerator(data["generator"] as string);
            world.generator.worldXLength = int.Parse(data["worldXLength"] as string);
            world.generator.worldZLength = int.Parse(data["worldZLength"] as string);
            world.SetupChunks();
            var chunks = data["chunks"] as Dictionary<string, object>;
            foreach (var pair in chunks)
            {
                var position = DataHelpers.ToVector3Int(pair.Key);
                var item = ItemType.Get(pair.Value as string);
                world.SetBlock(item, position.x, position.y, position.z);
            }
            // TODO models, buildings and residents.
        }
        Worlds.Add(world.label, world);
        world._generation = true;
        return world;
    }

    /// <summary>
    /// Gets a world by label.
    /// This world must be loaded for a value to be returned.
    /// </summary>
    /// <param name="label">The label of the world to be retrieved.</param>
    /// <returns>The world by the given name, or null if not present.</returns>
    public static World GetWorld(string label)
    {
        return Worlds.GetValueOrDefault(label, null);
    }

    /// <summary>
    /// Gets all the loaded worlds to be serialised.
    /// </summary>
    /// <returns>Gets the loaded worlds.</returns>
    public static World[] GetWorlds()
    {
        return Worlds.Values.ToArray();
    }

    private const int ChunkRebuildsPerFrame = 2;

    /// <summary>
    /// Applies mesh data generated asynchronously to the chunk object.
    /// </summary>
    /// <param name="chunk">The chunk to apply the mesh too.</param>
    /// <param name="data">The mesh data.</param>
    private static void ApplyMesh(Chunk chunk, MeshData data)
    {
        var mesh = chunk.MeshFilter.mesh ?? new Mesh();
        mesh.Clear();
        mesh.SetVertices(data.Vertices);
        mesh.SetTriangles(data.Triangles, 0);
        mesh.SetUVs(0, data.UVs);
        mesh.SetUVs(1, data.TextureIndices);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        chunk.MeshFilter.mesh = mesh;
        chunk.MeshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// Populates all vertices, triangles, uvs and texture indices lists given.
    /// TODO incorrect uv placement on ZY, YZ and YX planes. To be fixed.
    /// </summary>
    private static void AddQuad(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs,
        List<Vector3> textureIndices,
        ref int vertexIndex, int[] startPos, int[] du, int[] dv, int width, int height, int axis, int textureIndex,
        int direction)
    {
        var normal = new int[3];
        if (direction == 1)
            normal[axis] = 1;
        Vector3 v0 = new(startPos[0] + normal[0], startPos[1] + normal[1], startPos[2] + normal[2]);
        Vector3 v1 = new(startPos[0] + du[0] * width + normal[0],
            startPos[1] + du[1] * width + normal[1],
            startPos[2] + du[2] * width + normal[2]);
        Vector3 v2 = new(startPos[0] + du[0] * width + dv[0] * height + normal[0],
            startPos[1] + du[1] * width + dv[1] * height + normal[1],
            startPos[2] + du[2] * width + dv[2] * height + normal[2]);
        Vector3 v3 = new(startPos[0] + dv[0] * height + normal[0],
            startPos[1] + dv[1] * height + normal[1],
            startPos[2] + dv[2] * height + normal[2]);
        var start = vertexIndex;
        vertices.Add(v0);
        if (direction == 1)
        {
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
        }
        else
        {
            vertices.Add(v3);
            vertices.Add(v2);
            vertices.Add(v1);
        }

        triangles.Add(start + 0);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
        triangles.Add(start + 0);
        triangles.Add(start + 2);
        triangles.Add(start + 3);
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(width, 0));
        uvs.Add(new Vector2(width, height));
        uvs.Add(new Vector2(0, height));
        textureIndices.Add(new Vector3(0, 0, textureIndex));
        textureIndices.Add(new Vector3(1, 0, textureIndex));
        textureIndices.Add(new Vector3(1, 1, textureIndex));
        textureIndices.Add(new Vector3(0, 1, textureIndex));
        vertexIndex += 4;
    }

    /// <summary>
    /// Modulus operation that will not return a value lower than 0. 
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="size">The size of the divider.</param>
    /// <returns></returns>
    private static int Mod(int value, int size)
    {
        var r = value % size;
        return r < 0 ? r + size : r;
    }

    public delegate void WorldLoadEventHandler();
    public event WorldLoadEventHandler OnWorldLoadEvent;

    public string label;
    public Generator generator;
    public Vector3Int worldPos;
    public bool isMainWorld;
    private GameObject _chunkPrefab;
    private readonly Dictionary<Vector2Int, Chunk> _chunks = new();
    private readonly HashSet<Chunk> _dirtyChunks = new();
    private int _seed = -1;
    private bool _loaded;
    private long _start;
    private bool _generation;

    private void Awake()
    {
        if (TextureArrayBuilder.Instance == null)
            new GameObject("TextureArrayBuilder").AddComponent<TextureArrayBuilder>();
        _chunkPrefab = Resources.Load<GameObject>("Prefabs/World/Chunk/Chunk");
    }

    private void Start()
    {
        // Testing World Generation without all deserialization.
        GenerateWorld();
    }

    private int renderDistance = 8;
    private List<Chunk> _renderedChunks = new();
    
    private void GenerateWorld()
    {
        ItemType.Initialize(); // Initialize all of our ItemType values.
        SetupChunks();
        generator.Generate(this);
        var pos = new Vector3Int(100, 30, 100);
        var old = _renderedChunks;
        var chunk = GetChunkAt(pos.x, pos.z);
        if (!chunk.IsLoaded())
            chunk.Load();
        var originPos = chunk.chunkCoord;
        var originX = originPos.x;
        var originZ = originPos.y;
        var chunkPosition = chunk.chunkCoord;
        var xMax = chunkPosition.x + renderDistance;
        var zMax = chunkPosition.y + renderDistance;
        _renderedChunks.Clear();
        var updated = new List<Chunk>();
        for (var x = xMax; x > (originX - renderDistance); x--)
        {
            for (var z = zMax; z > (originZ - renderDistance); z--)
            {
                var found = GetChunk(x, z);
                found.Load();
                _renderedChunks.Add(found);
                updated.Add(found);
            }
        }
        foreach (var c in old)
        {
            foreach (var n in updated)
            {
                if (c.chunkCoord.x == n.chunkCoord.x && c.chunkCoord.y == n.chunkCoord.y)
                {
                    c.Unload();
                    _renderedChunks.Remove(c);
                }
            }
        }
        Debug.Log($"Rendered Chunks[100,150,100] at positions[{string.Join(",", _renderedChunks)}]");
    }

    /// <summary>
    /// Sets the seed of this world if unset.
    /// Will not override a set value, should only be set during generation. 
    /// </summary>
    /// <param name="seed">The seed of this world.</param>
    public void SetSeed(int seed)
    {
        if (_seed != -1)
            return;
        _seed = seed;
    }

    /// <summary>
    /// Gets the seed of this world.
    /// </summary>
    /// <returns>The seed of this world.</returns>
    public int GetSeed()
    {
        return _seed;
    }

    /// <summary>
    /// Sets the type for the block at the given coordinates.
    /// </summary>
    /// <param name="type">The type to set this block too.</param>
    /// <param name="pos">The block position.</param>
    public Block SetBlock(ItemType type, Vector3Int pos)
    {
        return SetBlock(type, pos.x, pos.y, pos.z);
    }

    /// <summary>
    /// Sets the type for the block at the given coordinates.
    /// </summary>
    /// <param name="type">The type to set this block to.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    public Block SetBlock(ItemType type, int x, int y, int z)
    {
        var chunk = GetChunkAt(x, z);
        var localX = Mod(x, Chunk.ChunkSize);
        var localZ = Mod(z, Chunk.ChunkSize);
        var block = new Block(new Vector3Int(x, y, z), this, type);
        chunk.ChunkBlocks[localX, y, localZ] = block;
        MarkChunkDirty(chunk);
        if (localX == 0) MarkChunkDirty(GetChunkAt(x - 1, z));
        if (localX == Chunk.ChunkSize - 1) MarkChunkDirty(GetChunkAt(x + 1, z));
        if (localZ == 0) MarkChunkDirty(GetChunkAt(x, z - 1));
        if (localZ == Chunk.ChunkSize - 1) MarkChunkDirty(GetChunkAt(x, z + 1));
        return block;
    }
    
    /// <summary>
    /// Sets a block to be placed but not mark the chunk as dirty.
    /// This is used for primarily for terrain generation, to not load chunks that aren't being rendered.
    /// </summary>
    /// <param name="type">The type to set this block to.</param>
    /// <param name="pos">The position.</param>
    public void SetUnprocessedBlock(ItemType type, Vector3 pos)
    {
        SetUnprocessedBlock(type, (int) pos.x, (int) pos.y, (int) pos.z);
    }

    /// <summary>
    /// Sets a block to be placed but not mark the chunk as dirty.
    /// This is used for primarily for terrain generation, to not load chunks that aren't being rendered.
    /// </summary>
    /// <param name="type">The type to set this block to.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    public void SetUnprocessedBlock(ItemType type, int x, int y, int z)
    {
        var chunk = GetChunkAt(x, z);
        var localX = Mod(x, Chunk.ChunkSize);
        var localZ = Mod(z, Chunk.ChunkSize);
        var block = new Block(new Vector3Int(x, y, z), this, type);
        chunk.ChunkBlocks[localX, y, localZ] = block;
    }

    /// <summary>
    /// Gets the highest block at the given x and z coordinates.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    /// <returns>The highest block at location, if blocks aren't found at the location, then null.</returns>
    public Block GetHighestAt(int x, int z)
    {
        var highest = Block.GetAirAt(this, x, 0, z);
        var chunk = GetChunkAt(x, z);
        if (chunk == null)
            return highest;
        var localX = x % Chunk.ChunkSize;
        var localZ = z % Chunk.ChunkSize;
        if (localX < 0) localX += Chunk.ChunkSize;
        if (localZ < 0) localZ += Chunk.ChunkSize;
        for (var y = 0; y < Chunk.ChunkHeight; y++)
        {
            var block = chunk.ChunkBlocks[localX, y, localZ];
            if (block != null && !block.Type.Equals(ItemType.Get(ItemType.Keys.Air)) && block.Position.y > highest.Position.y)
                highest = block;
        }

        return highest;
    }

    /// <summary>
    /// Gets the block at the given coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>The block at the given coordinates.</returns>
    public Block GetBlock(int x, int y, int z)
    {
        if (y < 0 || y >= Chunk.ChunkHeight)
            return Block.GetAirAt(this, x, y, z);
        var chunk = GetChunkAt(x, z);
        var localX = x % Chunk.ChunkSize;
        var localZ = z % Chunk.ChunkSize;
        if (localX < 0)
            localX += Chunk.ChunkSize;
        if (localZ < 0)
            localZ += Chunk.ChunkSize;
        var block = chunk.ChunkBlocks[localX, y, localZ];
        return block ?? Block.GetAirAt(this, x, y, z);
    }

    /// <summary>
    /// Checks if a block at the given position is air.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    /// <returns>True if air, else false.</returns>
    public bool IsAir(int x, int y, int z)
    {
        var b = GetBlock(x, y, z);
        return b == null || b.Type.Equals(ItemType.Get(ItemType.Keys.Air));
    }

    /// <summary>
    /// Gets the chunk at a set of world coordinates.
    /// </summary>
    /// <param name="x">The x-coord.</param>
    /// <param name="z">The z-coord.</param>
    /// <returns>The chunk at the coordinates. If this chunk doesn't exist, it will be created.</returns>
    public Chunk GetChunkAt(int x, int z)
    {
        var chunkX = Mathf.FloorToInt((float) x / Chunk.ChunkSize);
        var chunkZ = Mathf.FloorToInt((float) z / Chunk.ChunkSize);
        var coord = new Vector2Int(chunkX, chunkZ);
        if (!_chunks.TryGetValue(coord, out var chunk))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(SetupChunk(coord, chunk));
        }
        return chunk;
    }

    private IEnumerator SetupChunk(Vector2 coord, Chunk chunk)
    {
        var chunkObj = Instantiate(_chunkPrefab, transform);
        chunkObj.transform.SetParent(transform);
        chunk = chunkObj.GetComponent<Chunk>();
        chunk.Init(coord, this);
        _chunks.Add(coord, chunk);
    }

    /// <summary>
    /// Gets a chunk based on a set of CHUNK CO-ORDINATES.
    /// Use GetChunkAt(int x, int z) for world coordinates.
    /// </summary>
    /// <param name="x">The x-coord.</param>
    /// <param name="z">The z-coord.</param>
    /// <returns>The chunk at the coordinate.</returns>
    public Chunk GetChunk(int x, int z)
    {
        var coord = new Vector2Int(x, z);
        if (!_chunks.TryGetValue(coord, out var chunk))
        {
            var chunkObj = Instantiate(_chunkPrefab, transform);
            chunkObj.transform.SetParent(transform);
            chunk = chunkObj.GetComponent<Chunk>();
            chunk.Init(coord, this);
            _chunks.Add(coord, chunk);
        }
        return chunk;
    }

    /// <summary>
    /// Gets all the chunks of this world.
    /// </summary>
    /// <returns>The chunks of this world.</returns>
    public Chunk[] GetChunks()
    {
        return _chunks.Values.ToArray();
    }

    /// <summary>
    /// Marks the given chunk as dirty, to be rebuilt in the next available frame.
    /// </summary>
    /// <param name="chunk">The chunk to mark as dirty.</param>
    public void MarkChunkDirty(Chunk chunk)
    {
        if (chunk != null)
            _dirtyChunks.Add(chunk);
    }

    /// <summary>
    /// Gets the ItemType from given list.
    /// This is for greedy meshing, allowing for fast lookup when a blockface is in the chunk,
    /// but also allows for lookup of blocks that are in the neighbouring chunks, stopping disconnected block faces from appearing.
    /// </summary>
    /// <param name="chunk">The chunk the mesh data is checking for.</param>
    /// <param name="types">The types from the current chunk.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    /// <returns>The type of block at this location.</returns>
    private ItemType GetItemTypeFrom(Chunk chunk, ItemType[,,] types, int x, int y, int z)
    {
        if (y < 0 || y >= Chunk.ChunkHeight)
            return ItemType.Get(ItemType.Keys.Air);
        if (x > 0 && x < Chunk.ChunkSize && z >= 0 && z < Chunk.ChunkSize)
            return types[x, y, z];
        var worldX = chunk.chunkCoord.x * Chunk.ChunkSize + x;
        var worldZ = chunk.chunkCoord.y * Chunk.ChunkSize + z;
        return GetBlock(worldX, y, worldZ)?.Type ?? ItemType.Get(ItemType.Keys.Air);
    }

    // Cleanup test.
    public MeshData BuildMeshData(Chunk chunk)
    {
        // Mesh and Texture Data
        var vertices = new List<Vector3>(); // Corners on blocks in game space.
        var triangles = new List<int>(); // Triangle points on each block.
        var uvs = new List<Vector2>(); // The points to map in the texture array to each vertex.
        var textureIndices = new List<Vector3>(); // The texture index of the current blocks.
        var vertexIndex = 0;

        // Set up an accessible array from the current chunk.
        var types = new ItemType[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];
        for (var x = 0; x < Chunk.ChunkSize; x++)
        for (var y = 0; y < Chunk.ChunkHeight; y++)
        for (var z = 0; z < Chunk.ChunkSize; z++)
            types[x, y, z] =
                chunk.ChunkBlocks[x, y, z]?.Type ??
                ItemType.Get(ItemType.Keys.Air); // If no block is found, default to air.
        var chunkParams = new[] { Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize };
        var pos = new int[3]; // The walker position.
        var faceOffset = new int[3]; // The direction we are checking.
        // Loop over all axes. X-Axis: 0, Y-Axis: 1, Z-Axis: 2
        for (var axis = 0; axis < 3; axis++)
        {
            var axisU = (axis + 1) % 3; // Axis that's on the U, perpendicular.
            var axisV = (axis + 2) % 3; // Axis that's on the V, second perpendicular.
            faceOffset[0] = faceOffset[1] = faceOffset[2] = 0;
            faceOffset[axis] =
                1; // Resets all the faces and sets the current direction we are checking. [e.g +X, +Y, +Z]
            var mask = new (int texture, int direction, int axis)[chunkParams[axisU], chunkParams[axisV]];
            // Moving slice by slice through the chunk on the current axis.
            for (var d = -1; d < chunkParams[axis]; d++)
            {
                pos[axis] = d; // Sets the walkers position to the current block.
                for (pos[axisV] = 0; pos[axisV] < chunkParams[axisV]; pos[axisV]++)
                {
                    for (pos[axisU] = 0; pos[axisU] < chunkParams[axisU]; pos[axisU]++)
                    {
                        var blockA = GetItemTypeFrom(chunk, types, pos[0], pos[1], pos[2]);
                        var blockB = GetItemTypeFrom(chunk, types, pos[0] + faceOffset[0], pos[1] + faceOffset[1],
                            pos[2] + faceOffset[2]);
                        var isASolid = !blockA.Equals(ItemType.Get(ItemType.Keys.Air)) &&
                                       blockA.IsBlock; // If either block is not solid, display a face here.
                        if (isASolid != (!blockB.Equals(ItemType.Get(ItemType.Keys.Air)) && blockB.IsBlock))
                        {
                            var direction = isASolid ? 1 : -1;
                            var texture = (isASolid ? blockA : blockB).GetTextureIndex(axis, direction);
                            mask[pos[axisU], pos[axisV]] = (texture, direction, axis);
                        }
                        else
                            mask[pos[axisU], pos[axisV]] = (-1, 0, axis); // No face necessary.
                    }
                }

                // Greedy Merge Faces
                for (var v = 0; v < chunkParams[axisV]; v++)
                {
                    for (var u = 0; u < chunkParams[axisU];)
                    {
                        var type = mask[u, v];
                        if (type.direction == 0)
                        {
                            u++;
                            continue;
                        }

                        var width = 1; // Check the width. As we know there's a face here, we start at 1.
                        while (u + width < chunkParams[axisU] &&
                               mask[u + width, v].texture == type.texture &&
                               mask[u + width, v].direction == type.direction)
                            width++;
                        var height = 1; // Same logic for height.
                        var stop = false;
                        while (v + height < chunkParams[axisV])
                        {
                            for (var k = 0; k < width; k++)
                            {
                                var other = mask[u + k, v + height];
                                if (other.texture != type.texture || other.direction != type.direction)
                                {
                                    stop = true;
                                    break;
                                }
                            }

                            if (stop) break;
                            height++;
                        }

                        // Assemble into one quad.
                        pos[axisU] = u;
                        pos[axisV] = v;
                        var dirU = new int[3];
                        var dirV = new int[3];
                        dirU[axisU] = 1;
                        dirV[axisV] = 1;
                        var quadPos = new int[3];
                        quadPos[0] = pos[0];
                        quadPos[1] = pos[1];
                        quadPos[2] = pos[2];
                        quadPos[axisU] = u;
                        quadPos[axisV] = v;
                        quadPos[axis] = pos[axis] - (type.direction == -1 ? -1 : 0);
                        AddQuad(vertices, triangles, uvs, textureIndices, ref vertexIndex, quadPos, dirU, dirV, width,
                            height, axis, type.texture,
                            type.direction);
                        // Clear the mask to stop us grouping these faces with other meshes.
                        for (var y = 0; y < height; y++)
                        for (var x = 0; x < width; x++)
                            mask[u + x, v + y] = (-1, 0, axis);
                        u += width;
                    }
                }
            }
        }

        return new MeshData { Vertices = vertices, Triangles = triangles, UVs = uvs, TextureIndices = textureIndices };
    }

    /// <summary>
    /// Checks if this world has been loaded.
    /// </summary>
    /// <returns>True if loaded, else false.</returns>
    public bool IsLoaded()
    {
        return _loaded;
    }

    /// <summary>
    /// Sets the world's loaded status to true.
    /// </summary>
    public void SetLoaded()
    {
        _loaded = true;
    }

    /// <summary>
    /// Marks this world as loaded.
    /// Calls the on world load event.
    /// </summary>
    protected void MarkLoaded()
    {
        _loaded = true;
        OnWorldLoadEvent();
    }

    /// <summary>
    /// Rebuilds the given chunk asynchronously.
    /// </summary>
    /// <param name="chunk">The chunk to rebuild.</param>
    public async Task RebuildChunkAsync(Chunk chunk)
    {
        var meshData = await Task.Run(() => BuildMeshData(chunk));
        ApplyMesh(chunk, meshData);
    }

    private void Update()
    {
        var count = 0;
        foreach (var chunk in _dirtyChunks.ToArray())
        {
            var task = RebuildChunkAsync(chunk);
            task.ContinueWith(_ => _dirtyChunks.Remove(chunk));
            count++;
            if (count >= ChunkRebuildsPerFrame)
                break;
        }
    }

    /// <summary>
    /// Sets up the chunks for this world.
    /// Should only be called internally to lighten the load of terrain generation.
    /// </summary>
    private void SetupChunks()
    {
        for (var x = 0; x <= generator.worldXLength; x += Chunk.ChunkSize) // x
        {
            for (var z = 0; z <= generator.worldZLength; z += Chunk.ChunkSize) // z
            {
                var chunkX = Mathf.FloorToInt((float) x / Chunk.ChunkSize);
                var chunkZ = Mathf.FloorToInt((float) z / Chunk.ChunkSize);
                var coord = new Vector2Int(chunkX, chunkZ);
                Debug.Log($"Generating chunk[{chunkX},{chunkZ}]");
                var chunkObj = Instantiate(_chunkPrefab, transform);
                chunkObj.transform.SetParent(transform);
                var chunk = chunkObj.GetComponent<Chunk>();
                chunk.Init(coord, this);
                _chunks.Add(coord, chunk);
            }
        }
    }

    public Dictionary<string, object> Serialize()
    {
        var chunkData = new Dictionary<string, object>();
        foreach (var pair in _chunks)
            chunkData.Add($"{pair.Key.x},{pair.Key.y}", pair.Value.Serialize());
        var data = new Dictionary<string, object>
        {
            { "label", label },
            { "seed", _seed },
            { "worldPosition", $"{worldPos.x},{worldPos.y},{worldPos.z}" },
            { "generator", Generator.GetName(generator) },
            { "worldXLength", generator.worldXLength },
            { "worldZLength", generator.worldZLength },
            { "chunks", chunkData } // TODO models and houses.
        };
        return data;
    }
    
}