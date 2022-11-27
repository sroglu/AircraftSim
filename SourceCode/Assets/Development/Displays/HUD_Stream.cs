using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public class HUD_Stream : MonoBehaviour
{

    int bufferSize = (1280 * 960) * 4;

    Vector2Int pictureSize = new Vector2Int(1280, 960);
    int selectedPictureIndex = 0;

    //Shared Memory
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern SafeFileHandle OpenFileMapping(
     uint dwDesiredAccess,
     bool bInheritHandle,
     string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(
    SafeFileHandle hFileMappingObject,
    UInt32 dwDesiredAccess,
    UInt32 dwFileOffsetHigh,
    UInt32 dwFileOffsetLow,
    UIntPtr dwNumberOfBytesToMap);

    Texture2D tex;
    Material defaultMat;

    string szMapName = "shmem";

    const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
    const UInt32 SECTION_QUERY = 0x0001;
    const UInt32 SECTION_MAP_WRITE = 0x0002;
    const UInt32 SECTION_MAP_READ = 0x0004;
    const UInt32 SECTION_MAP_EXECUTE = 0x0008;
    const UInt32 SECTION_EXTEND_SIZE = 0x0010;
    const UInt32 SECTION_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SECTION_QUERY |
        SECTION_MAP_WRITE |
        SECTION_MAP_READ |
        SECTION_MAP_EXECUTE |
        SECTION_EXTEND_SIZE);
    const UInt32 FILE_MAP_ALL_ACCESS = SECTION_ALL_ACCESS;
    private SafeFileHandle sHandle;
    private IntPtr hHandle;
    private IntPtr pBuffer;
    bool attachSuccessful;

    HudCommunication hudCom;



    void Start()
    {
        // width, height, format, mipChain
        tex = new Texture2D(pictureSize.x, pictureSize.y, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Trilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        defaultMat = GetComponent<Renderer>().material;

        sHandle = new SafeFileHandle(hHandle, true);
        attachSuccessful = Attach(szMapName, bufferSize);


        //Com
        CommunicationConfig confData = Utility.ReadJsonDataFromStreamingAssets<CommunicationConfig>("hudComConfigStream.json");
        hudCom = new HudCommunication(confData);
    }

    public bool Attach(string SharedMemoryName, int NumBytes)
    {
        if (!sHandle.IsInvalid) return false;

        sHandle = OpenFileMapping(FILE_MAP_ALL_ACCESS, false, SharedMemoryName);
        //Debug.Log("Shared mem open: ");

        if (sHandle.IsInvalid) return false;

        Debug.Log("Shared mem open SUCCESS for HUD");
        pBuffer = MapViewOfFile(sHandle, FILE_MAP_ALL_ACCESS, 0, 0, new UIntPtr((uint)NumBytes));
        //Debug.Log("Shared mem mapped: ");

        return true;
    }

    public void Detach()
    {
        if (!sHandle.IsInvalid && !sHandle.IsClosed)
        {
            //CloseHandle(hHandle); //fair to leak if can't close
            sHandle.Close();
        }
        pBuffer = IntPtr.Zero;
        //lBufferSize = 0;
    }

    private void FixedUpdate()
    {
        HudData hudData = new HudData(GameManager.PlayerAircraft.CurrentAircraftData);
        hudCom.SendData(hudData);
    }

    void Update()
    {
        if (!attachSuccessful)
        {
            attachSuccessful = Attach(szMapName, bufferSize);
            return;
        }
        else
        {
            IntPtr ptr = IntPtr.Add(pBuffer, 0);
            tex.LoadRawTextureData(ptr, pictureSize.x * pictureSize.y * 4);
            tex.Apply(true);
            // tex.Resize(1280, 512);
            defaultMat.mainTexture = tex;
        }
    }

    void OnApplicationQuit()
    {
        if (hudCom != null)
            hudCom.Dispose();

        if (attachSuccessful)
            Detach();
    }


}