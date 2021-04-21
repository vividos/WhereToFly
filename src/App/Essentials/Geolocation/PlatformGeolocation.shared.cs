using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WhereToFly.App.Essentials
{
    public static partial class Geolocation
    {
        /// <summary>
        /// GeoLocation: Minimum distance to travel to send the next update
        /// </summary>
        private const double GeoLocationMinimumDistanceForUpdateInMeters = 2;

        private static bool isListening;

        private static bool PlatformIsListening() => isListening;

        private static async Task<bool> PlatformStartListeningForegroundAsync(GeolocationRequest request)
        {
            if (IsListening)
            {
                throw new InvalidOperationException("This Geolocation is already listening");
            }

            CrossGeolocator.Current.PositionChanged += OnPositionChanged;

            isListening = true;

            return await CrossGeolocator.Current.StartListeningAsync(
                request.Timeout,
                GeoLocationMinimumDistanceForUpdateInMeters,
                includeHeading: true,
                listenerSettings: null);
        }

        /// <summary>
        /// Called when the position has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private static void OnPositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs args)
        {
            var location = new Location(args.Position.Latitude, args.Position.Longitude)
            {
                Altitude = args.Position.Altitude,
                Course = args.Position.Heading,
                Speed = args.Position.Speed,
                Timestamp = args.Position.Timestamp,
                AltitudeReferenceSystem = AltitudeReferenceSystem.Geoid,
                Accuracy = args.Position.Accuracy,
                VerticalAccuracy = args.Position.AltitudeAccuracy,
            };

            OnLocationChanged(location);
        }

        public static Task<bool> PlatformStopListeningForegroundAsync()
        {
            if (!isListening)
            {
                return Task.FromResult(true);
            }

            CrossGeolocator.Current.PositionChanged -= OnPositionChanged;

            return Task.FromResult(true);
        }
    }
}
