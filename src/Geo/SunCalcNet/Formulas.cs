using System;

namespace WhereToFly.Geo.SunCalcNet
{
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

    /// <summary>
    /// Static class containing all the formulas used, in case it is needed for other
    /// calculations.
    /// </summary>
    public static class Formulas
    {
        #region Date/time constants and conversions
        /// <summary>
        /// Date 2000-01-01 at noon UTC, as julian day value
        /// </summary>
        public const double J2000 = 2451545.0;

        /// <summary>
        /// Unix epoch as julian day value
        /// </summary>
        public const double J1970 = 2440588.0;

        /// <summary>
        /// Year 0 as julian day value
        /// </summary>
        public const double J0 = 0.0009;

        /// <summary>
        /// Milliseconds in a day
        /// </summary>
        public const double DayMs = 1000 * 60 * 60 * 24;

        /// <summary>
        /// Converts a DateTimeOffset object to julian day value.
        /// </summary>
        /// <param name="date">date to convert</param>
        /// <returns>julian day value</returns>
        public static double ToJulian(DateTimeOffset date)
        {
            return (date.ToUnixTimeMilliseconds() / DayMs) - 0.5 + J1970;
        }

        /// <summary>
        /// Converts a DateTimeOffset object to days since the start of year 2000.
        /// </summary>
        /// <param name="date">date to convert</param>
        /// <returns>number of days</returns>
        public static double ToDays(DateTimeOffset date)
        {
            return ToJulian(date) - J2000;
        }

        /// <summary>
        /// Calculates a DateTimeOffset value from given julian day. NaN values are converted
        /// to a null DateTimeOffset object.
        /// </summary>
        /// <param name="j">julian day</param>
        /// <param name="offset">date/time span offset</param>
        /// <returns>date time object, or null when not a date</returns>
        public static DateTimeOffset? FromJulian(double j, TimeSpan offset)
        {
            if (double.IsNaN(j))
            {
                return null;
            }

            double millisecondsSinceEpoch = (j + 0.5 - J1970) * DayMs;

            return DateTimeOffset
                .FromUnixTimeMilliseconds((long)millisecondsSinceEpoch)
                .ToOffset(offset);
        }

        /// <summary>
        /// Calculates the julian cycle value, based on julian day and the longitude.
        /// </summary>
        /// <param name="d">days since J2000 value</param>
        /// <param name="lw">longitude of position, in radians</param>
        /// <returns>julian cycle value</returns>
        public static double JulianCycle(double d, double lw)
        {
            return Math.Round(d - J0 - (lw / (2 * Math.PI)));
        }
        #endregion

        #region general calculations for position
        /// <summary>
        /// Obliquity of the Earth; see https://en.wikipedia.org/wiki/Ecliptic#Obliquity
        /// </summary>
        public const double EarthObliquity = 23.4397 * (Math.PI / 180);

        /// <summary>
        /// Calculates the right ascension for the given ecliptic longitude.
        /// </summary>
        /// <param name="l">ecliptic longitude, in radians</param>
        /// <param name="b">days since january 1st</param>
        /// <returns>right ascension in radians</returns>
        public static double RightAscension(double l, int b)
        {
            return Math.Atan2(
                (Math.Sin(l) * Math.Cos(EarthObliquity)) -
                (Math.Tan(b) * Math.Sin(EarthObliquity)),
                Math.Cos(l));
        }

        /// <summary>
        /// Calculates the declination for the given ecliptic longitude.
        /// </summary>
        /// <param name="l">ecliptic longitude, in radians</param>
        /// <param name="b">days since january 1st</param>
        /// <returns>declination in radians</returns>
        public static double Declination(double l, int b)
        {
            return Math.Asin(
                (Math.Sin(b) * Math.Cos(EarthObliquity)) +
                (Math.Cos(b) * Math.Sin(EarthObliquity) * Math.Sin(l)));
        }

        /// <summary>
        /// Calculates azimuth angle of a celestial object in the sky
        /// </summary>
        /// <param name="H">hour angle, in radians</param>
        /// <param name="phi">north latitude of the observer, in radians</param>
        /// <param name="dec">declination of the sky object, in radians</param>
        /// <returns>azimuth angle, in radians</returns>
        public static double Azimuth(double H, double phi, double dec)
        {
            return Math.Atan2(
                Math.Sin(H),
                (Math.Cos(H) * Math.Sin(phi)) - (Math.Tan(dec) * Math.Cos(phi)))
                + Math.PI; // measure azimuth from south
        }

        /// <summary>
        /// Calculates altitude angle of a celestial object in the sky
        /// </summary>
        /// <param name="H">hour angle, in radians</param>
        /// <param name="phi">north latitude of the observer, in radians</param>
        /// <param name="dec">declination of the sky object, in radians</param>
        /// <returns>altitude angle, in radians</returns>
        public static double Altitude(double H, double phi, double dec)
        {
            return Math.Asin(
                (Math.Sin(phi) * Math.Sin(dec)) +
                (Math.Cos(phi) * Math.Cos(dec) * Math.Cos(H)));
        }

