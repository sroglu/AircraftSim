using System.IO;
using UnityEngine;

public class SurfaceLayerLoader : TerrainLayerLoader
{
    Mesh tileMesh;
    byte[] vertices, indices;
    public SurfaceLayerLoader(string dataFolderName, TerrainLayerLoader nextLoader = null) : base(TerrainLayer.surface, dataFolderName, nextLoader)
    {
    }

    protected override bool CheckSegmentCanBeLoaded(TerrainSegment segment)
    {
        return File.Exists(DataPath(segment.representation + "." + Extentions.vert)) && File.Exists(DataPath(segment.representation + "." + Extentions.ind));
    }

    protected override bool LoadLayer(TerrainSegment segment,float LOD, ref Transform tile)
    {
        MeshFilter meshFilter = tile.gameObject.GetComponent<MeshFilter>();
        tileMesh = meshFilter.mesh;
        vertices = System.IO.File.ReadAllBytes(DataPath(segment.representation + "." + Extentions.vert));
        indices = System.IO.File.ReadAllBytes(DataPath(segment.representation + "." + Extentions.ind));
        // specify vertex count and layout
        var layout = new[]
        {
                    new UnityEngine.Rendering.VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, UnityEngine.Rendering.VertexAttributeFormat.Float32, 3),
                    new UnityEngine.Rendering.VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Normal, UnityEngine.Rendering.VertexAttributeFormat.Float32, 3),
                    new UnityEngine.Rendering.VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.TexCoord0, UnityEngine.Rendering.VertexAttributeFormat.Float32, 2),
                };
        var vertexCount = vertices.Length / 32;

        tileMesh.SetVertexBufferParams(vertexCount, layout);

        // set vertex data
        var verts = new Unity.Collections.NativeArray<Vertex>(vertexCount, Unity.Collections.Allocator.Temp);


        // ... fill in vertex array data here...
        TAI.Tool.NativeArrayExtension.CopyFromRawBytes<Vertex>(verts, vertices);
        tileMesh.SetVertexBufferData(verts, 0, 0, vertexCount);

        var IndexCount = indices.Length / 2;

        // set index buffer
        tileMesh.SetIndexBufferParams(IndexCount, UnityEngine.Rendering.IndexFormat.UInt16);
        var idx = new Unity.Collections.NativeArray<short>(IndexCount, Unity.Collections.Allocator.Temp);

        TAI.Tool.NativeArrayExtension.CopyFromRawBytes<short>(idx, indices);
        tileMesh.SetIndexBufferData(idx, 0, 0, IndexCount, UnityEngine.Rendering.MeshUpdateFlags.DontValidateIndices);

        tileMesh.subMeshCount = 1;
        tileMesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, IndexCount, MeshTopology.Triangles));

        // Correct bounding volume, without this, mesh can't be rendered properly in some angles - distances
        tileMesh.RecalculateBounds();

        meshFilter.mesh = tileMesh;

        Debug.Log(DataPath(segment.representation + "." + Extentions.vert));
        return true;
    }




    // Vertex with FP32 position, FP16 2D normal and a 4-byte tangent.
    // In some cases StructLayout attribute needs
    // to be used, to get the data layout match exactly what it needs to be.
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct Vertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uv;
    }


}

