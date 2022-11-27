using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;



[System.Serializable]
public struct HudDataAMC
{
    public double kias_kts;
    public double nz_g;
    public double mach_number;
    public double altitude_ft;
    public double radar_altitude_ft;
    public double vertical_speed_fpm;
    public double yaw_deg;
    public double roll_deg;
    public double pitch_deg;
    public double angle_of_attack_deg;
    public double angle_of_slide_deg;
    public double gamma_deg;
    public double throttle;
    public double lateral_fpm_deg;
    public double target_roll_deg;
    public double target_pitch_deg;
    public double target_lateral_deg;
    public double pipper_red;
    public double lat;
    public double lon;
}


[System.Serializable]
public struct HudDataAMCv2
{
    public double kias_kts;
    public double nz_g;
    public double mach_number;
    public double altitude_ft;
    public double radar_altitude_ft;
    public double vertical_speed_fpm;
    public double yaw_deg;
    public double roll_deg;
    public double pitch_deg;
    public double aoa_deg;
    public double aos_deg;
    public double gamma_deg;
    public double throttle;
    public double lateral_fpm_deg;
    public double target_roll_deg;
    public double target_pitch_deg;
    public double target_lateral_fpm_deg;
    public double pipper_red;
    public double theta_correctness;
    public double chevron_dist;
    public double chevron_status;

    /*--------------------------------------------------------------------------------------------------------------------------*/
    //                                             HUD AIR TO AIR DATA(23 new value)                                             //

    public double target_aspect_deg;
    public double asc_azimuth_deg;
    public double asc_elevation_deg;
    public double asec_radius;
    public double raero_range;
    public double roptimum_range;
    public double rmax_range;
    public double rmaneuvermax_range;
    public double maneuvermin_range;
    public double targetcarret_range;
    public double rmin_range;
    public double target_closure_rate;
    public double loft_solution_cue;
    public double post_launch_time_remaining;                               //TODO DELETE
    public double target_azimuth_deg;
    public double target_eleaviton_deg;
    public double is_asec_locked;
    public double pre_post_launch_selection;
    public double is_navigation_mode;
    public double target_aircraft_speed;
    public double time_until_intercept;
    public double time_until_active;
    public double time_until_mprf;
}


public class HudCommunicationAMC : Communication<HudDataAMC,object>
{
    public HudCommunicationAMC(CommunicationConfig configData) : base(configData)
    {
    }

    protected override void InitRcvData(out object rcvData)
    {
        rcvData=null;
    }

    protected override void InitSndData(out byte[] sndData)
    {
        sndData = new byte[Marshal.SizeOf(typeof(HudDataAMC))];
    }

    protected override bool PackData(HudDataAMC data, ref byte[] sndData)
    {
        bool result = false;
        try
        {
            Array.Copy(BitConverter.GetBytes(data.kias_kts),            0, sndData, 0,                  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.nz_g),                0, sndData, sizeof(double),     sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.mach_number),         0, sndData, sizeof(double) *2,  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.altitude_ft),         0, sndData, sizeof(double) *3,  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.radar_altitude_ft),   0, sndData, sizeof(double) *4,  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.vertical_speed_fpm),  0, sndData, sizeof(double) *5,  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.yaw_deg),             0, sndData, sizeof(double) *6,  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.roll_deg),            0, sndData, sizeof(double) *7,  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.pitch_deg),           0, sndData, sizeof(double) *8,  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.angle_of_attack_deg), 0, sndData, sizeof(double) *9,  sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.angle_of_slide_deg),  0, sndData, sizeof(double) *10, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.gamma_deg),           0, sndData, sizeof(double) *11, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.throttle),            0, sndData, sizeof(double) *12, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.lateral_fpm_deg),     0, sndData, sizeof(double) *13, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_roll_deg),     0, sndData, sizeof(double) *14, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_pitch_deg),    0, sndData, sizeof(double) *15, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_lateral_deg),  0, sndData, sizeof(double) *16, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.pipper_red),          0, sndData, sizeof(double) *17, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.lat),                 0, sndData, sizeof(double) *18, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.lon),                 0, sndData, sizeof(double) *19, sizeof(double));

            result = true;
        }
        catch (Exception ex)
        {

        }
        return result;
    }

    protected override bool UnPackData(byte[] data, ref object rcvData)
    {
        return false;
    }
}

