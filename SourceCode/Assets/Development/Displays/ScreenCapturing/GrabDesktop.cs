using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GrabDesktop : MonoBehaviour
{
    public string windowName;
    private string wasWindowName="";
    private int nWinHandle;
    private GameObject surface;
    public Rect rect=new Rect(Vector2.zero,new Vector2(500,500));

    private Texture2D texture;
    private Renderer renderer;

    WindowsUtils.ScreenCapture.ScreenCapture screenCapture = new WindowsUtils.ScreenCapture.ScreenCapture();
    IntPtr iH = (IntPtr)null;
    Bitmap bitmap=null;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        texture = new Texture2D(256, 256);
    }

    private void FixedUpdate()
    {
        GetWindowHandle();

        byte[] byteArrayDesktopImage;
        bool canGetImg = GrabDesktopImp(out byteArrayDesktopImage);

        SetTextureFromByteArray(ref byteArrayDesktopImage);
    }

    void GetWindowHandle()
    {
        iH = (IntPtr)null;
        if (windowName.Length > 0)
            iH = screenCapture.WinGetHandle(windowName);
    }

    bool GrabDesktopImp(out byte[] outputByteArray)
    {
        if(iH!=(IntPtr)null ||(rect.width*rect.height !=0))
        {
            bitmap = screenCapture.GetScreenshot(iH, (int)rect.top, (int)rect.left, (int)rect.height, (int)rect.width);
            if (bitmap != null)
            {
                //Wrap screen image onto object surface,as albedo color wrap
                outputByteArray = screenCapture.BitmapToByteArray(bitmap);
                return true;
            }
        }
        outputByteArray = null;
        return false;
    }

    void SetTextureFromByteArray(ref byte[] byteArray)
    {
        if (byteArray != null&&renderer!=null)
        {
            texture.Reinitialize(bitmap.Width, bitmap.Height, TextureFormat.BGRA32, false);
            texture.LoadRawTextureData(byteArray);
            texture.Apply();

            renderer.material.SetTexture("_BaseColorMap", texture);
        }
    }

}
