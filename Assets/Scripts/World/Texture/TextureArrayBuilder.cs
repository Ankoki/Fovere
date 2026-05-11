using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to build the texture array used by the shader.
/// Allows for block types generated procedurally. 
/// </summary>
public class TextureArrayBuilder : MonoBehaviour
{
    public static TextureArrayBuilder Instance;

    public Texture2DArray textureArray;
    public int textureCount;
    private readonly Dictionary<string, int> _textureIndices = new();
    private const int TileSize = 256;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Build();
    }

    /// <summary>
    /// Builds the texture array from our BlockTextures resource file.
    /// </summary>
    private void Build()
    {
        var textures = Resources.LoadAll<Texture2D>("BlockTextures");
        textureCount = textures.Length;
        textureArray = new(TileSize, TileSize, textureCount, TextureFormat.RGBA32, false);
        textureArray.filterMode = FilterMode.Point;
        textureArray.wrapMode = TextureWrapMode.Repeat;
        for (var i = 0; i < textures.Length; i++)
        {
            var tex = textures[i];
            Graphics.CopyTexture(tex, 0, 0, textureArray, i, 0);
            var name = "fovere:" + tex.name.ToLower().Trim();
            _textureIndices[name] = i;
            Debug.Log($"Texture[{name}={i}]");
        }
    }

    /// <summary>
    /// Gets the index of a block key in the texture atlas.
    /// </summary>
    /// <param name="blockKey">The key of the block to search for.</param>
    /// <returns>The block index, or 0 if not found.</returns>
    public int GetIndex(string blockKey)
    {
        blockKey = blockKey.ToLower().Trim();
        if (_textureIndices.TryGetValue(blockKey, out var i))
            return i;
        Debug.LogWarning($"Texture not found: {blockKey}");
        return 0;
    }

}