public class HudCommunicationAMCv2 : Communication<HudDataAMCv2, object>
{
    public HudCommunicationAMCv2(CommunicationConfig configData) : base(configData)
    {
    }

    protected override void InitRcvData(out object rcvData)
    {
        rcvData = null;
    }

    protected override void InitSndData(out byte[] sndData)
    {
        sndData = new byte[Marshal.SizeOf(typeof(HudDataAMCv2))];
        //Debug.Log("Size: "+ sndData.Length+"  "+sizeof(int)+"  "+sizeof(double));
    }

    protected override bool PackData(HudDataAMCv2 data, ref byte[] sndData)
    {
        bool result = false;
        try
        {

            Array.Copy(BitConverter.GetBytes(data.kias_kts),                0, sndData, 0, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.nz_g),                    0, sndData, sizeof(double), sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.mach_number),             0, sndData, sizeof(double) * 2, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.altitude_ft),             0, sndData, sizeof(double) * 3, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.radar_altitude_ft),       0, sndData, sizeof(double) * 4, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.vertical_speed_fpm),      0, sndData, sizeof(double) * 5, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.yaw_deg),                 0, sndData, sizeof(double) * 6, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.roll_deg),                0, sndData, sizeof(double) * 7, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.pitch_deg),               0, sndData, sizeof(double) * 8, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.aoa_deg),                 0, sndData, sizeof(double) * 9, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.aos_deg),                 0, sndData, sizeof(double) * 10, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.gamma_deg),               0, sndData, sizeof(double) * 11, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.throttle),                0, sndData, sizeof(double) * 12, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.lateral_fpm_deg),         0, sndData, sizeof(double) * 13, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_roll_deg),         0, sndData, sizeof(double) * 14, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_pitch_deg),        0, sndData, sizeof(double) * 15, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_lateral_fpm_deg),  0, sndData, sizeof(double) * 16, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.pipper_red),              0, sndData, sizeof(double) * 17, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.theta_correctness),       0, sndData, sizeof(double) * 18, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.chevron_dist),            0, sndData, sizeof(double) * 19, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.chevron_status),          0, sndData, sizeof(double) * 20, sizeof(double));

            /*--------------------------------------------------------------------------------------------------------------------------*/
            //                                             HUD AIR TO AIR DATA(23 new value)                                            //
            Array.Copy(BitConverter.GetBytes(data.target_aspect_deg),           0, sndData, sizeof(double) * 21, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.asc_azimuth_deg),             0, sndData, sizeof(double) * 22, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.asc_elevation_deg),           0, sndData, sizeof(double) * 23, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.asec_radius),                 0, sndData, sizeof(double) * 24, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.raero_range),                 0, sndData, sizeof(double) * 25, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.roptimum_range),              0, sndData, sizeof(double) * 26, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.rmax_range),                  0, sndData, sizeof(double) * 27, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.rmaneuvermax_range),          0, sndData, sizeof(double) * 28, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.maneuvermin_range),           0, sndData, sizeof(double) * 29, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.targetcarret_range),          0, sndData, sizeof(double) * 30, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.rmin_range),                  0, sndData, sizeof(double) * 31, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_closure_rate),         0, sndData, sizeof(double) * 32, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.loft_solution_cue),           0, sndData, sizeof(double) * 33, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.post_launch_time_remaining),  0, sndData, sizeof(double) * 34, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_azimuth_deg),          0, sndData, sizeof(double) * 35, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_eleaviton_deg),        0, sndData, sizeof(double) * 36, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.is_asec_locked),              0, sndData, sizeof(double) * 37, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.pre_post_launch_selection),   0, sndData, sizeof(double) * 38, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.is_navigation_mode),          0, sndData, sizeof(double) * 39, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.target_aircraft_speed),       0, sndData, sizeof(double) * 40, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.time_until_intercept),        0, sndData, sizeof(double) * 41, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.time_until_active),           0, sndData, sizeof(double) * 42, sizeof(double));
            Array.Copy(BitConverter.GetBytes(data.time_until_mprf),             0, sndData, sizeof(double) * 43, sizeof(double));


            result = true;
        }
        catch (Exception ex)
        {

        }
        return result;
    }

    protected override bool UnPackData(byte[] data, ref object rcvData)
    {
        return false;
    }
}
