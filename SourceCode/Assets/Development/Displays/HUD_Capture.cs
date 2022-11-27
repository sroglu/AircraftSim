using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class HUD_Capture : MonoBehaviour
{
    public string windowName;
    public string procName;
    private string wasWindowName = "";
    private int nWinHandle;
    public Rect rect = new Rect(Vector2.zero, new Vector2(1520, 840));
    public UnityEngine.Color cutoutColor= UnityEngine.Color.black;
    public float cutoutTreshold = 0.1f;
    [SerializeField]
    private Texture2D texture;
    WindowsUtils.ScreenCapture.ScreenCapture screenCapture = new WindowsUtils.ScreenCapture.ScreenCapture();
    IntPtr iH = (IntPtr)null;
    Bitmap bitmap = null;

    public UnityEngine.UI.Image hudImg;
    HudCommunicationAMCv2 hudCom;

    bool initiated = false;
    // Start is called before the first frame update
    void Awake()
    {
        texture = new Texture2D(1520, 840);
        hudImg.transform.localScale = new Vector2(1,-1);


        //Com
        CommunicationConfig confData = Utility.ReadJsonDataFromStreamingAssets<CommunicationConfig>("hudComConfigCapture.json");
        //hudCom = new HudCommunicationAMC(confData);
        hudCom = new HudCommunicationAMCv2(confData);




        hudData = new HudDataAMCv2();
        initiated = true;
    }


    private void FixedUpdate()
    {
        UpdateWindowHandle();
        byte[] byteArrayDesktopImage;
        bool canGetImg = GrabDesktopImp(out byteArrayDesktopImage);

        SetTextureFromByteArray(ref byteArrayDesktopImage);
    }

    void UpdateWindowHandle()
    {
        iH = (IntPtr)null;
        GetWindowHandle();
        GetWindowHandleByProcessName();
    }
    void GetWindowHandle()
    {
        if (iH == (IntPtr)null && windowName.Length > 0)
            iH = screenCapture.WinGetHandle(windowName);
    }

    void GetWindowHandleByProcessName()
    {
        if (iH == (IntPtr)null && procName.Length > 0)
            iH = screenCapture.WinGetHandleByProcessName(procName);
    }

    bool GrabDesktopImp(out byte[] outputByteArray)
    {
        if (iH != (IntPtr)null || (rect.width * rect.height != 0))
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
        if (byteArray != null && hudImg != null)
        {
            texture.Reinitialize(bitmap.Width, bitmap.Height, TextureFormat.BGRA32, false);
            texture.LoadRawTextureData(byteArray);
            texture.Apply();

            hudImg.material.SetTexture("_MainTex", texture);

            //renderer.material.SetTexture("_BaseColorMap", texture);
        }
    }


    void FillHudData(ref HudDataAMC data)
    {
        data.kias_kts = GameManager.PlayerAircraft.CurrentAircraftData.kias_kts;
        data.nz_g = GameManager.PlayerAircraft.CurrentAircraftData.nz_g;
        data.mach_number = GameManager.PlayerAircraft.CurrentAircraftData.mach_number;
        data.altitude_ft = (GameManager.PlayerAircraft.CurrentAircraftData.transform.alt + Constants.OFFSET_BY_TERRAIN) / Constants.FEET_TO_METER;
        data.radar_altitude_ft = GameManager.PlayerAircraft.CurrentAircraftData.radar_altitude_ft;
        data.vertical_speed_fpm = GameManager.PlayerAircraft.CurrentAircraftData.vertical_speed_fpm;
        data.yaw_deg = GameManager.PlayerAircraft.CurrentAircraftData.transform.yaw;
        data.roll_deg = GameManager.PlayerAircraft.CurrentAircraftData.transform.roll;
        data.pitch_deg = GameManager.PlayerAircraft.CurrentAircraftData.transform.pitch;
        data.angle_of_attack_deg = GameManager.PlayerAircraft.CurrentAircraftData.angle_of_attack;
        data.angle_of_slide_deg = GameManager.PlayerAircraft.CurrentAircraftData.angle_of_slide_deg;
        data.gamma_deg = GameManager.PlayerAircraft.CurrentAircraftData.gamma_deg;
        data.throttle = GameManager.PlayerAircraft.CurrentAircraftData.throttle;
        data.lateral_fpm_deg = GameManager.PlayerAircraft.CurrentAircraftData.lateral_fpm_deg;
        data.target_roll_deg = GameManager.PlayerAircraft.CurrentAircraftData.target_roll_deg;
        data.target_pitch_deg = GameManager.PlayerAircraft.CurrentAircraftData.target_pitch_deg;
        data.target_lateral_deg = GameManager.PlayerAircraft.CurrentAircraftData.target_lateral_deg;
        data.pipper_red = GameManager.PlayerAircraft.CurrentAircraftData.pipper_red;
        data.lat = GameManager.PlayerAircraft.CurrentAircraftData.transform.lat;
        data.lon = GameManager.PlayerAircraft.CurrentAircraftData.transform.lon;
    }

    void FillHudData(ref HudDataAMCv2 data)
    {
        data.kias_kts = GameManager.PlayerAircraft.CurrentAircraftData.kias_kts;
        data.nz_g = GameManager.PlayerAircraft.CurrentAircraftData.nz_g;
        data.mach_number = GameManager.PlayerAircraft.CurrentAircraftData.mach_number;
        data.altitude_ft = (GameManager.PlayerAircraft.CurrentAircraftData.transform.alt + Constants.OFFSET_BY_TERRAIN) * Constants.METER_TO_FEET;
        data.radar_altitude_ft = GameManager.PlayerAircraft.CurrentAircraftData.radar_altitude_ft;
        data.vertical_speed_fpm = GameManager.PlayerAircraft.CurrentAircraftData.vertical_speed_fpm;
        data.yaw_deg = GameManager.PlayerAircraft.CurrentAircraftData.transform.yaw;
        data.roll_deg = GameManager.PlayerAircraft.CurrentAircraftData.transform.roll;
        data.pitch_deg = GameManager.PlayerAircraft.CurrentAircraftData.transform.pitch;
        data.aoa_deg = GameManager.PlayerAircraft.CurrentAircraftData.angle_of_attack;
        data.aos_deg = GameManager.PlayerAircraft.CurrentAircraftData.angle_of_slide_deg;
        data.gamma_deg = GameManager.PlayerAircraft.CurrentAircraftData.gamma_deg;
        data.throttle = GameManager.PlayerAircraft.CurrentAircraftData.throttle;
        data.lateral_fpm_deg = GameManager.PlayerAircraft.CurrentAircraftData.lateral_fpm_deg;
        data.target_roll_deg = GameManager.PlayerAircraft.CurrentAircraftData.target_roll_deg;
        data.target_pitch_deg = GameManager.PlayerAircraft.CurrentAircraftData.target_pitch_deg;
        data.target_lateral_fpm_deg = GameManager.PlayerAircraft.CurrentAircraftData.target_lateral_deg;
        data.pipper_red = GameManager.PlayerAircraft.CurrentAircraftData.pipper_red;
        data.theta_correctness = 0;
        data.chevron_dist = 0;
        data.chevron_status = 0;
        data.target_aspect_deg = 0;
        data.asc_azimuth_deg = 0;
        data.asc_elevation_deg = 0;
        data.asec_radius = 0;
        data.raero_range = 0;
        data.roptimum_range = 0;
        data.rmax_range = 0;
        data.rmaneuvermax_range = 0;
        data.maneuvermin_range = 0;
        data.targetcarret_range = 0;
        data.rmin_range = 0;
        data.target_closure_rate = 0;
        data.loft_solution_cue = 0;
        data.post_launch_time_remaining = 0;
        data.target_azimuth_deg = 0;
        data.target_eleaviton_deg = 0;
        data.is_asec_locked = 0;
        data.pre_post_launch_selection = 0;
        data.is_navigation_mode = 0;
        data.target_aircraft_speed = 0;
        data.time_until_intercept = 0;
        data.time_until_active = 0;
        data.time_until_mprf = 0;
    }


    HudDataAMCv2 hudData;
    // Update is called once per frame
    void Update()
    {
        if (initiated)
        {
            FillHudData(ref hudData);

            hudCom.SendData(hudData);
        }
    }




































    /*
    public string windowName;
    private string wasWindowName = "";
    private int nWinHandle;
    public Rect rect = new Rect(Vector2.zero, new Vector2(500, 500));

    private Texture2D texture;
    private Renderer renderer;

    WindowsUtils.ScreenCapture.ScreenCapture screenCapture = new WindowsUtils.ScreenCapture.ScreenCapture();
    IntPtr iH = (IntPtr)null;
    Bitmap bitmap = null;

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
        if (iH != (IntPtr)null || (rect.width * rect.height != 0))
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
        if (byteArray != null && renderer != null)
        {
            texture.Reinitialize(bitmap.Width, bitmap.Height, TextureFormat.BGRA32, false);
            texture.LoadRawTextureData(byteArray);
            texture.Apply();

            renderer.material.SetTexture("_BaseColorMap", texture);
        }
    }
    */
}
