using UnityEngine;

//public class InitialLayerLoader : TerrainLayerLoader
//{
//    protected Material tileMaterial;
//    public InitialLayerLoader(TerrainLayerLoader nextLoader = null, Material tileMaterial = null) : base(TerrainLayer.initial, null, nextLoader)
//    {

//        if (tileMaterial == null)
//            tileMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

//        this.tileMaterial = tileMaterial;
//    }

//    protected override bool CheckSegmentCanBeLoaded(TerrainSegment segment)
//    {
//        return true;
//    }

//    protected override bool LoadLayer(TerrainSegment segment, ref Transform tile)
//    {
//        Transform newTile = new GameObject("TerrainTile_" + segment.representation).transform;

//        newTile.parent = tile;
//        newTile.transform.localPosition = Vector3.zero;
//        newTile.transform.localRotation = Quaternion.identity;
//        tile = newTile;

//        MeshRenderer meshRenderer = tile.gameObject.AddComponent<MeshRenderer>();
//        meshRenderer.material = tileMaterial;
//        tile.gameObject.AddComponent<MeshFilter>();

//        // Move the mesh into correct position
//        tile.position = Utility.CreatePositionByCoords(segment.lat, segment.lon, 0) - GameManager.SimPivot;


//        return true;
//    }
//}
