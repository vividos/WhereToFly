using System;
using System.Collections.Generic;

/// <summary>
/// (c) 2011-2015, Vladimir Agafonkin
/// (c) 2020-2022 Michael Fink
/// SunCalc.Net is a C# library for calculating sun/moon position and light phases.
/// The library was ported from the suncalc JavaScript library:
/// https://github.com/mourner/suncalc
/// The code is based on version 1.9.0.
/// Sun calculations are based on https://aa.quae.nl/en/reken/zonpositie.html formulas
/// Note that moon calculations weren't ported over (yet).
/// </summary>
namespace WhereToFly.Geo.SunCalcNet
{
#pragma warning disable SA1312 // Variable names should begin with lower-case letter

    /// <summary>
    /// Sun calculations
    /// </summary>
    public static partial class SunCalc
    {
        /// <summary>
        /// Calculates sun times for a given date, latitude/longitude, and, optionally,
        /// the observer height (in meters) relative to the horizon.
        /// </summary>
        /// <param name="date">date to calculate solar times for</param>
        /// <param name="latitude">latitude value, going from greenwich meridian to the east</param>
        /// <param name="longitude">longitude value, going to the north and the south</param>
        /// <param name="height">height relative to the horizon, in meters</param>
        /// <returns>solar times object</returns>
        public static SolarTimes GetTimes(DateTimeOffset date, double latitude, double longitude, double height = 0.0)
        {
            var offset = date.Offset;
            double lw = -longitude.ToRadians();
            double phi = latitude.ToRadians();

            double dh = Formulas.ObserverAngle(height);

            double d = Formulas.ToDays(date);
            double n = Formulas.JulianCycle(d, lw);
            double ds = Formulas.ApproxTransit(0, lw, n);

            double M = Formulas.SolarMeanAnomaly(ds);
            double L = Formulas.EclipticLongitude(M);
            double dec = Formulas.Declination(L, 0);

            double Jnoon = Formulas.SolarTransitJ(ds, M, L);

            var sunriseSunsetTimes = new Dictionary<SunTimeType, DateTimeOffset>();

            var result = new SolarTimes
            {
                SolarNoon = Formulas.FromJulian(Jnoon - 0.5, offset),
                Nadir = Formulas.FromJulian(Jnoon - 0.5, offset),
                SunriseSunsetTimes = sunriseSunsetTimes,
            };

            var times = new (double Time, SunTimeType RiseType, SunTimeType SetType)[]
            {
                (-0.833, SunTimeType.Sunrise, SunTimeType.Sunset),
                (-0.3, SunTimeType.SunriseEnd, SunTimeType.SunsetStart),
                (-6, SunTimeType.Dawn, SunTimeType.Dusk),
                (-12, SunTimeType.NauticalDawn, SunTimeType.NauticalDusk),
                (-18, SunTimeType.NightEnd, SunTimeType.Night),
                (6, SunTimeType.GoldenHourEnd, SunTimeType.GoldenHour),
            };

            foreach (var item in times)
            {
                double time = item.Time;
                double h0 = (time + dh).ToRadians();

                double Jset = Formulas.GetSetJ(h0, lw, phi, dec, n, M, L);
                double Jrise = Jnoon - (Jset - Jnoon);

                var rise = Formulas.FromJulian(Jrise, offset);
                var set = Formulas.FromJulian(Jset, offset);
                if (rise.HasValue)
                {
                    sunriseSunsetTimes.Add(item.RiseType, rise.Value);
                }

                if (set.HasValue)
                {
                    sunriseSunsetTimes.Add(item.SetType, set.Value);
                }

                if (item.RiseType == SunTimeType.Sunrise)
                {
                    result.Sunrise = rise;
                }

                if (item.SetType == SunTimeType.Sunset)
                {
                    result.Sunset = set;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates sun position for a given date and latitude/longitude
        /// </summary>
        /// <param name="date">date to calculate position for</param>
        /// <param name="latitude">latitude value, going from greenwich meridian to the east</param>
        /// <param name="longitude">longitude value, going to the north and the south</param>
        /// <returns>sun position</returns>
        public static SunPosition GetPosition(DateTimeOffset date, double latitude, double longitude)
        {
            double lw = -longitude.ToRadians();
            double phi = latitude.ToRadians();

            double d = Formulas.ToDays(date);

            EquatorialCoordinates coord = Formulas.SunCoords(d);
            double H = Formulas.SiderealTime(d, lw) - coord.RightAscension;

            return new SunPosition
            {
                Azimuth = Formulas.Azimuth(H, phi, coord.Declination),
                Altitude = Formulas.Altitude(H, phi, coord.Declination),
            };
        }
    }
}
