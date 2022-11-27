using System;

public class Indexer
{
    // VARIABLES

    
    // METHODS
    public static string ObjNameFromCoords(float lat, float lon)
    {
        int retLat, latIdx, retLon, lonIdx;
        (retLat, latIdx, retLon, lonIdx) = ObjIdxFromCoords(lat, lon);
        return $"{retLat}_{latIdx}_{retLon}_{lonIdx}";
    }

    public static (int, int, int, int) ObjIdxFromCoords(float lat, float lon)
    {
        int retLat = (int)lat;
        int retLon = (int)lon;

        double tmpLat, tmpLon;

        tmpLat = Math.Abs(lat - retLat);
        tmpLon = Math.Abs(lon - retLon);
        int retLatIdx = (int)(tmpLat * Constants.DivisorOfOneDeg) + 1;
        int retLonIdx = (int)(tmpLon * Constants.DivisorOfOneDeg) + 1;

        return (retLat, retLatIdx, retLon, retLonIdx);
    }

    public static (float,float) CoordsFromObjIdx(int lat , int lat_idx, int lon, int lon_idx)
    {
        return (lat+ ((float)(lat_idx-1)/Constants.DivisorOfOneDeg), lon + ((float)(lon_idx-1) / Constants.DivisorOfOneDeg));
    }
    public static (float, float) CenterCoordsFromCornerCoords(float lat,float lon)
    {
        return (lat + (Constants.TileLenght / 2), lon + (Constants.TileLenght / 2));
    }
    public static (float, float) CenterCoordsFromCornerCoords((float, float) coordPair)
    {
        return CenterCoordsFromCornerCoords(coordPair.Item1, coordPair.Item2);
    }
    public static (float, float) CenterCoordsFromObjIdx(int lat, int lat_idx, int lon, int lon_idx)
    {
        return CenterCoordsFromCornerCoords(CoordsFromObjIdx(lat, lat_idx, lon, lon_idx));
    }


    public static bool AirplaneInBounds(float lat, float lon)
    {
        bool inBounds = true;
        if (lat > 41.5f || lat < 36.5f || lon > 44.5f || lon < 26.5f)
        {
            inBounds = false;
        }
        return inBounds;
    }

    public static MapSection GetSouthwestBoundary(float lat, float lon, float vision)
    {
        double latMin, lonMin;
        (latMin, _) = Utility.LatLonFromPositionBearingDistance(lat, lon, Constants.Pi, vision);
        (_, lonMin) = Utility.LatLonFromPositionBearingDistance(lat, lon, Constants.ThreePiOverTwo, vision);

        return BoundaryCalculator(latMin, lonMin);
    }

    public static MapSection GetNortheastBoundary(float lat, float lon, float vision)
    {
        double latMax, lonMax;

        (latMax, _) = Utility.LatLonFromPositionBearingDistance(lat, lon, Constants.Zero, vision);
        (_, lonMax) = Utility.LatLonFromPositionBearingDistance(lat, lon, Constants.PiOverTwo, vision);

        return BoundaryCalculator(latMax, lonMax);
    }

    private static MapSection BoundaryCalculator(double lat, double lon)
    {
        int latName, latPartName, lonName, lonPartName;
        double tmpLat, tmpLon;

        latName = (int) lat;
        lonName = (int) lon;
        tmpLat = Math.Abs(lat - latName);
        tmpLon = Math.Abs(lon - lonName);
        latPartName = (int) (tmpLat * Constants.DivisorOfOneDeg) + 1;
        lonPartName = (int) (tmpLon * Constants.DivisorOfOneDeg) + 1;

        return new MapSection(latName, latPartName, lonName, lonPartName);
    }
}
