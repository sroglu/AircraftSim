using System;
using System.IO;
using System.Text;
using UnityEngine;
public enum TerrainLayer
{
    initial,
    surface,
    texture,
    building,
    tree,
    water,
}
public enum Extentions
{
    ind,
    vert,
    timg,
}

public abstract class TerrainLayerLoader : IDisposable
{
    public static string RootDir;
    protected TerrainLayer layer;
    bool disposed = false;
    protected TerrainLayerLoader nextLoader;

    string dataFolderName;

    string DataFolderPath { get { return Path.Combine(RootDir, dataFolderName); } }
    protected string DataPath (string fileName)
    {  return Path.Combine(DataFolderPath, fileName);  }


    public TerrainLayerLoader(TerrainLayer layer, string dataFolderName, TerrainLayerLoader nextLoader)
    {
        this.layer = layer;
        this.dataFolderName = dataFolderName;
        this.nextLoader = nextLoader;
        disposed = false;
    }

    public Transform Load(TerrainSegment segment,float LOD, Transform tile = null)
    {
        if (CheckSegmentCanBeLoaded(segment))
            LoadLayer(segment, LOD, ref tile);

        nextLoader?.Load(segment, LOD, tile);
        return tile;
    }

    protected abstract bool LoadLayer(TerrainSegment segment, float LOD, ref Transform tile);

    protected abstract bool CheckSegmentCanBeLoaded(TerrainSegment segment);

    #region Partitioning



    #endregion

    #region Representation
    StringBuilder representation;
    char seperator = '_';
    protected string GetLayerRepresentation(TerrainSegment segment)
    {
        representation.Clear();
        representation.Append(segment.representation);

        string representationAddition = RepresentationAddition();
        if (representationAddition != null)
        {
            representation.Append(seperator);
            representation.Append(RepresentationAddition());
        }

        return representation.ToString();
    }
    protected virtual string RepresentationAddition() { return layer.ToString(); }
    #endregion



    public void Dispose()
    {
        disposed = true;
    }
}
