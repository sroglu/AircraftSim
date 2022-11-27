using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using WindowsUtils.WindowGetter;
using UnityEngine;
using System.Diagnostics;

namespace WindowsUtils.ScreenCapture
{
    public class ScreenCapture
    {
        [DllImport("user32.dll")]
        static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);
        [DllImport("user32.dll")]
        static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn);

        const int ERROR = 0;
        const int NULLREGION = 1;
        const int SIMPLEREGION = 2;
        const int COMPLEXREGION = 3;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hWnd, IntPtr hDC, uint nFlags);

        private string wasWindowName = "";
        private int nWinHandle;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }
            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }
            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }
            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(X, Y); }
                set { X = value.X; Y = value.Y; }
            }
            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }
            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }
            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Top && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        public Bitmap GetScreenshot(IntPtr ihandle, int top, int left, int height, int width)
        {
            if (ihandle != (IntPtr)null)
            {
                return GetWindowScreenShot(ihandle);
            }
            else
            {
                return GetWindowScreenShot(top, left, height, width);
            }
        }

        private Bitmap GetWindowScreenShot(IntPtr ihandle)
        {
            IntPtr hwnd = ihandle;
            IntPtr hdcBitmap = (IntPtr)0;
            RECT rect;
            Bitmap bmp=null;
            bool succeeded = false;
            System.Drawing.Graphics gfxBmp = null;

            try
            {
                GetWindowRect(new HandleRef(null, hwnd), out rect);
                bmp = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top, PixelFormat.Format32bppArgb);
                if (bmp == null)
                    return null;
                gfxBmp = System.Drawing.Graphics.FromImage(bmp);
                hdcBitmap = gfxBmp.GetHdc();
                succeeded = PrintWindow(hwnd, hdcBitmap, 0);
            }
            catch (Exception e)
            {
                succeeded = false;
                UnityEngine.Debug.Log("Graphics Error. Suggest restart Unity");
                //return null;
            }

            if (succeeded)
            {
                gfxBmp.ReleaseHdc(hdcBitmap);
            }
            else
            {
                //failed to grab window, then set a gray bitmap as a default window content
                gfxBmp.FillRectangle(new SolidBrush(System.Drawing.Color.Gray),new Rectangle(Point.Empty,bmp.Size));
            }

            return bmp;


            
            //Clip out the required region
            Region region = null;
            IntPtr hRgn = (IntPtr)0;

            try
            {
                hRgn = CreateRectRgn(0, 0, 0, 0);
                GetWindowRgn(hwnd, hRgn);
                region = Region.FromHrgn(hRgn);//err here once
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Region Error");
                UnityEngine.Debug.Log(hRgn);
                UnityEngine.Debug.Log(region);

            }

            if (!region.IsEmpty(gfxBmp))
            {
                gfxBmp.ExcludeClip(region);
                gfxBmp.Clear(System.Drawing.Color.Transparent);
            }

            try
            {
                return bmp;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            finally
            {
                gfxBmp.Dispose();
            }
            
        }

        private Bitmap GetWindowScreenShot(int top, int left, int height, int width)
        {
            //IntPtr hdcBitmap=(IntPtr)0;

            Bitmap bmp = null;
            Size s;
            System.Drawing.Graphics gfxBmp = null;

            //Get screen rect
            try
            {
                bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                s = new Size(bmp.Width, bmp.Height);
                gfxBmp = System.Drawing.Graphics.FromImage(bmp);
                gfxBmp.CopyFromScreen(left, top, 0, 0,s);
            }
            catch (Exception e)
            {
                bmp = null;
            }

            return bmp;
        }

        public void WriteBitmapToFile(string filename, Bitmap bitmap)
        {
            bitmap.Save(filename, ImageFormat.Jpeg);
        }


        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string sClass, string sWindow);

        public IntPtr WinGetHandle(string wName)
        {
            if (this.wasWindowName != wName || nWinHandle == 0)
            {
                //Only search for window if different name,and previously found. Otherwise window handle will be same as before.
                nWinHandle = LocateWindow(wName);
                if (nWinHandle != 0)
                    wasWindowName = wName;
                else
                {
                    nWinHandle = 0;
                    wasWindowName = "";
                }
            }
            return (IntPtr)nWinHandle;
        }


        public IntPtr WinGetHandleByProcessName(string procName)
        {
            if (this.wasWindowName != procName || nWinHandle == 0)
            {
                IntPtr windowHandle;
                Process[] processes = Process.GetProcessesByName(procName);

                foreach (Process p in processes)
                {
                    windowHandle = p.MainWindowHandle;
                    // do something with windowHandle
                    return windowHandle;
                }
            }
            return (IntPtr)nWinHandle;
        }

        int LocateWindow(string wName)
        {
            int ret = 0;
            //no name given
            if (wName.Length < 1)
                return ret;

            //Simple find gets the window
            ret = FindWindow(null, wName);
            if (ret != 0)
                return ret;

            //Search all top level windows for one that has some of name in it
            foreach (var window in OpenWindowGetter.GetOpenWindows())
            {
                if (window.Value.Contains(wName))
                    return (int)window.Key;
            }
            return ret;
        }

        public byte[] BitmapToByteArray(Bitmap bitmap)
        {
            byte[] bytedata=null;
            //Convert bitmap of captured image to a bytearray for a texture
            if (bitmap == null)
                return null;

            BitmapData bmpData = null;

            try
            {
                bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpData.Stride * bitmap.Height;
                bytedata = new byte[numbytes];
                IntPtr ptr = bmpData.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            finally
            {
                if (bmpData != null)
                    bitmap.UnlockBits(bmpData);
            }

            return bytedata;

        }



        public static System.Drawing.Color MakeTransparent(System.Drawing.Color c, int threshold)
        {   // calculate the weighed brightness:
            byte val = (byte)((c.R * 0.299f + c.G * 0.587f + c.B * 0.114f));
            return val < threshold ? System.Drawing.Color.FromArgb(0, c.R, c.G, c.B) : c;
        }

    }
}