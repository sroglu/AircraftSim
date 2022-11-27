using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

public class Utility
{

    // Calculates earth position using Lla2Ecef algorithm
    public static Vector3 CreatePositionByCoords(float lat, float lon, float alt)
    {
        float x, y, z, n, rho, lat_rad, lon_rad, sinphi, cosphi;

        lat_rad = lat * Mathf.Deg2Rad;
        lon_rad = lon * Mathf.Deg2Rad;

        sinphi = Mathf.Sin(lat_rad);
        cosphi = Mathf.Cos(lat_rad);

        //N = EarthRadiusInM / (1 - E2 * sinphi ** 2) ** 0.5; pythonic way
        n = (float)(Constants.EarthRadiusInM / Math.Pow(1 - Constants.E2 * Math.Pow(sinphi, 2), 0.5f));
        rho = (n + alt) * cosphi;

        y = (float)(n * (1 - Constants.E2) + alt) * sinphi;
        x = rho * Mathf.Cos(lon_rad);
        z = rho * Mathf.Sin(lon_rad);

        Vector3 on_earth_pos = new Vector3((float)x, (float)y, (float)z);
        return on_earth_pos;
    }

    static double DegToRad = Math.PI / 180.0;

    public static Vector3 CreatePositionByCoords(double lat, double lon, double alt)
    {
        double x, y, z, n, rho, lat_rad, lon_rad, sinphi, cosphi;

        lat_rad = lat * Mathf.Deg2Rad;
        lon_rad = lon * Mathf.Deg2Rad;

        sinphi = Math.Sin(lat_rad);
        cosphi = Math.Cos(lat_rad);

        //N = EarthRadiusInM / (1 - E2 * sinphi ** 2) ** 0.5; pythonic way

        n = Constants.EarthRadiusInM / Math.Pow(1 - Constants.E2 * Math.Pow(sinphi, 2), 0.5f);
        rho = (n + alt) * cosphi;

        y = (n * (1 - Constants.E2) + alt) * sinphi;
        x = rho * Math.Cos(lon_rad);
        z = rho * Math.Sin(lon_rad);

        Vector3 on_earth_pos = new Vector3((float)x, (float)y, (float)z);
        return on_earth_pos;
    }

    static Quaternion simRotationCorrection { get { return Quaternion.Euler(-90, 0, -90); } }
    public static Quaternion CreateBaseRotationByCoords(SimObjTransform coordTransform)
    {
        return Quaternion.Euler(0, -(float)coordTransform.lon, (float)coordTransform.lat) * Quaternion.Euler(0, 0, -90);
    }

    public static Quaternion CreateLocalRotationByCoords(SimObjTransform coordTransform)
    {
        return Quaternion.Euler(0, 0, 90) *
            Quaternion.Euler((float)coordTransform.yaw, 0, (float)-coordTransform.pitch) *
            Quaternion.Euler(0, (float)-coordTransform.roll, 0) *
            simRotationCorrection; ;
    }


    public static (int, int, int, int) SplitName(GameObject terrain)
    {
        string name;
        int latDeg, latIdx, lonDeg, lonIdx;

        // Example name: '39_2_32_5'
        name = terrain.name;

        // Parse string and convert to int
        latDeg = System.Int32.Parse(name.Substring(0, 2));
        latIdx = System.Int32.Parse(name[3].ToString());
        lonDeg = System.Int32.Parse(name.Substring(5, 2));
        lonIdx = System.Int32.Parse(name[8].ToString());

        return (latDeg, latIdx, lonDeg, lonIdx);
    }

    static Regex regexNumbersPattern = new Regex(@"([0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static (int, int, int, int) SplitName(string name)
    {
        // Example name: '39_2_32_5'
        int latDeg, latIdx, lonDeg, lonIdx;

        var numbers = regexNumbersPattern.Matches(name);

        // Parse string and convert to int
        latDeg = Int32.Parse(numbers[0].Value);
        latIdx = Int32.Parse(numbers[1].Value);
        lonDeg = Int32.Parse(numbers[2].Value);
        lonIdx = Int32.Parse(numbers[3].Value);

        return (latDeg, latIdx, lonDeg, lonIdx);
    }

    public static (double, double) LatLonFromPositionBearingDistance(double lat, double lon, double bearing, double distance)
    {
        // Lat-Lon unit: degree, Bearing unit: rad, Distance unit: km

        // Declare new variables for shorter formulas
        double d = distance;
        double R = Constants.EarthRadiusInKm;

        // Convert lat-lon to radians
        double lat1 = lat * Mathf.Deg2Rad;
        double lon1 = lon * Mathf.Deg2Rad;

        double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d / R) + Math.Cos(lat1) * Math.Sin(d / R) * Math.Cos(bearing));
        double lon2 = lon1 + Math.Atan2(Math.Sin(bearing) * Math.Sin(d / R) * Math.Cos(lat1), Math.Cos(d / R) - Math.Sin(lat1) * Math.Sin(lat2));

        // Convert lat-lon back to degrees
        lat2 = lat2 * Mathf.Rad2Deg;
        lon2 = lon2 * Mathf.Rad2Deg;

        return (lat2, lon2);
    }

    public static Vector3 CalcBasePos(float latitude, float longitude, float aircraftAlt)
    {
        Vector3 basePos = CreatePositionByCoords(latitude, longitude, -aircraftAlt);
        return basePos;
    }



    /// <summary>
    /// Indicates whether any network connection is available.
    /// Filter connections below a specified speed, as well as virtual network cards.
    /// </summary>
    /// <param name="minimumSpeed">The minimum speed required. Passing 0 will not filter connection using speed.</param>
    /// <returns>
    ///     <c>true</c> if a network connection is available; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNetworkAvailable(long minimumSpeed)
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
            return false;

        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            // discard because of standard reasons
            if ((ni.OperationalStatus != OperationalStatus.Up) ||
                (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
                (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                continue;

            // this allow to filter modems, serial, etc.
            // I use 10000000 as a minimum speed for most cases
            if (ni.Speed < minimumSpeed)
                continue;

            // discard virtual cards (virtual box, virtual pc, etc.)
            if ((ni.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
                (ni.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0))
                continue;

            // discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
            if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                continue;

            return true;
        }
        return false;
    }


    public static T ReadJsonDataFromStreamingAssets<T>(string streamingAssetPath)
    {
        string plainData = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/" + streamingAssetPath);
        return JsonUtility.FromJson<T>(plainData);
    }

}
