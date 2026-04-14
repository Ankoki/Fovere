using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> vertices = new();
    public List<int> triangles = new();
    public List<Vector2> uvs = new();
    public List<Vector3> textureIndices = new();
}