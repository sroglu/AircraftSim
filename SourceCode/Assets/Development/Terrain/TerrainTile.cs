using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainSegment
{
    public int lat;
    public int latIdx;
    public int lon;
    public int lonIdx;

    public string representation;

    public TerrainSegment((int, int, int, int) value) : this(value.Item1, value.Item2, value.Item3, value.Item4)
    {
    }

    public TerrainSegment(int lat, int latIdx, int lon, int lonIdx)
    {
        this.lat = lat;
        this.latIdx = latIdx;
        this.lon = lon;
        this.lonIdx = lonIdx;

        this.representation = $"{lat}_{latIdx}_{lon}_{lonIdx}";
    }

    public List<TerrainSegment> GetSegmentsTo(TerrainSegment segment)
    {
        List<TerrainSegment> midSegments = new List<TerrainSegment>();

        int latDir = IsLatAxisBiggerThan(this, segment) ? -1 : 1;
        int lonDir = IsLonAxisBiggerThan(this, segment) ? -1 : 1;

        int latCount = Math.Abs(lat - segment.lat) * Constants.DivisorOfOneDeg + Math.Abs(latIdx - segment.latIdx) * latDir;
        int lonCount = Math.Abs(lon - segment.lon) * Constants.DivisorOfOneDeg + Math.Abs(lonIdx - segment.lonIdx) * lonDir;

        for (int i = 0; i <= latCount; i++)
        {
            for (int j = 0; j <= lonCount; j++)
            {
                if (i == 0 && j == 0 || i == latCount && j == lonCount) continue;

                midSegments.Add(
                    new TerrainSegment(
                        lat + (i * latDir / Constants.DivisorOfOneDeg),
                        (latIdx - 1 + i + Constants.DivisorOfOneDeg * latDir) % Constants.DivisorOfOneDeg + 1,
                        lon + ((j * lonDir) / Constants.DivisorOfOneDeg),
                        (lonIdx - 1 + j + Constants.DivisorOfOneDeg * lonDir) % Constants.DivisorOfOneDeg + 1
                    ));

                //if (midSegments[midSegments.Count-1].latIdx<1 || midSegments[midSegments.Count - 1].lonIdx < 1)
                //{
                //    Debug.Log("Errorrr");
                //}

            }

        }

        return midSegments;
    }


    public static bool IsLatAxisBiggerThan(TerrainSegment segment, TerrainSegment otherSegment)
    {
        return segment.lat > otherSegment.lat ? true : segment.latIdx > otherSegment.latIdx;
    }
    
    public static bool IsLonAxisBiggerThan(TerrainSegment segment, TerrainSegment otherSegment)
    {
        return segment.lon > otherSegment.lon ? true : segment.lonIdx > otherSegment.lonIdx;
    }
}


public class TerrainTile : MonoBehaviour
{
    #region StaticMethods
    static bool Initiated { get { return TileMaterial != null; } }
    static Material TileMaterial;
    static TerrainLayerLoader TerrainLoader;
    static Transform RootTransform;
    public static void Init(TerrainLayerLoader terrainLoader, Transform rootTransform = null, Material tileMaterial = null)
    {
        TerrainLoader = terrainLoader;
        RootTransform = rootTransform;


        if (tileMaterial == null)
            tileMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        TileMaterial = tileMaterial;

    }
    public static TerrainTile CreateTile()
    {
        TerrainTile loadedTile;
        if (Initiated)
        {
            Transform tile = new GameObject("TerrainTile_Unallocated").transform;
            tile.parent = RootTransform;
            tile.transform.localPosition = Vector3.zero;
            tile.transform.localRotation = Quaternion.identity;

            MeshRenderer meshRenderer = tile.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = TileMaterial;
            tile.gameObject.AddComponent<MeshFilter>().mesh = new Mesh();

            loadedTile= tile.gameObject.AddComponent<TerrainTile>();
            return loadedTile;
        }

        return null;
    }
    #endregion


    public float LOD;
    public TerrainSegment Segment { get; private set; }

    private void Awake()
    {
        
    }
    public void Set(TerrainSegment segmentToLoad)
    {
        Segment = segmentToLoad;
        transform.name = "TerrainTile_" + Segment.representation;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        // Move the mesh into correct position
        transform.position = Utility.CreatePositionByCoords(Segment.lat, Segment.lon, 0) - GameManager.SimPivot;
    }

    public void UpdateTile()
    {
        TerrainLoader.Load(Segment, LOD, transform);
    }
}
