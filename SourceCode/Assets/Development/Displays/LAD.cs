using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using UnityEngine.InputSystem;
using static ManagementActions;

public class LAD : MonoBehaviour
{
    public Vector2Int ladScreenSize=new Vector2Int(2560, 1024);
    int LadPixelNumber{ get{ return ladScreenSize.x * ladScreenSize.y; } }



    #region Touch Variables (out)
    public LayerMask layerMask;
    LadCommunication ladCom;

    [SerializeField]
    bool useTouchFeedbackObj=false;

    [SerializeField]
    bool handleMouseClicks = false;
    [SerializeField]
    bool mousePressed, mouseReleased, mousePress,mouseUnpress;

    [SerializeField]
    TouchDataArr touchDataArr;
    Dictionary<Transform,int> fingerPoints;

    Vector2 mousePointOnLAD;
    Vector3 ladHitPoint;
    #endregion

    #region Display Variables (in)
    int bufferSize;
    int[] offsetValues;
    Vector2Int[] pictureSize;
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

    string szMapName = "VapsCaptureVRShmem";

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

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        SetupTouch();
        SetupDisplay();
    }

    private void FixedUpdate()
    {
        FillTouchData();
        FillHOTASData();

        //Debug.Log(touchDataArr?.dataArr[0]?.touchType+"   "+ touchDataArr?.dataArr[0]?.x+"-"+ touchDataArr?.dataArr[0]?.y);
        ladCom.SendData(touchDataArr);


        //Debug.Log((FeedbackUI.Instance!=null)+" "+touchDataArr?.dataArr[0]?.touchType + "   " + touchDataArr?.dataArr[0]?.x + "-" + touchDataArr?.dataArr[0]?.y);

        FeedbackUI.Instance?.ShowVals(touchDataArr?.dataArr[0]?.x.ToString(), touchDataArr?.dataArr[0]?.y.ToString(), touchDataArr?.dataArr[0]?.touchType.ToString(), ladCom.IP.ToString()+":"+ladCom.sndPort.ToString(), 
            InputManager.Instance.HotasInputData.cursorMovement.ToString(),
            InputManager.Instance.HotasInputData.DMS_TMS_Data.ToString());

    }

    private void Update()
    {
        
        HandleDisplay();

        mousePressed = mouseInput.Press.triggered;
        mouseReleased = mouseInput.Release.triggered;

        if (mousePressed && !mousePress) mousePress = true;
        else if (mouseReleased && mousePress) { mousePress = false; mouseUnpress = true; }


        thereIsPointOnLad = handleMouseClicks && mousePress ?
                                    ScreenPointRaycasting(mouseInput.Position.ReadValue<Vector2>(), ref ladHitPoint) :
                                    false;

        if (thereIsPointOnLad)
            ShowFeedbackObj(ladHitPoint);
        else
            if (fingerPoints.Keys.Count > 0)
                ShowFeedbackObj(fingerPoints.ElementAt(0).Key.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((layerMask | 1 << other.gameObject.layer) == layerMask)
        {
            if (fingerPoints.ContainsKey(other.transform))
            {
                fingerPoints[other.transform] = 1;
            }
            else
            {
                fingerPoints.Add(other.transform, 1);
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if ((layerMask | 1 << other.gameObject.layer) == layerMask)
        {
            if (fingerPoints.ContainsKey(other.transform))
            {
                fingerPoints[other.transform] = 2;
            }
        }
    }
    #region TouchMethods
    Vector2 transformedPoint;

    Renderer feedbackObj;
    private void SetupTouch()
    {
        CommunicationConfig confData = Utility.ReadJsonDataFromStreamingAssets<CommunicationConfig>("ladComConfig.json");
        ladCom = new LadCommunication(confData);

        touchDataArr = new TouchDataArr();
        touchDataArr.dataArr = new TouchData[5];
        touchDataArr.hotasData = new HotasData();

        for (int i = 0; i < touchDataArr.dataArr.Length; i++)
        {
            touchDataArr.dataArr[i] = new TouchData();
        }

        fingerPoints = new Dictionary<Transform, int>();


        //Feedback setup
        if (useTouchFeedbackObj)
        {
            feedbackObj = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<Renderer>();
            feedbackObj.transform.parent = transform;
            feedbackObj.transform.localScale = Vector3.one / 16;
            feedbackObj.transform.parent = null;
            feedbackObj.transform.localScale = Vector3.one* feedbackObj.transform.localScale.y;
            feedbackObj.transform.parent = transform;

            feedbackObj.material.shader = Shader.Find("Universal Render Pipeline/Unlit");
            feedbackObj.material.color= Color.red;
            feedbackObj.transform.name = "FeedbackObj";

            Destroy(feedbackObj.GetComponent<Collider>());


        }

        //Input setup
        mouseInput = InputManager.Instance.ManagerInputs.Mouse;

    }
    [SerializeField]
    MouseActions mouseInput;
    bool thereIsPointOnLad;
    void FillTouchData()
    {
        touchDataArr.count = fingerPoints.Count;

        for (int i = 0; i < touchDataArr.dataArr.Length; i++)
        {
            touchDataArr.dataArr[i] = new TouchData();
        }

        if (handleMouseClicks)
        {
            if(!mouseUnpress)
                mousePointOnLAD = Vector2.zero;

            if (thereIsPointOnLad)
            {
                mousePointOnLAD = GetTouchPointOnLAD(ladHitPoint);
            }



            touchDataArr.count = 1;
            touchDataArr.dataArr[0].x = mousePointOnLAD.x;
            touchDataArr.dataArr[0].y = mousePointOnLAD.y;
            touchDataArr.dataArr[0].touchType = mouseUnpress ? 2 :
                                                     mousePress ? 1 : 0;

            if (mouseUnpress && !mousePress)
            {
                mouseUnpress=false;
            }

        }
        else
        {
            for (int i = 0; i < fingerPoints.Keys.Count; i++)
            {
                var point = fingerPoints.ElementAt(i);
                mousePointOnLAD = GetTouchPointOnLAD(point.Key.position);

                touchDataArr.dataArr[i].x = mousePointOnLAD.x;
                touchDataArr.dataArr[i].y = mousePointOnLAD.y;
                touchDataArr.dataArr[i].touchType = point.Value;


                //If it is up turn it to the none
                if (point.Value == 2)
                    fingerPoints[point.Key] = 0;
            }


        }


    }


    void FillHOTASData()
    {
        touchDataArr.hotasData.cursorUp = Convert.ToInt32(InputManager.Instance.HotasInputData.cursorMovement == Vector2.up);
        touchDataArr.hotasData.cursorDown = Convert.ToInt32(InputManager.Instance.HotasInputData.cursorMovement == Vector2.down);
        touchDataArr.hotasData.cursorRight = Convert.ToInt32(InputManager.Instance.HotasInputData.cursorMovement == Vector2.right);
        touchDataArr.hotasData.cursorLeft = Convert.ToInt32(InputManager.Instance.HotasInputData.cursorMovement == Vector2.left);


        touchDataArr.hotasData.tmsUp = Convert.ToInt32(InputManager.Instance.HotasInputData.DMS_TMS_Data == Vector2.up);
        touchDataArr.hotasData.tmsDown = Convert.ToInt32(InputManager.Instance.HotasInputData.DMS_TMS_Data == Vector2.down);
        touchDataArr.hotasData.dmsRight = Convert.ToInt32(InputManager.Instance.HotasInputData.DMS_TMS_Data == Vector2.right);
        touchDataArr.hotasData.dmsLeft = Convert.ToInt32(InputManager.Instance.HotasInputData.DMS_TMS_Data == Vector2.left);

    }

    #endregion

    #region DisplayMethods
    private void SetupDisplay()
    {
        bufferSize = (2560 * 1024 + 1280 * 512 + 640 * 256 + 320 * 128) * 4;

        offsetValues = new int[]{ 0, (2560 * 1024) * 4, (2560 * 1024 + 1280 * 512) * 4, (2560 * 1024 + 1280 * 512 + 640 * 256) * 4 };

         pictureSize = new Vector2Int[]{ new Vector2Int(2560, 1024), new Vector2Int(1280, 512), new Vector2Int(640, 256), new Vector2Int(320, 128) };


        // width, height, format, mipChain
        tex = new Texture2D(pictureSize[selectedPictureIndex].x, pictureSize[selectedPictureIndex].y, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Trilinear;

        defaultMat = GetComponent<Renderer>().material;

        sHandle = new SafeFileHandle(hHandle, true);
        attachSuccessful = Attach(szMapName, bufferSize);
    }
    
    private void HandleDisplay()
    {
        if (!attachSuccessful)
        {
            attachSuccessful = Attach(szMapName, bufferSize);
            return;
        }
        else
        {
            IntPtr ptr = IntPtr.Add(pBuffer, offsetValues[selectedPictureIndex]);
            tex.LoadRawTextureData(ptr, pictureSize[selectedPictureIndex].x * pictureSize[selectedPictureIndex].y * sizeof(int));
            tex.Apply(true);
            defaultMat.mainTexture = tex;
        }
    }


    void ShowFeedbackObj(Vector3 hitpoint)
    {
        if (useTouchFeedbackObj && feedbackObj != null)
        {
            feedbackObj.transform.position = hitpoint;
        }
    }
    private bool ScreenPointRaycasting(Vector2 touchPosition, ref Vector3 hitPoint)
    {
        //Debug.Log("touchPosition: "+ touchPosition);

        RaycastHit hit;
        if (touchPosition.x < 0 || touchPosition.x > Screen.width || touchPosition.y < 0 || touchPosition.y > Screen.height)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        Debug.DrawRay(ray.origin, ray.direction* float.MaxValue,Color.cyan);
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, 1 << gameObject.layer))
        {
            hitPoint = hit.point;
            //Debug.Log("hitPoint: "+ hitPoint);
            return true;
        }

        return false;
    }

    private Vector2 GetTouchPointOnLAD(Vector3 hitPoint)
    {
        transformedPoint = transform.InverseTransformPoint(hitPoint);
        return new Vector2(
        (transformedPoint.x + 0.5f) * ladScreenSize.x,
        (transformedPoint.y + 0.5f) * ladScreenSize.y);
    }

    public bool Attach(string SharedMemoryName, int NumBytes)
    {
        if (!sHandle.IsInvalid) return false;

        sHandle = OpenFileMapping(FILE_MAP_ALL_ACCESS, false, SharedMemoryName);

        if (sHandle.IsInvalid) return false;

        Debug.Log("Shared mem open SUCCESS for LAD");
        pBuffer = MapViewOfFile(sHandle, FILE_MAP_ALL_ACCESS, 0, 0, new UIntPtr((uint)NumBytes));

        return true;
    }

    public void Detach()
    {
        if (!sHandle.IsInvalid && !sHandle.IsClosed)
        {
            sHandle.Close();
        }
        pBuffer = IntPtr.Zero;
    }
    #endregion

    private void OnApplicationQuit()
    {
        if (ladCom != null)
            ladCom.Dispose();

        if (attachSuccessful)
            Detach();
    }
}
