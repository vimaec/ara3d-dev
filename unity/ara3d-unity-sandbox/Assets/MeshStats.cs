using UnityEngine;
using Ara3D;

public class MeshStats : MonoBehaviour
{
    public int NumVertices;
    public int NumTriangles;
    public bool HasFilter;
    public bool HasRenderer;
    public int NumSubMeshes;
    public string IndexFormat;
    public bool IsReadable;

    public void Update()
    {
        //OnValidate();
    }

    public void OnValidate()
    {
        var mesh = this.GetMesh();
        if (mesh != null)
        {
            NumVertices = mesh.vertexCount;
            NumSubMeshes = mesh.subMeshCount;
            IndexFormat = mesh.indexFormat.ToString();
            HasFilter = this.GetComponent<MeshFilter>() != null;
            HasRenderer = this.GetComponent<MeshRenderer>() != null;
            IsReadable = mesh.isReadable;
            NumTriangles = mesh.triangles.Length;
        }
    }
}
