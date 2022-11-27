using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class MapSection : IComparable<MapSection>
{
    public int latitude;
    public int latitudeIdx;
    public int longitude;
    public int longitudeIdx;
    public string representation;
    // May be duplicated, move to a common place
    //public static string terrainDataPath = "D:\\Projects\\aircraftvr\\Data\\";
    public Transform Section;
    public Transform BuildingSection;
    public Texture2D usedTexture = null;
    public TerrainManager.ZoomLevel zoom;
    public Vector3 basePos;

    public MapSection(float lat, float lon)
    {
        (latitude, latitudeIdx, longitude, longitudeIdx) = Indexer.ObjIdxFromCoords(lat, lon);

        representation = $"{latitude}_{latitudeIdx}_{longitude}_{longitudeIdx}";
        Section = null;
        zoom = TerrainManager.ZoomLevel.Invalid;
    }

    public MapSection(int lat, int latIdx, int lon, int lonIdx)
    {
        latitude = lat;
        latitudeIdx = latIdx;
        longitude = lon;
        longitudeIdx = lonIdx;
        representation = $"{latitude}_{latitudeIdx}_{longitude}_{longitudeIdx}";
        Section = null;
        zoom = TerrainManager.ZoomLevel.Invalid;
    }

    public MapSection(string sectName)
    {
        (latitude, latitudeIdx, longitude, longitudeIdx) = Utility.SplitName(sectName);
        representation = $"{latitude}_{latitudeIdx}_{longitude}_{longitudeIdx}";
        Section = null;
        zoom = TerrainManager.ZoomLevel.Invalid;
    }

    public MapSection IncrementLongitude()
    {
        if(longitudeIdx == Constants.DivisorOfOneDeg)
        {
            return new MapSection(latitude, latitudeIdx, longitude + 1, 1);
        }
        else
        {
            return new MapSection(latitude, latitudeIdx, longitude, longitudeIdx + 1);
        }
    }

    public MapSection DecrementLongitude()
    {
        if (longitudeIdx == 1)
        {
            return new MapSection(latitude, latitudeIdx, longitude - 1, Constants.DivisorOfOneDeg);
        }
        else
        {
            return new MapSection(latitude, latitudeIdx, longitude, longitudeIdx - 1);
        }
    }

    public MapSection IncrementLatitude()
    {
        if (latitudeIdx == Constants.DivisorOfOneDeg)
        {
            return new MapSection(latitude + 1, 1, longitude, longitudeIdx);
        }
        else
        {
            return new MapSection(latitude, latitudeIdx + 1, longitude, longitudeIdx);
        }
    }

    public MapSection DecrementLatitude()
    {
        if (latitudeIdx == 1)
        {
            return new MapSection(latitude - 1, Constants.DivisorOfOneDeg, longitude, longitudeIdx);
        }
        else
        {
            return new MapSection(latitude, latitudeIdx - 1, longitude, longitudeIdx);
        }
    }

    public bool LoadMesh(Material material, float acAltitude, Vector3 acBasePos)
    {
        string vertPath = $"{TerrainManager.Instance.terrainDataPath}/TerrainModel/{representation}.vert";
        string indPath = $"{TerrainManager.Instance.terrainDataPath}/TerrainModel/{representation}.ind";
        byte[] vertices;
        byte[] indices;

        if (File.Exists(vertPath) && File.Exists(indPath))
        {
            vertices = System.IO.File.ReadAllBytes(vertPath);
            indices = System.IO.File.ReadAllBytes(indPath);
            // Create game object for mesh to attach
            GameObject terrObj = new GameObject();
            Section = terrObj.transform;
            // Give object a meaningful name
            terrObj.name = representation;

            // Attach a mesh renderer and mesh filter to game object
            MeshRenderer meshRenderer = terrObj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            MeshFilter meshFilter = terrObj.AddComponent<MeshFilter>();

            // Then create the mesh
            Mesh mesh = new Mesh();
            //Debug.Log($"Mesh name is {mesh.name}");
            // specify vertex count and layout
            var layout = new[]
            {
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                };
            var vertexCount = vertices.Length / 32;
            mesh.SetVertexBufferParams(vertexCount, layout);

            // set vertex data
            var verts = new NativeArray<Vertex>(vertexCount, Allocator.Temp);

            // ... fill in vertex array data here...
            TAI.Tool.NativeArrayExtension.CopyFromRawBytes<Vertex>(verts, vertices);
            mesh.SetVertexBufferData(verts, 0, 0, vertexCount);

            var IndexCount = indices.Length / 2;

            // set index buffer
            mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt16);
            var idx = new NativeArray<short>(IndexCount, Allocator.Temp);

            TAI.Tool.NativeArrayExtension.CopyFromRawBytes<short>(idx, indices);
            mesh.SetIndexBufferData(idx, 0, 0, IndexCount, MeshUpdateFlags.DontValidateIndices);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, IndexCount, MeshTopology.Triangles));

            // Correct bounding volume, without this, mesh can't be rendered properly in some angles - distances
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            // Move the mesh into correct position
            Vector3 basePos = Utility.CalcBasePos(latitude, longitude, acAltitude);
            // Do I need this?
            terrObj.transform.position = basePos - acBasePos;

            return true;
        }
        else
        {
            //Debug.LogError($".vert or .ind file cannot be found for terrain {representation} - path : {vertPath}");
            return false;
        }
    }

    public bool LoadBuildingMesh(Material buildingMaterial,float acAltitude)
    {

        string buildingDataPath = $"{TerrainManager.Instance.terrainDataPath}/BuildingModel/{representation}.buildingdata";

        byte[] buildingsMeshRaw;

        if (File.Exists(buildingDataPath))
        {
            buildingsMeshRaw = File.ReadAllBytes(buildingDataPath);

            int vertexNum = BitConverter.ToInt32(buildingsMeshRaw.Take(sizeof(int)).ToArray(), 0);

            //Debug.Log("vertexNum: " + vertexNum);

            // set vertex data
            var vertsRaw = new NativeArray<Vector3>(vertexNum, Allocator.Temp);

            //// ... fill in vertex array data here...
            TAI.Tool.NativeArrayExtension.CopyFromRawBytes<Vector3>(vertsRaw, buildingsMeshRaw.Skip(4).Take(vertexNum * 3 * sizeof(float)).ToArray());

            var trisRawArr = buildingsMeshRaw.Skip(sizeof(int) + vertexNum * 3 * sizeof(float)).ToArray();

            int[] tris = new int[(trisRawArr.Length / sizeof(int))];
            Buffer.BlockCopy(trisRawArr, 0, tris, 0, trisRawArr.Length);



            //CreateBuildingMesh(verts, tris);

            GameObject buildingObj = new GameObject();
            buildingObj.transform.parent = Section;
            buildingObj.transform.localPosition = Vector3.zero;
            buildingObj.transform.localRotation= Quaternion.identity;
            buildingObj.name = representation+"_Buildings";
            BuildingSection = buildingObj.transform;

            // Attach a mesh renderer and mesh filter to game object
            MeshRenderer meshRenderer = buildingObj.AddComponent<MeshRenderer>();
            meshRenderer.material = buildingMaterial;
            MeshFilter meshFilter = buildingObj.AddComponent<MeshFilter>();

            // Then create the mesh
            Mesh mesh = new Mesh();

            // specify vertex count and layout
            var layout = new[]
            {
                new VertexAttributeDescriptor(UnityEngine.Rendering.VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            };

            mesh.SetVertexBufferParams(vertsRaw.Length, layout);
            mesh.SetVertexBufferData(vertsRaw, 0, 0, vertsRaw.Length);
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, 0, MeshTopology.Points));

            mesh.triangles = tris;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;

            buildingObj.transform.localRotation = Quaternion.Euler(0, -longitude, latitude) * Quaternion.AngleAxis(-90, Vector3.forward) * Quaternion.AngleAxis(-90, Vector3.up);

            buildingObj.transform.localPosition = Utility.CalcBasePos(latitude + ((float)(latitudeIdx - 1) / 6) + (1f / 12f), longitude + ((float)(longitudeIdx - 1) / 6) + (1f / 12f), acAltitude)
                                                - Utility.CalcBasePos(latitude, longitude, acAltitude);
            return true;
        }
        else
        {
            //Debug.LogError($".buildingdata file cannot be found for terrain {representation} - path : {buildingDataPath}");
            return false;
        }
    }

    public int CompareTo(MapSection other)
    {
        int latDiff = latitude - other.latitude;
        int lonDiff = longitude - other.longitude;

        int latIdxDiff = latitudeIdx - other.latitudeIdx;
        int lonIdxDiff = longitudeIdx - other.longitudeIdx;

        if (latDiff == 0 && lonDiff == 0)
        {
            if (latIdxDiff > 0 || lonIdxDiff > 0)
                return 1;
            if (latIdxDiff == 0 && lonIdxDiff == 0)
                return 0;
            return -1;
        }

        if (latDiff > 0 || lonDiff > 0)
            return 1;
        if (latDiff < 0 && lonDiff < 0)
            return -1;
        // One is 0 other one is negative
        if (latDiff == 0)
            return latIdxDiff;
        if (lonDiff == 0)
            return lonIdxDiff;
        return 0;
    }

    public int SubtractLatitude(MapSection other)
    {
        int latDiff = this.latitudeIdx - other.latitudeIdx;
        latDiff += (this.latitude - other.latitude) * 6;

        return latDiff;
    }

    public int SubtractLongitude(MapSection other)
    {
        int lonDiff = this.longitudeIdx - other.longitudeIdx;
        lonDiff += (this.longitude - other.longitude) * 6;

        return lonDiff;
    }

    public bool IsNeighbor(MapSection other)
    {
        return (Math.Abs(SubtractLatitude(other)) <= 1) && (Math.Abs(SubtractLongitude(other)) <= 1);
    }

    public bool IsNeighbor(String otherRepresentation)
    {
        return IsNeighbor(new MapSection(otherRepresentation));
    }

    // Define the is greater than operator.
    public static bool operator >(MapSection ms1, MapSection ms2)
    {
        // Compare to only works for < and <=, workaround needed
        return ms2.CompareTo(ms1) < 0;
    }

    // Define the is less than operator.
    public static bool operator <(MapSection ms1, MapSection ms2)
    {
        return ms1.CompareTo(ms2) < 0;
    }

    // Define the is greater than or equal to operator.
    public static bool operator >=(MapSection ms1, MapSection ms2)
    {
        // Compare to only works for < and <=, workaround needed
        return ms2.CompareTo(ms1) <= 0;
    }

    // Define the is less than or equal to operator.
    public static bool operator <=(MapSection ms1, MapSection ms2)
    {
        return ms1.CompareTo(ms2) <= 0;
    }

    // Define the equal to operator.
    public static bool operator ==(MapSection ms1, MapSection ms2)
    {
        return ms1.CompareTo(ms2) == 0;
    }

    // Define the not equal to operator.
    public static bool operator !=(MapSection ms1, MapSection ms2)
    {
        return ms1.CompareTo(ms2) != 0;
    }

    public override string ToString()
    {
        return representation;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
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
