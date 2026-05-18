using System.Collections.Generic;
using UnityEngine;

namespace Fovere
{
    /// <summary>
    /// Utility class used to store texture and model data for chunk meshes.
    /// </summary>
    public class MeshData
    {

        public List<Vector3> Vertices = new();
        public List<int> Triangles = new();
        public List<Vector2> UVs = new();
        public List<Vector3> TextureIndices = new();

    }
}