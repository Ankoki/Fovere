using System.Collections.Generic;
using UnityEngine;

namespace Fovere
{
    /// <summary>
    /// Creates an atlas of textures to be used by the voxel engine.
    /// Only one can be present per game. Any future atlases will be destroyed upon creation.
    /// </summary>
    public class TextureAtlas : MonoBehaviour
    {
        private const int TileSize = 256;

        private static TextureAtlas _instance;

        /// <summary>
        /// Gets the atlas created for this game.
        /// </summary>
        /// <returns>The atlas.</returns>
        public static TextureAtlas GetAtlas()
        {
            return _instance;
        }

        public int SpriteStep { get; private set; }
        private int _textureCount;
        private readonly Dictionary<string, Vector2Int> _textureIndices = new();
        public Texture2D TextureMap { get; private set; }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BuildAtlas();
        }

        /// <summary>
        /// Builds the texture atlas from all textures found in the BlockTextures resource path.
        /// </summary>
        private void BuildAtlas()
        {
            var textures = Resources.LoadAll<Texture2D>("BlockTextures");
            _textureCount = textures.Length;
            SpriteStep = Mathf.FloorToInt(Mathf.Sqrt(_textureCount)) + 1;
            var texture2d = new Texture2D(SpriteStep * TileSize, SpriteStep * TileSize, TextureFormat.RGBA32, false);
            var i = -1;
            for (var x = 0; x < SpriteStep; x++)
            {
                for (var y = 0; y < SpriteStep; y++)
                {
                    i++;
                    if (i >= _textureCount)
                        break;
                    var texture = textures[i];
                    Graphics.CopyTexture(texture, 0, 0, 0, 0, TileSize, TileSize, texture2d, 0, 0,
                        x * TileSize, y * TileSize);
                    var position = new Vector2Int(x, y);
                    var label = "fovere:" + texture.name.ToLower().Trim();
                    _textureIndices.Add(label, position);
                }
            }

            TextureMap = texture2d;
        }

        /// <summary>
        /// Gets the index in the atlas of a texture key.
        /// </summary>
        /// <param name="key">The key of the texture.</param>
        /// <returns>The texture index, or 0, 0[Air] if not found.</returns>
        public Vector2Int GetIndex(string key)
        {
            if (!_textureIndices.ContainsKey(key))
                Debug.Log($"Texture[{key}] not found.");
            return _textureIndices.GetValueOrDefault(key,
                new Vector2Int(0, 0)); // 0, 0 will be the position of the air texture.
        }

    }
}