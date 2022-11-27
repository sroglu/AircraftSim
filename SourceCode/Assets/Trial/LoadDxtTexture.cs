using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadDxtTexture : MonoBehaviour
{
    [SerializeField]
    Material terrainMaterial;

    int textureBufferSize = 256 * 1024;        // 600 kb
    byte[] textureBuffer;

    MemoryStream memStream;

    public string terrainDataPath = @"D:\Dted1\DT1";

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the texture queue
        textureBuffer = new byte[textureBufferSize];

        int twoHundredMB = 209715200;
        memStream = new MemoryStream(twoHundredMB);

        LoadMesh();
        LoadTexture();
    }

    void LoadTexture()
    {
        string texturePath = $"{terrainDataPath}\\TerrainTexture\\16\\39_32\\39_1_32_1.dxt1";

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
                        //Debug.Log($"Buraya {count} kere girdi");
                        //count++;
                        memStream.Write(textureBuffer, 0, byteRead);
                    }

                    ddsBytes = memStream.ToArray();
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
                    UnityEngine.Profiling.Profiler.BeginSample("Copying array");
                    byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];

                    Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);
                    UnityEngine.Profiling.Profiler.EndSample();

                    Texture2D texture = new Texture2D(width, height, textureFormat, false);
                    Texture2D tmpTexture = new Texture2D(width, height, textureFormat, false);
                    tmpTexture.LoadRawTextureData(dxtBytes);
                    tmpTexture.Apply();
                    Graphics.CopyTexture(tmpTexture, texture);
                    //Destroy(tmpTexture);

                    GetComponent<MeshRenderer>().material = terrainMaterial;
                    GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tmpTexture);
                    //GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
                }
            }
        }
    }

    void LoadMesh()
    {

    }
}
