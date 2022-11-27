using System;
using System.Collections;
using System.Collections.Generic;

public static class Constants
{
    // This may change, maybe declare as 'static readonly' instead of const?
    public const int DivisorOfOneDeg = 6;
    public static float TileLenght = 1f / DivisorOfOneDeg;

    // Global zero to avoid magic numbers
    public const int Zero = 0;

    public const double Pi = Math.PI;
    public const double PiOverTwo = Pi / 2.0;
    public const double ThreePiOverTwo = (3.0 * Pi) / 2.0;
    public const double TwoPi = 2.0 * Pi;

    // For geometric calculations
    public const double EarthRadiusInM = 6378137;
    public const double EarthRadiusInKm = 6378.137;
    public const double Flattening = 0.0033528106647474805f;     // 1/298.257223563
    public const double E2 = 0.0066943799901413165f;             // Flattening * (2-Flattening)

    public const double FEET_TO_METER = 0.3048f;
    public const double METER_TO_FEET = 1f/0.3048f;
    public const double OFFSET_BY_TERRAIN = 2.45f;
}
