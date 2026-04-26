using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PlayFab.Json;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Class to store all block and procedural generation data for given worlds.
/// Multiple worlds can be used and isolated, with different locations for each.
/// </summary>
// TODO fix texturing. all greedy meshing on the horizontal plane is correct, details found in page 28 of PB.
[PublicAPI]
public class World : MonoBehaviour
{
    private static readonly Dictionary<string, World> Worlds = new();

    public static string SerializeWorlds()
    {
        // Using PlayFab JSON objects.
        var root = new JsonObject();
        var worlds = new JsonObject();
        foreach (var world in Worlds.Values)
        {
            var current = new JsonObject();
            current.Add("worldXLength", world.generator.worldXLength);
            current.Add("worldYLength", world.generator.worldYLength);
            current.Add("worldZLength", world.generator.worldZLength);
            foreach (var chunk in world._chunks.Values)
            {
                var blocks = chunk.Serialize();
                foreach (var block in blocks)
                    current.Add(block.Key, block.Value);
            }
            worlds.Add(world.label, current);
        }
        root.Add("worlds", worlds);
        return root.ToString();
    }

    /// <summary>
    /// Gets a world by name.
    /// This world must be loaded for a value to be returned.
    /// </summary>
    /// <param name="name">The name of the world to be retrieved.</param>
    /// <returns></returns>
    public static World GetWorld(string name)
    {
        return Worlds[name];
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

        mesh.SetVertices(data.vertices);
        mesh.SetTriangles(data.triangles, 0);
        mesh.SetUVs(0, data.uvs);
        mesh.SetUVs(1, data.textureIndices);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        chunk.MeshFilter.mesh = mesh;
        chunk.MeshCollider.sharedMesh = mesh;
    }
    
    private static void AddQuad(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> textureIndices,
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
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private static int Mod(int value, int size)
    {
        var r = value % size;
        return r < 0 ? r + size : r;
    }

    public string label;
    public Generator generator;
    public Vector3Int worldPos;
    public bool isMainWorld;
    public GameObject playerPrefab; // Should be present if isMainWorld is true. TODO is this a good way of doing it lol
    private GameObject _chunkPrefab;
    private readonly Dictionary<Vector2Int, Chunk> _chunks = new();
    private readonly HashSet<Chunk> _dirtyChunks = new();

    private void Awake()
    {
        if (TextureArrayBuilder.Instance == null)
            new GameObject("TextureArrayBuilder").AddComponent<TextureArrayBuilder>();
    }

    // TODO the way this is handled will need to be changed. map data will be retrieved from database per person.
    private void Start()
    {
        _chunkPrefab = Resources.Load<GameObject>("Prefabs/World/Chunk/Chunk");
        generator.Generate(this);
        Worlds.Add(label, this);
        Debug.Log($"World {label} generated.");
        if (!isMainWorld)
            return;
        if (playerPrefab == null)
        {
            Debug.Log("playerPrefab is null, no player will be spawned.");
            return;
        }
        var spawned = Instantiate(playerPrefab, transform);
        var cameraController = spawned.GetComponent<CameraController>();
        cameraController.standardCamera = GameObject.Find("CMStandard").GetComponent<CinemachineCamera>();
        cameraController.zoomCamera = GameObject.Find("CMZoom").GetComponent<CinemachineCamera>();
        cameraController.obstructedViewCamera = GameObject.Find("CMObstructed").GetComponent<CinemachineCamera>();
        var cameraTarget = GameObject.Find("CameraTarget");
        cameraController.standardCamera.Follow = cameraTarget.transform;
        cameraController.zoomCamera.Follow = cameraTarget.transform;
        cameraController.obstructedViewCamera.Follow = cameraTarget.transform;
        spawned.GetComponent<PlayerHandler>().SafeTeleport(this, generator.worldXLength / 2f, generator.worldZLength / 2f);
        Debug.Log($"World Serialize test={SerializeWorlds()}");
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
        for (var y = 0; y < generator.worldYLength; y++)
        {
            var block = chunk.ChunkBlocks[localX, y, localZ];
            if (block != null && block.Type != ItemType.Air && block.Position.y > highest.Position.y)
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
        return b == null || b.Type == ItemType.Air;
    }

    /// <summary>
    /// Gets the chunk at a specific set of coordinates.
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
            var chunkObj = Instantiate(_chunkPrefab, transform);
            chunk = chunkObj.GetComponent<Chunk>();
            chunk.Init(coord);

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
            return ItemType.Air;
        if (x < 0 || x >= Chunk.ChunkSize || z < 0 || z >= Chunk.ChunkSize)
        {
            var worldX = chunk.ChunkCoord.x * Chunk.ChunkSize + x;
            var worldZ = chunk.ChunkCoord.y * Chunk.ChunkSize + z;
            return GetBlock(worldX, y, worldZ)?.Type ?? ItemType.Air;
        }
        return types[x, y, z];
    }

    private ItemType GetItem(ItemType[,,] types, Chunk chunk, int x, int y, int z)
    {
        if (y < 0 || y >= Chunk.ChunkHeight)
            return ItemType.Air;
        var worldX = chunk.ChunkCoord.x * Chunk.ChunkSize + x;
        var worldZ = chunk.ChunkCoord.y * Chunk.ChunkSize + z;
        return GetBlock(worldX, y, worldZ)?.Type ?? ItemType.Air;
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
                ItemType.Air; // If no block is found, default to air.
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
                        var isASolid = blockA != ItemType.Air &&
                                       blockA.isBlock; // If either block is not solid, display a face here.
                        if (isASolid != (blockB != ItemType.Air && blockB.isBlock))
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
                        } // Assemble into one big quad.

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

        return new MeshData { vertices = vertices, triangles = triangles, uvs = uvs, textureIndices = textureIndices };
    }

    /// <summary> /// Adds all vertices and triangles for the given block. /// </summary> private void AddQuad( List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> textureIndices, ref int vertexIndex, int[] startPos, int[] du, int[] dv, int width, int height, int axis, int textureIndex, int direction) { var normal = new int[3]; if (direction == 1) normal[axis] = 1; Vector3 v0 = new( startPos[0] + normal[0], startPos[1] + normal[1], startPos[2] + normal[2]); Vector3 v1 = new( startPos[0] + du[0] * width + normal[0], startPos[1] + du[1] * width + normal[1], startPos[2] + du[2] * width + normal[2]); Vector3 v2 = new( startPos[0] + du[0] * width + dv[0] * height + normal[0], startPos[1] + du[1] * width + dv[1] * height + normal[1], startPos[2] + du[2] * width + dv[2] * height + normal[2]); Vector3 v3 = new( startPos[0] + dv[0] * height + normal[0], startPos[1] + dv[1] * height + normal[1], startPos[2] + dv[2] * height + normal[2]); var start = vertexIndex; vertices.Add(v0); if (direction == 1) { vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); } else { vertices.Add(v3); vertices.Add(v2); vertices.Add(v1); } triangles.Add(start + 0); triangles.Add(start + 1); triangles.Add(start + 2); triangles.Add(start + 0); triangles.Add(start + 2); triangles.Add(start + 3); uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(width, 0)); uvs.Add(new Vector2(width, height)); uvs.Add(new Vector2(0, height)); textureIndices.Add(new Vector3(0, 0, textureIndex)); textureIndices.Add(new Vector3(1, 0, textureIndex)); textureIndices.Add(new Vector3(1, 1, textureIndex)); textureIndices.Add(new Vector3(0, 1, textureIndex)); vertexIndex += 4; }
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
    
}