        /// <summary>
        /// Calculates sidereal time for the earth
        /// </summary>
        /// <param name="d">days since J2000 value</param>
        /// <param name="lw">longitude of position, in radians</param>
        /// <returns>sidereal time, in radians</returns>
        public static double SiderealTime(double d, double lw)
        {
            return (280.16 + (360.9856235 * d)).ToRadians() - lw;
        }
        #endregion

        #region general sun calculations
        /// <summary>
        /// Calculates the observer angle from the height relative to the horizon, in meters.
        /// </summary>
        /// <param name="height">height in meters</param>
        /// <returns>observer angle, in degrees</returns>
        public static double ObserverAngle(double height)
        {
            return -2.076 * Math.Sqrt(height) / 60;
        }

        /// <summary>
        /// Calculates the sun's mean anomaly.
        /// See https://en.wikipedia.org/wiki/Mean_anomaly
        /// </summary>
        /// <param name="ds">julian cycle value</param>
        /// <returns>solar mean anomaly, in radians</returns>
        public static double SolarMeanAnomaly(double ds)
        {
            return (357.5291 + (0.98560028 * ds)).ToRadians();
        }

        /// <summary>
        /// Calculates ecliptic longitude from solar mean anomaly.
        /// </summary>
        /// <param name="M">solar mean anomaly, in radians</param>
        /// <returns>ecliptic longitude, in radians</returns>
        public static double EclipticLongitude(double M)
        {
            // sun's equation of the center
            // https://en.wikipedia.org/wiki/Equation_of_the_center
            double C =
                ((1.9148 * Math.Sin(M)) +
                (0.02 * Math.Sin(2 * M)) +
                (0.0003 * Math.Sin(3 * M))).ToRadians();

            // perihelion of the Earth
            double P = 102.9372.ToRadians();

            return M + C + P + Math.PI;
        }

        /// <summary>
        /// Calculates sun coordinates for a given julian date
        /// </summary>
        /// <param name="d">days since J2000 value</param>
        /// <returns>sun's equatorial coordinates</returns>
        public static EquatorialCoordinates SunCoords(double d)
        {
            double M = SolarMeanAnomaly(d);
            double L = EclipticLongitude(M);

            return new EquatorialCoordinates
            {
                Declination = Declination(L, 0),
                RightAscension = RightAscension(L, 0),
            };
        }

        /// <summary>
        /// Calculates the hour angle of the sun.
        /// See https://en.wikipedia.org/wiki/Hour_angle#Solar_hour_angle
        /// or https://en.wikipedia.org/wiki/Sunrise_equation#Hour_angle
        /// </summary>
        /// <param name="h">observer angle, in radians</param>
        /// <param name="phi">north latitude of the observer, in radians</param>
        /// <param name="dec">declination of the sun</param>
        /// <returns>hour angle, in radians</returns>
        private static double HourAngle(double h, double phi, double dec)
        {
            return Math.Acos(
                (Math.Sin(h) - (Math.Sin(phi) * Math.Sin(dec))) / (Math.Cos(phi) * Math.Cos(dec)));
        }

        /// <summary>
        /// Calculates transit time of sun at given hour angle and longitude
        /// </summary>
        /// <param name="Ht">hour angle, in radians</param>
        /// <param name="lw">longitude of position, in radians</param>
        /// <param name="n">julian cycle number</param>
        /// <returns>julian cycle value</returns>
        public static double ApproxTransit(double Ht, double lw, double n)
        {
            return J0 + ((Ht + lw) / (2 * Math.PI)) + n;
        }

        /// <summary>
        /// Calculates solar transit as julian day value
        /// </summary>
        /// <param name="ds">julian cycle value</param>
        /// <param name="M">solar mean anomaly, in radians</param>
        /// <param name="L">ecliptic longitude, in radians</param>
        /// <returns>julian day value</returns>
        public static double SolarTransitJ(double ds, double M, double L)
        {
            return J2000 + ds + (0.0053 * Math.Sin(M)) - (0.0069 * Math.Sin(2 * L));
        }

        /// <summary>
        /// Returns set time for the given sun altitude
        /// </summary>
        /// <param name="h">observer angle, in radians</param>
        /// <param name="lw">longitude of position, in radians</param>
        /// <param name="phi">north latitude of the observer, in radians</param>
        /// <param name="dec">declination of the sun</param>
        /// <param name="n">julian cycle number</param>
        /// <param name="M">solar mean anomaly, in radians</param>
        /// <param name="L">ecliptic longitude, in radians</param>
        /// <returns>set time, as julian day value</returns>
        public static double GetSetJ(
            double h,
            double lw,
            double phi,
            double dec,
            double n,
            double M,
            double L)
        {
            double w = HourAngle(h, phi, dec);
            double a = ApproxTransit(w, lw, n);
            return SolarTransitJ(a, M, L);
        }
        #endregion
    }
}
