using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainManagerv2 : MonoBehaviour
{


    static TerrainManagerv2 instance;
    public static TerrainManagerv2 Instance
    {
        get { return instance; }
        private set
        {
            if (instance == null)
                instance = value;
            else
                Destroy(value);
        }
    }
    

    const float RADIUS = 0.7f;
    static float CHECK_POINT_ANGLE = Mathf.Atan2(Constants.TileLenght, RADIUS);

    public string terrainDataRootPath,
        SurfaceLayerPath = "TerrainModel";

    [SerializeField]
    Material terrainMaterial;

    Dictionary<TerrainSegment, TerrainTile> terrainTiles= new Dictionary<TerrainSegment, TerrainTile>();
    Vector2 origin;

    UnityEngine.Pool.ObjectPool<TerrainTile> terrainTilePool;
    TerrainLayerLoader terrainLoader;
    void Awake()
    {
        Instance = this;
        //GameManager.GameStarted += AfterGameManagerStart;


        //TerrainLayerLoader.RootDir = terrainDataRootPath;
        ////Debug.Log(Indexer.ObjNameFromCoords(32.986f,44.3457f));

        //terrainLoader = new SurfaceLayerLoader(SurfaceLayerPath);

        //TerrainTile.Init(terrainLoader, transform, terrainMaterial);
        //terrainTilePool = new UnityEngine.Pool.ObjectPool<TerrainTile>(
        //    createFunc: () => TerrainTile.CreateTile(),
        //    actionOnGet: (obj) => obj.gameObject.SetActive(true),
        //    actionOnRelease: (obj) => obj.gameObject.SetActive(false),
        //    actionOnDestroy: (obj) => Destroy(obj), 
        //    collectionCheck: false);
    }


    private void AfterGameManagerStart()
    {

        origin = new Vector2((float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lat, (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lon);
        currentSegment = new TerrainSegment(Indexer.ObjIdxFromCoords(origin.x, origin.y));




        //TerrainTile tile = terrainTilePool.Get();
        //tile.Set(currentSegment);
        //tile.LOD = 1;
        //tile.UpdateTile();
        //terrainTiles.Add(tile.Segment, tile);


        UpdateTiles();



    }

    void UpdateTiles()
    {

        foreach (TerrainTile tile in terrainTiles.Values)
        {
            tile.LOD = -1;
        }


        //Create current tile
        SetSegmentToTile(currentSegment);

        float angle = 0;
        while (angle < Math.PI*2)
        {
            Vector2 checkPoint = origin + new Vector2(RADIUS*Mathf.Cos(angle), RADIUS * Mathf.Sin(angle));
            //TODO

            TerrainSegment segment =  new TerrainSegment(Indexer.ObjIdxFromCoords(checkPoint.x, checkPoint.y));

            SetSegmentToTile(segment);

            foreach(TerrainSegment midSegment in currentSegment.GetSegmentsTo(segment))
            {
                SetSegmentToTile(midSegment);
            }

            angle += CHECK_POINT_ANGLE;
        }


        TerrainSegment minSegmentOnLat = currentSegment, maxSegmentOnLat= currentSegment;

        foreach (TerrainSegment segment in terrainTiles.Keys)
        {
            if(TerrainSegment.IsLatAxisBiggerThan(segment, maxSegmentOnLat))
            {
                maxSegmentOnLat = segment;
            }else if(TerrainSegment.IsLatAxisBiggerThan(minSegmentOnLat,segment))
            {
                minSegmentOnLat = segment;
            }
        }




        foreach (TerrainTile tile in terrainTiles.Values)
        {
            //if(tile.LOD < 0)
            //{
            //    terrainTilePool.Release(tile);
            //}
            //else
                tile.UpdateTile();
        }

    }

    void SetSegmentToTile(TerrainSegment segment)
    {
        TerrainTile tile;
        if (terrainTiles.ContainsKey(segment))
        {
            tile = terrainTiles[segment];
        }
        else
        {
            tile = terrainTilePool.Get();
            tile.Set(segment);
            terrainTiles.Add(segment, tile);
        }

        float lat, lon;
        (lat, lon) = Indexer.CenterCoordsFromObjIdx(tile.Segment.lat, tile.Segment.latIdx, tile.Segment.lon, tile.Segment.lonIdx);
        Vector2 tilePos = new Vector2(lat, lon);

        if(1 - (Vector2.Distance(origin, tilePos) / RADIUS) < 0)
        {
            Debug.Log("Check here");
        }

        tile.LOD = 1 - (Vector2.Distance(origin, tilePos) / RADIUS);// + Constants.TileLenght;

    }



    // Start is called before the first frame update
    void Start()
    {


    }

    TerrainSegment currentSegment;

    // Update is called once per frame
    void Update()
    {
        origin.x = (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lat;
        origin.y = (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lon;

        (currentSegment.lat, currentSegment.lon, currentSegment.lat, currentSegment.lat) =
            Indexer.ObjIdxFromCoords(origin.x, origin.y);
    }
}
