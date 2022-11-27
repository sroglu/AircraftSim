using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// GameObject ter = Resources.Load("terrain_adi") as GameObject;

public class TerrainController : MonoBehaviour
{
    public GameObject[,] terrainPart;
    public GameObject noTerrain;

    public const double DEG_2_RAD = System.Math.PI / 180.0;
    public const int EARTH_RADIUS = 6378137;
    public const int ZERO = 0;

    public Vector3 otuzdokuzOtuzikiBasePos, otuzdokuzOtuzucBasePos;
    public Vector3 airplanePos;
    public Vector3 airplanePosStack;
    public Vector3[,] basePositions;
    public Vector3[,] basePositionsStack;

    // Start is called before the first frame update
    void Start()
    {

        // For start, only from (0, 0) to (50, 50) is indexed for quick demo
        terrainPart = new GameObject[50 * 6, 50 * 6];
        for (int i = 0; i < 50 * 6; i++)
        {
            for (int j = 0; j < 50 * 6; j++)
            {
                terrainPart[i, j] = noTerrain;
            }
        }

        /*Object[] terrainInResources = Resources.LoadAll("");
        Debug.Log(terrainInResources.Length);
        foreach (Object terrain in terrainInResources)
        {
            Debug.Log("Type of object is " + terrain.GetType() + " and name of the object is " + terrain.name);
        }
        //GameObject ter = Resources.Load("36_1_32_1.fbx") as GameObject;
        GameObject instance = Instantiate(Resources.Load("36_1_32_1", typeof(GameObject))) as GameObject;*/

        /*GameObject g = GameObject.Instantiate(Resources.Load("36_1_32_1.fbx")) as GameObject;
        if (g == null)
        {
            Debug.Log("NULL");
        }
        else
        {
            Debug.Log("NOT NULL");
        }*/

        /*foreach (Object terrain in terrainInResources)
        {
            int latIdx, lonIdx;
            (latIdx, lonIdx) = GetIndex((GameObject)terrain);
            terrainPart[latIdx, lonIdx] = (GameObject)terrain;
        }*/

        basePositions = new Vector3[50, 50];
        for (int lati = 0; lati < 50; lati++)
        {
            for (int longi = 0; longi < 50; longi++)
            {
                basePositions[lati, longi] = CalculateBasePos(lati, longi);
            }
        }

        basePositionsStack = new Vector3[50, 50];
        for (int lati = 0; lati < 50; lati++)
        {
            for (int longi = 0; longi < 50; longi++)
            {
                basePositionsStack[lati, longi] = CalculateBasePosStack(lati, longi);
            }
        }

        otuzdokuzOtuzikiBasePos = XyzPos(39, 32, ZERO);
        otuzdokuzOtuzucBasePos = XyzPos(39, 33, ZERO);
        airplanePos = XyzPos(39, 32, 0);

        Debug.Log(String.Format("airplane pos x, y, z is {0} - {1} - {2}", airplanePos.x, airplanePos.y, airplanePos.z));
        Debug.Log(String.Format("base pos for 39, 32 is {0} - {1} - {2}", basePositions[39, 32].x, basePositions[39, 32].y, basePositions[39, 32].z));
        airplanePosStack = XyzPosStack(39.5f, 32.5f, 500);

        // Fill in the terrainPart variable
        GameObject[] terrainModels = GameObject.FindGameObjectsWithTag("Terrain");
        foreach (GameObject terrainModel in terrainModels)
        {
            int latIdx, lonIdx;
            (latIdx, lonIdx) = GetIndex(terrainModel);
            terrainPart[latIdx, lonIdx] = terrainModel;
        }

        // Move terrain with respect to their basePos
        foreach(GameObject terrain in terrainPart)
        {
            if (terrain.CompareTag("Terrain"))
            {
                Vector3 basePos = GetBasePos(terrain);
                Vector3 basePosStack = GetBasePosStack(terrain);
                //Debug.Log("Name of the terrain is " + terrain.name);
                //Debug.Log("x, y, z of the plane is " + airplanePosStack.x + ", " + airplanePosStack.y + ", " + airplanePosStack.z);
                //Debug.Log("x, y, z of the base position is " + basePosStack.x + ", " + basePosStack.y + ", " + basePosStack.z);
                //terrain.transform.Translate(basePosStack - airplanePosStack);
                //terrain.transform.Translate(basePos - airplanePos);
                terrain.transform.position = basePos - airplanePos;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 XyzPos(float lat, float lon, float altitude)
    {
        double y, x, z;
        z = System.Math.Cos(lat * DEG_2_RAD) * System.Math.Cos(lon * DEG_2_RAD);
        x = System.Math.Cos(lat * DEG_2_RAD) * System.Math.Sin(lon * DEG_2_RAD);
        y = System.Math.Sin(lat * DEG_2_RAD);

        float multiplyConst = EARTH_RADIUS + altitude;

        Vector3 aircraftUp = new Vector3((float)x, (float)y, (float)z);

        Vector3 onEarthPos = aircraftUp * multiplyConst;

        return onEarthPos;
    }

    public Vector3 XyzPosStack(float lat, float lon, float alt)
    {
        var cosLat = System.Math.Cos(lat * System.Math.PI / 180.0);
        var sinLat = System.Math.Sin(lat * System.Math.PI / 180.0);
        var cosLon = System.Math.Cos(lon * System.Math.PI / 180.0);
        var sinLon = System.Math.Sin(lon * System.Math.PI / 180.0);

        double x, y, z;
        float multiplyConst = EARTH_RADIUS + alt;

        x = multiplyConst * cosLat * cosLon;
        z = multiplyConst * cosLat * sinLon;
        y = multiplyConst * sinLat;

        Vector3 onEarthPos = new Vector3((float)x, (float)y, (float)z);
        return onEarthPos;
    }

    public (int, int, int, int) SplitName(GameObject terrain)
    {
        // Indexing starts from (lat, lon) = (0, 0) to (lat, lon) = (50, 50)
        // Debug.Log("Terrain name is " + terrain.name);
        string name = terrain.name;
        string latStr = name.Substring(0, 2);
        string latMultStr = name[3].ToString();
        string lonStr = name.Substring(5, 2);
        string lonMultStr = name[8].ToString();

        int latInt = System.Int32.Parse(latStr);
        int latMultInt = System.Int32.Parse(latMultStr);
        int lonInt = System.Int32.Parse(lonStr);
        int lonMultInt = System.Int32.Parse(lonMultStr);

        // print("lat, mult, lon, mult is " + latStr + ", " + latMultStr + ", " + lonStr + ", " + lonMultStr);

        return (latInt, latMultInt, lonInt, lonMultInt);
    }

    public (int, int) GetIndex(GameObject terrain)
    {
        int lat, latOffset, lon, lonOffset;
        (lat, latOffset, lon, lonOffset) = SplitName(terrain);
        
        int latIndex = lat * 6 + latOffset;
        int lonIndex = lon * 6 + lonOffset;

        return (latIndex, lonIndex);
    }

    public (int, int) GetLatLon(GameObject terrain)
    {
        int lat, lon;
        (lat, _, lon, _) = SplitName(terrain);

        return (lat, lon);
    }

    public Vector3 CalculateBasePos(int lat, int lon)
    {
        Vector3 basePos = XyzPos(lat, lon, ZERO);
        return basePos;
    }

    public Vector3 CalculateBasePosStack(int lat, int lon)
    {
        Vector3 basePos = XyzPosStack(lat, lon, ZERO);
        return basePos;
    }

    public Vector3 GetBasePos(GameObject terrain)
    {
        int lat, lon;
        (lat, lon) = GetLatLon(terrain);
        return basePositions[lat, lon];
    }

    public Vector3 GetBasePosStack(GameObject terrain)
    {
        int lat, lon;
        (lat, lon) = GetLatLon(terrain);
        return basePositionsStack[lat, lon];
    }
}
