using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

public class TerrainManager : MonoBehaviour
{
    static TerrainManager instance;
    public static TerrainManager Instance
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

    //public Aircraft baseAircraft;
    [SerializeField]
    Material terrainMaterial;
    [SerializeField]
    Material emptyTerrainMaterial;
    [SerializeField]
    Material buildingMaterial;

    MapSection currSection;
    MapSection Southwestern;
    MapSection Northeastern;
    MapSection InnerSouthwestern;
    MapSection InnerNortheastern;
    public float visionKm;
    public float visionShortKm;
    int latSectionCount;
    int lonSectionCount;

    byte[] textureBuffer;

    // This needs to be updated
    Queue<(string, ZoomLevel)> texturesToLoad;
    HashSet<string> airportTextures;
    bool textureLoading;
    MemoryStream memStream;

    public Vector3[,] basePositions;
    public Dictionary<string, MapSection> terrains;

    int sectionBufferSize = 1;

    public string terrainDataPath;      // May need to read from config file

    public Texture2D texture16k;

    public List<Texture2D> freeTexture4kList;
    public List<Texture2D> usedTexture4kList;

    public List<Texture2D> freeTexture1kList;
    public List<Texture2D> usedTexture1kList;

    public Texture2D bufferTexture;
    TextureFormat textureFormat;

    /* Defining Texture2d Values */
    const int textureBufferSize = ((512 * 512) / 2);
    const int size16k = 1024 * 16;
    const int size4k = 1024 * 4;
    const int size1k = 1024;
    const int sizeHalfK = 1024 / 2;

    bool initiated = false;

    void Awake()
    {
        Instance = this;

        GameManager.GameStarted += AfterGameManagerStart;
    }

    private void AfterGameManagerStart()
    {
        // Initialize the currSection
        InitializeCurrentSection();

        textureFormat = TextureFormat.DXT1;


        bufferTexture = new Texture2D(sizeHalfK, sizeHalfK, textureFormat, false);
        texture16k = new Texture2D(size16k, size16k, textureFormat, false);

        freeTexture4kList = new List<Texture2D>();
        usedTexture4kList = new List<Texture2D>();
        for (int i = 0; i < 100; i++)
        {
            freeTexture4kList.Add(new Texture2D(size4k, size4k, textureFormat, false));
        }

        freeTexture1kList = new List<Texture2D>();
        usedTexture4kList = new List<Texture2D>();
        for (int i = 0; i < 600; i++)
        {
            freeTexture1kList.Add(new Texture2D(size1k, size1k, textureFormat, false));
        }

        // Initialize the terrains dictionary
        terrains = new Dictionary<string, MapSection>();

        // Determine the outer matrix to be loaded
        Southwestern = Indexer.GetSouthwestBoundary(currSection.latitude + ((float)(currSection.latitudeIdx - 1) / Constants.DivisorOfOneDeg), currSection.longitude + ((float)(currSection.longitudeIdx - 1) / Constants.DivisorOfOneDeg), visionKm);
        Northeastern = Indexer.GetNortheastBoundary(currSection.latitude + ((float)(currSection.latitudeIdx - 1) / Constants.DivisorOfOneDeg), currSection.longitude + ((float)(currSection.longitudeIdx - 1) / Constants.DivisorOfOneDeg), visionKm);

        // Determine section counts to bypass distance calculation every frame
        latSectionCount = Mathf.Abs(currSection.SubtractLatitude(Northeastern));
        lonSectionCount = Mathf.Abs(currSection.SubtractLongitude(Northeastern));

        // Initialize the texture queue
        textureBuffer = new byte[textureBufferSize];

        // Calculate queue size to avoid instantiation of a larger queue in run time
        int qSize = ((latSectionCount + 1) * 2) * ((lonSectionCount + 1) * 2) + 9/*For high-res textures*/;
        texturesToLoad = new Queue<(string, ZoomLevel)>(qSize);

        // Initialize memory stream for efficient data transfer
        int twoHundredMB = 209715200;
        memStream = new MemoryStream(twoHundredMB);

        // Load meshes in dictionary format
        LoadInitialMeshes();

        // Find all available airports from file directory to avoid checking in run time
        string[] airportTextureFiles = Directory.GetFiles($"{terrainDataPath}/TerrainTexture/timg/{(int)ZoomLevel.Sixteen}/");
        //Debug.Log("texturePath1 : " + $"{terrainDataPath}/TerrainTexture/timg/{(int)ZoomLevel.Sixteen}/");
        airportTextures = new HashSet<string>(airportTextureFiles.Select(x => Path.GetFileNameWithoutExtension(x)));

        // Start the coroutine for loading textures which will run entire time
        textureLoading = false;
        //tarik
        StartCoroutine(LoadTextures());

        initiated = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initiated) return;

