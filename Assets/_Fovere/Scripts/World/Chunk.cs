using System.Collections.Generic;
using UnityEngine;

namespace Fovere
{
    /// <summary>
    /// Class which stores all the data necessary for block, mesh and location storage
    /// for a chunk. Used to make up worlds and use greedy meshing to benefit performance.
    /// TODO Will later store model and structure data.
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        /// <summary>
        /// Deserializes chunk data into the chunk object.
        /// </summary>
        /// <param name="data">The data of the chunk.</param>
        /// <returns></returns>
        public static Chunk Deserialize(Dictionary<string, string> data)
        {
            return null; // TODO
        }

        public const int ChunkSize = 16;
        public const int ChunkHeight = 256;

        public Vector2Int chunkCoord;
        public Block[,,] ChunkBlocks;
        public World world;
        public MeshFilter MeshFilter { get; private set; }
        public MeshCollider MeshCollider { get; private set; }
        private MeshRenderer _meshRenderer;
        private bool _loaded;

        /// <summary>
        /// Initialises this chunk at the given position.
        /// </summary>
        /// <param name="position">The chunk position.</param>
        /// <param name="world">The world this chunk is in.</param>
        public void Init(Vector2Int position, World world)
        {
            chunkCoord = position;
            this.world = world;
            ChunkBlocks = new Block[ChunkSize, ChunkHeight, ChunkSize];
            MeshFilter = gameObject.GetComponent<MeshFilter>();
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Custom/BlockTextureArray"));
            mat.SetTexture("_MainTex", TextureAtlas.GetAtlas().TextureMap);
            _meshRenderer.material = mat;
            MeshCollider = gameObject.GetComponent<MeshCollider>();
            transform.position = new Vector3(
                chunkCoord.x * ChunkSize,
                0,
                chunkCoord.y * ChunkSize
            );
        }

        /// <summary>
        /// Serializes' the chunk data into a dictionary used for storage.
        /// Keeps the position of the blocks relative to the chunks.
        /// </summary>
        /// <returns>The serializes chunk data.</returns>
        public Dictionary<string, object> Serialize()
        {
            var data = new Dictionary<string, object>();
            var blockData = new Dictionary<string, object>();
            foreach (var block in ChunkBlocks)
            {
                if (block == null || block.Type == ItemType.Get(ItemType.Keys.Air))
                    continue;
                blockData.Add(DataHelpers.FromVector3(block.Position), block.Type.Key);
            }

            data.Add("blockData", blockData);
            return data;
        }

        /// <summary>
        /// Converts a position from a chunk position to a world position.
        /// </summary>
        /// <param name="position">The position to convert.</param>
        /// <returns>The converted position</returns>
        private Vector3Int ToWorld(Vector3Int position)
        {
            position.x = (chunkCoord.x * ChunkSize) + position.x;
            position.z = (chunkCoord.y * ChunkSize) + position.z;
            return position;
        }

        /// <summary>
        /// Serializes a vector into a string.
        /// </summary>
        /// <param name="vector">The vector to serialize.</param>
        /// <returns></returns>
        private string FromVector(Vector3Int vector)
        {
            return $"{vector.x},{vector.y},{vector.z}";
        }

        /// <summary>
        /// Loads this chunk.
        /// </summary>
        public async void Load()
        {
            await world.RebuildChunkAsync(this);
            _loaded = true;
        }

        /// <summary>
        /// Checks if this chunk is loaded.
        /// </summary>
        /// <returns>True if is loaded, else false.</returns>
        public bool IsLoaded()
        {
            return _loaded;
        }

        /// <summary>
        /// Unloads a chunk.
        /// </summary>
        public void Unload()
        {
            _loaded = false;
            MeshFilter.mesh = null;
            MeshCollider.sharedMesh = null;
        }

        public override string ToString()
        {
            return $"Chunk[@{chunkCoord.x},{chunkCoord.y}]";
        }
    }
}