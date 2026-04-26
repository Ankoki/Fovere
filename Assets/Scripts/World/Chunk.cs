using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{

    public static Chunk Deserialize(Dictionary<string, string> data)
    {
        return null; // TODO
    }
    
    public const int ChunkSize = 16;
    public const int ChunkHeight = 32;
    public const int TileSize = 256;

    public Vector2Int ChunkCoord;
    public Block[,,] ChunkBlocks;
    public MeshFilter MeshFilter => _meshFilter;
    public MeshCollider MeshCollider => _meshCollider;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;

    public void Init(Vector2Int position)
    {
        ChunkCoord = position;
        ChunkBlocks = new Block[ChunkSize, ChunkHeight, ChunkSize];
        _meshFilter = gameObject.GetComponent<MeshFilter>();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Custom/BlockTextureArray"));
        mat.SetTexture("_MainTex", TextureArrayBuilder.Instance.TextureArray);
        _meshRenderer.material = mat;
        _meshCollider = gameObject.GetComponent<MeshCollider>();
        transform.position = new Vector3(
            ChunkCoord.x * ChunkSize,
            0,
            ChunkCoord.y * ChunkSize
        );
    }
    
    public Dictionary<string, object> Serialize()
    {
        var dictionary = new Dictionary<string, object>();
        foreach (var block in ChunkBlocks)
        {
            if (block == null || block.Type == ItemType.Air)
                continue;
            dictionary.Add(FromVector(ToWorld(block.Position)), block.Type.key);
        }
        return dictionary;
    }

    private Vector3Int ToWorld(Vector3Int position)
    {
        position.x = (ChunkCoord.x * ChunkSize) + position.x;
        position.z = (ChunkCoord.y * ChunkSize) + position.z;
        return position;
    }

    private static string FromVector(Vector3Int vector)
    {
        return $"{vector.x},{vector.y},{vector.z}";
    }
    
}