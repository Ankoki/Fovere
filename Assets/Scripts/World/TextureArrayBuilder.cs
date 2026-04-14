using System.Collections.Generic;
using UnityEngine;

public class TextureArrayBuilder : MonoBehaviour
{
    public static TextureArrayBuilder Instance;

    public Texture2DArray TextureArray;
    public int TextureCount;

    private Dictionary<string, int> _textureIndices = new();

    private const int TileSize = 256;

    private void Awake()
    {
        Instance = this;
        Build();
    }

    private void Build()
    {
        var textures = Resources.LoadAll<Texture2D>("BlockTextures");

        TextureCount = textures.Length;
        TextureArray = new Texture2DArray(TileSize, TileSize, TextureCount, TextureFormat.RGBA32, false);
        TextureArray.filterMode = FilterMode.Point;
        TextureArray.wrapMode = TextureWrapMode.Repeat;

        for (var i = 0; i < textures.Length; i++)
        {
            var tex = textures[i];

            Graphics.CopyTexture(tex, 0, 0, TextureArray, i, 0);

            // 🔑 CRITICAL: store mapping
            var name = tex.name.ToLower().Trim();
            _textureIndices[name] = i;

            Debug.Log($"Mapped {name} → {i}");
        }
    }

    public int GetIndex(string blockName)
    {
        blockName = blockName.ToLower().Trim();

        if (_textureIndices.TryGetValue(blockName, out var i))
            return i;

        Debug.LogWarning($"Texture not found: {blockName}");
        return 0;
    }

}