        // Update CurrSection
        UpdateCurrentSection();

        // Update inner boundaries
        UpdateInnerBoundaries();

        // Load high level textures
        LoadDetailedTextures();

        // Load new terrains and remove old ones
        LoadCloseTerrains();
        // There is a buffer row/column of terrains to prevent reloading terrains
        RemoveFarTerrains();

        // Update positions
        UpdateTerrainPositions();
    }

    void LoadMesh(string meshName)
    {
        // Add new entry to terrains dictionary, do I need to check?
        terrains[meshName] = new MapSection(meshName);

        // Load mesh
        //terrains[meshName].LoadMesh(emptyTerrainMaterial, baseAircraft.altitude, baseAircraft.aircraftbasePos);

        //terrains[meshName].LoadBuildingMesh(buildingMaterial,baseAircraft.altitude);
        bool terrainsLoaded = terrains[meshName].LoadMesh(emptyTerrainMaterial,(float)GameManager.PlayerAircraft.CurrentAircraftData.transform.alt, GameManager.SimPivot);

        bool buildingsLoaded = terrains[meshName].LoadBuildingMesh(buildingMaterial, (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.alt);

        if (terrains[meshName].Section!=null)
        {
            // Set mesh's parent for organization
            terrains[meshName].Section.parent = transform;
        }
    }

    void LoadDetailedTextures()
    {
        // Load the square matrix (Mesh)
        Queue<string> innerQ = new Queue<string>();
        HashSet<string> innerVisited = new HashSet<string>();

        innerQ.Enqueue(currSection.representation);
        innerVisited.Add(currSection.representation);

        // Populate meshes using a plus-shaped queue
        while (innerQ.Count > 0)
        {
            int size = innerQ.Count;
            for (int i = 0; i < size; i++)
            {
                string meshName = innerQ.Dequeue();
                // First, lowest level of textures need to be loaded
                if (airportTextures.Contains(meshName))
                {
                    //   texturesToLoad.Enqueue((meshName, ZoomLevel.Sixteen));
                }

                // Check top neighbor
                MapSection northNeighbor = terrains[meshName].IncrementLatitude();
                if (northNeighbor <= InnerNortheastern && !innerVisited.Contains(northNeighbor.representation))
                {
                    innerVisited.Add(northNeighbor.representation);
                    innerQ.Enqueue(northNeighbor.representation);
                }
                // Check bottom neighbor
                MapSection southNeighbor = terrains[meshName].DecrementLatitude();
                if (southNeighbor >= InnerSouthwestern && !innerVisited.Contains(southNeighbor.representation))
                {
                    innerVisited.Add(southNeighbor.representation);
                    innerQ.Enqueue(southNeighbor.representation);
                }
                // Check right neighbor
                MapSection eastNeighbor = terrains[meshName].IncrementLongitude();
                if (eastNeighbor <= InnerNortheastern && !innerVisited.Contains(eastNeighbor.representation))
                {
                    innerVisited.Add(eastNeighbor.representation);
                    innerQ.Enqueue(eastNeighbor.representation);
                }
                // Check left neighbor
                MapSection westNeighbor = terrains[meshName].DecrementLongitude();
                if (westNeighbor >= InnerSouthwestern && !innerVisited.Contains(westNeighbor.representation))
                {
                    innerVisited.Add(westNeighbor.representation);
                    innerQ.Enqueue(westNeighbor.representation);
                }
            }
        }
    }

    void LoadInitialMeshes()
    {
        // Load the square matrix (Mesh)
        Queue<string> meshQ = new Queue<string>();
        HashSet<string> meshVisited = new HashSet<string>();

        meshQ.Enqueue(currSection.representation);
        meshVisited.Add(currSection.representation);

        // Populate meshes using a plus-shaped queue
        while (meshQ.Count > 0)
        {
            int size = meshQ.Count;
            for (int i = 0; i < size; i++)
            {
                string meshName = meshQ.Dequeue();
                // Load the mesh
                LoadMesh(meshName);
                // First, lowest level of textures need to be loaded
                texturesToLoad.Enqueue((meshName, ZoomLevel.Eleven));
                //texturesToLoad.Enqueue((meshName, ZoomLevel.Thirteen));
                if (currSection.IsNeighbor(meshName))
                {
                    texturesToLoad.Enqueue((meshName, ZoomLevel.Sixteen));
                }

                // Check top neighbor
                MapSection northNeighbor = terrains[meshName].IncrementLatitude();
                if (northNeighbor <= Northeastern && !meshVisited.Contains(northNeighbor.representation))
                {
                    meshVisited.Add(northNeighbor.representation);
                    meshQ.Enqueue(northNeighbor.representation);
                }
                // Check bottom neighbor
                MapSection southNeighbor = terrains[meshName].DecrementLatitude();
                if (southNeighbor >= Southwestern && !meshVisited.Contains(southNeighbor.representation))
                {
                    meshVisited.Add(southNeighbor.representation);
                    meshQ.Enqueue(southNeighbor.representation);
                }
                // Check right neighbor
                MapSection eastNeighbor = terrains[meshName].IncrementLongitude();
                if (eastNeighbor <= Northeastern && !meshVisited.Contains(eastNeighbor.representation))
                {
                    meshVisited.Add(eastNeighbor.representation);
                    meshQ.Enqueue(eastNeighbor.representation);
                }
                // Check left neighbor
                MapSection westNeighbor = terrains[meshName].DecrementLongitude();
                if (westNeighbor >= Southwestern && !meshVisited.Contains(westNeighbor.representation))
                {
                    meshVisited.Add(westNeighbor.representation);
                    meshQ.Enqueue(westNeighbor.representation);
                }
            }
        }
    }

    IEnumerator LoadTextures()
    {
        // Run this as a background task
        while (true)
        {
            if (texturesToLoad.Count > 0 && !textureLoading)
            {
                string meshName;
                ZoomLevel zoom;
                (meshName, zoom) = texturesToLoad.Dequeue();
                //LoadTextureDxtAsync(terrains[meshName].Section);
                if (terrains.ContainsKey(meshName))
                {
                    if (terrains[meshName].zoom != zoom)
                    {
                        //tarik
                        StartCoroutine(LoadTextureSliced(meshName, zoom));
                    }
                }
            }
            yield return null;
        }
    }

    IEnumerator LoadTextureSliced(string meshName, ZoomLevel zoom)
    {
        textureLoading = true;

        //string texturePath = $"{terrainDataPath}/TerrainTexture/timg/{(int)zoom}/{meshName}.dxt1";
        string texturePath = $"{terrainDataPath}/TerrainTexture/timg/{(int)zoom}/{meshName}.timg";
        //string texturePath = $"{terrainDataPath}/TerrainTexture/timg/{(int)zoom}/newterrainimage.timg";

        if (File.Exists(texturePath))
        {
            using (FileStream fs = File.OpenRead(texturePath))
            {
                using (BinaryReader reader = new BinaryReader(fs))

                //using (BufferedStream bs = new BufferedStream(fs, textureBufferSize * 2))
                {
                    reader.Read(textureBuffer, 0, 1);
                    int divider = (int)textureBuffer[0];

                    Texture2D meshTexture = GetEmptyTexture(divider);

                    for (int ti = 0; ti < divider * divider; ti++)
                    {
                        int totalNo = reader.Read(textureBuffer, 0, textureBufferSize); //textureBufferSize
                                                                                        // Debug.Log("totalNo : " + totalNo );

                        if (totalNo > 0)
                        {
                            /* memStream.Write(textureBuffer, textureBufferSize, byteRead);
                             yield return null;*/

                            bufferTexture.LoadRawTextureData(textureBuffer);
                            //   yield return null;
                            bufferTexture.Apply(false);
                            //   yield return null;

                            //                                Debug.Log("copy coord : " + ((ti % divider) * 512) + " - " + ((divider - (ti / divider) - 1) * 512));

                            //Graphics.CopyTexture(Texture src, int srcElement, int srcMip, int srcX, int srcY, int srcWidth, int srcHeight, Texture dst, int dstElement, int dstMip, int dstX, int dstY);
                            Graphics.CopyTexture(bufferTexture, 0, 0, 0, 0, 512, 512, meshTexture, 0, 0, (ti % divider) * 512, (ti / divider) * 512);
                            yield return null;

                        }
                    }

                    if (terrains.ContainsKey(meshName))
                    {
                        //  terrainMaterial.SetTexture("_BaseMap", meshTexture);
                        terrains[meshName].usedTexture = meshTexture;
                        terrains[meshName].Section.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", meshTexture);
                        terrains[meshName].zoom = zoom;
                    }

                }
            }
        }
        // Check if file exists, maybe don't check everytime for zoom level 16?

        textureLoading = false;
    }

    Texture2D GetEmptyTexture(int divider)
    {
        Texture2D selectedTexture = null;
        if (divider == 32)
        {
            selectedTexture = texture16k;
        }
        else if (divider == 8)
        {
            if (freeTexture4kList.Count > 0)
            {
                selectedTexture = freeTexture4kList[0];
                freeTexture4kList.RemoveAt(0);

            }
            else
            {
                selectedTexture = new Texture2D(size4k, size4k, textureFormat, false);
            }
            //usedTexture4kList.Add(selectedTexture);
        }
        else if (divider == 2)
        {
            if (freeTexture1kList.Count > 0)
            {
                selectedTexture = freeTexture1kList[0];
                freeTexture1kList.RemoveAt(0);

            }
            else
            {
                selectedTexture = new Texture2D(size1k, size1k, textureFormat, false);
            }
            //usedTexture1kList.Add(selectedTexture);
        }

        return selectedTexture;
    }

    void FlushTexture2D(MapSection mapSection)
    {
        if (mapSection.zoom == ZoomLevel.Thirteen)
        {
            freeTexture4kList.Add(mapSection.usedTexture);
        }
        else if (mapSection.zoom == ZoomLevel.Eleven)
        {
            freeTexture1kList.Add(mapSection.usedTexture);
        }
        mapSection.usedTexture = null;
    }



    IEnumerator LoadTexture(string meshName, ZoomLevel zoom)
    {
        textureLoading = true;
        string texturePath = $"{terrainDataPath}/TerrainTexture/timg/{(int)zoom}/{meshName}.dxt1";

        Debug.Log("texturePath : " + texturePath);
        if (File.Exists(texturePath))
        {
            byte[] ddsBytes;
            using (FileStream fs = new FileStream(texturePath, FileMode.Open, FileAccess.Read, FileShare.Read, textureBufferSize))
            {
                using (BufferedStream bs = new BufferedStream(fs, textureBufferSize))
                {
                    int byteRead;
                    while ((byteRead = bs.Read(textureBuffer, 0, textureBufferSize)) > 0)
                    {
                        memStream.Write(textureBuffer, 0, byteRead);
                        yield return null;
                    }
                    ddsBytes = memStream.ToArray();
                    // Restart memStream without initializing
                    memStream.Seek(0, SeekOrigin.Begin);

                    TextureFormat textureFormat = TextureFormat.DXT1;
                    if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
                        throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

                    byte ddsSizeCheck = ddsBytes[4];
                    if (ddsSizeCheck != 124)
                        throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

                    int height = ddsBytes[13] * 256 + ddsBytes[12];
                    int width = ddsBytes[17] * 256 + ddsBytes[16];

                    int DDS_HEADER_SIZE = 128;
                    byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
                    Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

                    if (terrains.ContainsKey(meshName))
                    {
                        Texture2D texture = new Texture2D(width, height, textureFormat, false);
                        texture.LoadRawTextureData(dxtBytes);
                        yield return null;
                        texture.Apply(false);
                        yield return null;
                        // Double checking in case of deletion of further terrains
                        if (terrains.ContainsKey(meshName))
                        {
                            terrains[meshName].Section.GetComponent<MeshRenderer>().material = terrainMaterial;
                            //terrains[meshName].Section.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
                            //URP Texture Assignment
                            terrains[meshName].Section.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", texture);
                            terrains[meshName].zoom = zoom;
                        }
                    }
                }
            }
        }
        // Check if file exists, maybe don't check everytime for zoom level 16?

        textureLoading = false;
    }

    void UpdateTerrainPositions()
    {
        foreach (KeyValuePair<string, MapSection> sectEntry in terrains)
        {
            MapSection mapSect = sectEntry.Value;
            Vector3 seaLevelPos = Utility.CreatePositionByCoords(mapSect.latitude, mapSect.longitude, Constants.Zero);
            //mapSect.Section.position = basePos - baseAircraft.aircraftbasePos;
            mapSect.Section.position = seaLevelPos - GameManager.SimPivot;
        }
    }

    void InitializeCurrentSection()
    {
        // Determine the MapSection which is below the aircraft
        //if (Indexer.AirplaneInBounds(baseAircraft.lat, baseAircraft.lon))
        if (Indexer.AirplaneInBounds((float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lat, (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lon))
        {
            //currSection = new MapSection(baseAircraft.lat, baseAircraft.lon);
            currSection = new MapSection((float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lat, (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lon);
        }
        else
        {
            // Default starting point
            currSection = new MapSection((float)GameManager.Instance.defaultAircraftData.transform.lat, (float)GameManager.Instance.defaultAircraftData.transform.lon);
        }
    }

    void UpdateCurrentSection()
    {
        //if (Indexer.AirplaneInBounds(baseAircraft.lat, baseAircraft.lon))
        if (Indexer.AirplaneInBounds((float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lat, (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lon))
        {
            //if (currSection.representation != Indexer.ObjNameFromCoords(baseAircraft.lat, baseAircraft.lon))
            if (currSection.representation != Indexer.ObjNameFromCoords((float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lat, (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lon))
            {
                //currSection = new MapSection(baseAircraft.lat, baseAircraft.lon);
                currSection = new MapSection((float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lat, (float)GameManager.PlayerAircraft.CurrentAircraftData.transform.lon);
            }
        }
    }

    void UpdateInnerBoundaries()
    {
        InnerSouthwestern = currSection.DecrementLatitude().DecrementLongitude();
        InnerNortheastern = currSection.IncrementLatitude().IncrementLongitude();
    }

    IEnumerator AddMapSectionRows(int rowDiff)
    {
        if (rowDiff > 0)
        {
            // Add row to north
            MapSection northwestern = new MapSection(Northeastern.latitude, Northeastern.latitudeIdx, Southwestern.longitude, Southwestern.longitudeIdx);
            northwestern = northwestern.IncrementLatitude();
            MapSection northeastern = Northeastern.IncrementLatitude();
            MapSection sectToAdd = northwestern;
            // Update the boundary MapSection
            Northeastern = Northeastern.IncrementLatitude();
            // Iterate over longitudes
            while (sectToAdd.representation != northeastern.representation)
            {
                LoadMesh(sectToAdd.representation);
                yield return null;
                texturesToLoad.Enqueue((sectToAdd.representation, ZoomLevel.Thirteen));
                sectToAdd = sectToAdd.IncrementLongitude();
            }
            LoadMesh(northeastern.representation);
            yield return null;
            texturesToLoad.Enqueue((northeastern.representation, ZoomLevel.Thirteen));
        }
        else if (rowDiff < 0)
        {
            // Add row to south
            MapSection southwestern = Southwestern.DecrementLatitude();
            MapSection southeastern = new MapSection(Southwestern.latitude, Southwestern.latitudeIdx, Northeastern.longitude, Northeastern.longitudeIdx);
            southeastern = southeastern.DecrementLatitude();
            MapSection sectToAdd = southwestern;
            // Update the boundary MapSection
            Southwestern = Southwestern.DecrementLatitude();
            // Iterate over longitudes
            while (sectToAdd.representation != southeastern.representation)
            {
                LoadMesh(sectToAdd.representation);
                yield return null;
                texturesToLoad.Enqueue((sectToAdd.representation, ZoomLevel.Thirteen));
                sectToAdd = sectToAdd.IncrementLongitude();
            }
            LoadMesh(southeastern.representation);
            yield return null;
            texturesToLoad.Enqueue((southeastern.representation, ZoomLevel.Thirteen));
        }
    }

    void RemoveMapSectionRows(int rowDiff)
    {
        if (rowDiff > 0)
        {
            // Remove northmost row
            MapSection northwestern = new MapSection(Northeastern.latitude, Northeastern.latitudeIdx, Southwestern.longitude, Southwestern.longitudeIdx);
            MapSection sectToRemove = northwestern;
            // Iterate over longitudes
            while (sectToRemove.representation != Northeastern.representation)
            {
                FlushTexture2D(sectToRemove);
                terrains.Remove(sectToRemove.representation);
                Destroy(transform.Find(sectToRemove.representation).gameObject);
                sectToRemove = sectToRemove.IncrementLongitude();
            }
            FlushTexture2D(Northeastern);
            terrains.Remove(Northeastern.representation);
            Destroy(transform.Find(Northeastern.representation).gameObject);

            // Update the boundary MapSection
            Northeastern = Northeastern.DecrementLatitude();
        }
        else if (rowDiff < 0)
        {
            // Remove southmost row
            MapSection southeastern = new MapSection(Southwestern.latitude, Southwestern.latitudeIdx, Northeastern.longitude, Northeastern.longitudeIdx);
            MapSection sectToRemove = Southwestern;
            // Iterate over longitudes
            while (sectToRemove.representation != southeastern.representation)
            {
                FlushTexture2D(sectToRemove);
                terrains.Remove(sectToRemove.representation);
                if(transform.Find(sectToRemove.representation).gameObject!=null)
                    Destroy(transform.Find(sectToRemove.representation).gameObject);
                sectToRemove = sectToRemove.IncrementLongitude();
            }
            FlushTexture2D(southeastern);
            terrains.Remove(southeastern.representation);
            Destroy(transform.Find(southeastern.representation).gameObject);

            // Update the boundary MapSection
            Southwestern = Southwestern.IncrementLatitude();
        }
    }

    IEnumerator AddMapSectionColumns(int colDiff)
    {
        if (colDiff > 0)
        {
            // Add column to east
            MapSection southeastern = new MapSection(Southwestern.latitude, Southwestern.latitudeIdx, Northeastern.longitude, Northeastern.longitudeIdx);
            southeastern = southeastern.IncrementLongitude();
            MapSection northeastern = Northeastern.IncrementLongitude();
            MapSection sectToAdd = southeastern;
            // Iterate over latitudes
            while (sectToAdd.representation != northeastern.representation)
            {
                LoadMesh(sectToAdd.representation);
                yield return null;
                texturesToLoad.Enqueue((sectToAdd.representation, ZoomLevel.Thirteen));
                sectToAdd = sectToAdd.IncrementLatitude();
            }
            LoadMesh(northeastern.representation);
            yield return null;
            texturesToLoad.Enqueue((northeastern.representation, ZoomLevel.Thirteen));

            // Update the boundary MapSection
            Northeastern = Northeastern.IncrementLongitude();
        }
        else if (colDiff < 0)
        {
            // Remove westmost column
            MapSection southwestern = Southwestern.DecrementLongitude();
            MapSection northwestern = new MapSection(Northeastern.latitude, Northeastern.latitudeIdx, Southwestern.longitude, Southwestern.longitudeIdx);
            northwestern = northwestern.DecrementLongitude();
            MapSection sectToAdd = southwestern;
            while (sectToAdd.representation != northwestern.representation)
            {
                LoadMesh(sectToAdd.representation);
                yield return null;
                texturesToLoad.Enqueue((sectToAdd.representation, ZoomLevel.Thirteen));
                sectToAdd = sectToAdd.IncrementLatitude();
            }
            LoadMesh(northwestern.representation);
            yield return null;
            texturesToLoad.Enqueue((northwestern.representation, ZoomLevel.Thirteen));

            // Update the boundary MapSection
            Southwestern = Southwestern.DecrementLongitude();
        }
    }

    void RemoveMapSectionColumns(int colDiff)
    {
        if (colDiff > 0)
        {
            // Remove eastmost column
            MapSection southeastern = new MapSection(Southwestern.latitude, Southwestern.latitudeIdx, Northeastern.longitude, Northeastern.longitudeIdx);
            MapSection sectToRemove = southeastern;
            // Iterate over latitudes
            while (sectToRemove.representation != Northeastern.representation)
            {
                terrains.Remove(sectToRemove.representation);
                if(transform.Find(sectToRemove.representation)!=null)
                    Destroy(transform.Find(sectToRemove.representation).gameObject);
                sectToRemove = sectToRemove.IncrementLatitude();
            }
            terrains.Remove(Northeastern.representation);
            if (transform.Find(Northeastern.representation) != null)
                Destroy(transform.Find(Northeastern.representation).gameObject);

            // Update the boundary MapSection
            Northeastern = Northeastern.DecrementLongitude();
        }
        else if (colDiff < 0)
        {
            // Remove westmost column
            MapSection northwestern = new MapSection(Northeastern.latitude, Northeastern.latitudeIdx, Southwestern.longitude, Southwestern.longitudeIdx);
            MapSection sectToRemove = Southwestern;
            while (sectToRemove.representation != northwestern.representation)
            {
                terrains.Remove(sectToRemove.representation);
                Destroy(transform.Find(sectToRemove.representation).gameObject);
                sectToRemove = sectToRemove.IncrementLatitude();
            }
            terrains.Remove(northwestern.representation);
            Destroy(transform.Find(northwestern.representation).gameObject);

            // Update the boundary MapSection
            Southwestern = Southwestern.IncrementLongitude();
        }
    }

    void LoadCloseTerrains()
    {
        // Check whether new row needs to be added to north
        MapSection northern = new MapSection(Northeastern.latitude, Northeastern.latitudeIdx, currSection.longitude, currSection.longitudeIdx);
        if (Mathf.Abs(currSection.SubtractLatitude(northern)) < latSectionCount)
        {
            StartCoroutine(AddMapSectionRows(1));
        }

        // Check whether new row needs to be added to south
        MapSection southern = new MapSection(Southwestern.latitude, Southwestern.latitudeIdx, currSection.longitude, currSection.longitudeIdx);
        if (Mathf.Abs(currSection.SubtractLatitude(southern)) < latSectionCount)
        {
            StartCoroutine(AddMapSectionRows(-1));
        }

        // Check whether new column needs to be added to east
        MapSection eastern = new MapSection(currSection.latitude, currSection.latitudeIdx, Northeastern.longitude, Northeastern.longitudeIdx);
        if (Mathf.Abs(currSection.SubtractLongitude(eastern)) < lonSectionCount)
        {
            StartCoroutine(AddMapSectionColumns(1));
        }

        // Check whether new column needs to be added to west
        MapSection western = new MapSection(currSection.latitude, currSection.latitudeIdx, Southwestern.longitude, Southwestern.longitudeIdx);
        if (Mathf.Abs(currSection.SubtractLongitude(western)) < lonSectionCount)
        {
            StartCoroutine(AddMapSectionColumns(-1));
        }

        //int latDiff = CurrSection.SubtractLatitude(PrevSection);
        //int lonDiff = CurrSection.SubtractLongitude(PrevSection);

        //AddMapSectionRows(latDiff);
        //AddMapSectionColumns(lonDiff);
    }

    void RemoveFarTerrains()
    {
        // Check whether northern row needs to be removed
        MapSection northern = new MapSection(Northeastern.latitude, Northeastern.latitudeIdx, currSection.longitude, currSection.longitudeIdx);
        if (Mathf.Abs(currSection.SubtractLatitude(northern)) > latSectionCount + sectionBufferSize)
        {
            RemoveMapSectionRows(1);
        }

        // Check whether southern row needs to be removed
        MapSection southern = new MapSection(Southwestern.latitude, Southwestern.latitudeIdx, currSection.longitude, currSection.longitudeIdx);
        if (Mathf.Abs(currSection.SubtractLatitude(southern)) > latSectionCount + sectionBufferSize)
        {
            RemoveMapSectionRows(-1);
        }

        // Check whether eastern column needs to be removed
        MapSection eastern = new MapSection(currSection.latitude, currSection.latitudeIdx, Northeastern.longitude, Northeastern.longitudeIdx);
        if (Mathf.Abs(currSection.SubtractLongitude(eastern)) > lonSectionCount + sectionBufferSize)
        {
            RemoveMapSectionColumns(1);
        }

        // Check whether western column needs to be removed
        MapSection western = new MapSection(currSection.latitude, currSection.latitudeIdx, Southwestern.longitude, Southwestern.longitudeIdx);
        if (Mathf.Abs(currSection.SubtractLongitude(western)) > lonSectionCount + sectionBufferSize)
        {
            RemoveMapSectionColumns(-1);
        }
    }

    public enum ZoomLevel
    {
        Eleven = 11,
        Thirteen = 13,
        Sixteen = 16,
        Eighteen = 18,
        Invalid = -1
    }
}
