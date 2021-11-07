namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// Type of location
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// A general waypoint
        /// </summary>
        Waypoint = 0,

        /// <summary>
        /// Location is a summit
        /// </summary>
        Summit = 1,

        /// <summary>
        /// A mountain pass, saddle or col
        /// </summary>
        Pass = 2,

        /// <summary>
        /// A lake access point
        /// </summary>
        Lake = 3,

        /// <summary>
        /// A bridge
        /// </summary>
        Bridge = 4,

        /// <summary>
        /// A viewpoint or scenic view
        /// </summary>
        Viewpoint = 5,

        /// <summary>
        /// Alpine hut
        /// </summary>
        AlpineHut = 6,

        /// <summary>
        /// A restaurant
        /// </summary>
        Restaurant = 7,

        /// <summary>
        /// Church, chapel or small shrine
        /// </summary>
        Church = 8,

        /// <summary>
        /// Castle or ruin
        /// </summary>
        Castle = 9,

        /// <summary>
        /// Cave or grotto
        /// </summary>
        Cave = 10,

        /// <summary>
        /// Information spot
        /// </summary>
        Information = 11,

        /// <summary>
        /// A spot for swimming, e.g. a beach or a river spot
        /// </summary>
        SwimmingSpot = 12,

        /// <summary>
        /// Public transport station for bus
        /// </summary>
        PublicTransportBus = 13,

        /// <summary>
        /// Public transport station for train
        /// </summary>
        PublicTransportTrain = 14,

        /// <summary>
        /// Parking area, usually for cars
        /// </summary>
        Parking = 15,

        /// <summary>
        /// Webcam location
        /// </summary>
        Webcam = 16,

        /// <summary>
        /// Start or end point of a via ferrata, a climbing trail
        /// </summary>
        ViaFerrata = 100,

        /// <summary>
        /// Cable car station, either on top of the mountain, or in the valley
        /// </summary>
        CableCar = 101,

        /// <summary>
        /// Flying, Hanggliding or Paragliding takeoff location
        /// </summary>
        FlyingTakeoff = 150,

        /// <summary>
        /// Flying, Hanggliding or Paragliding landing place location
        /// </summary>
        FlyingLandingPlace = 151,

        /// <summary>
        /// Flying, Hanggliding or Paragliding winch towing location
        /// </summary>
        FlyingWinchTowing = 152,

        /// <summary>
        /// Turnpoint, e.g. for planned flying tracks
        /// </summary>
        Turnpoint = 153,

        /// <summary>
        /// Thermal point
        /// </summary>
        Thermal = 154,

        /// <summary>
        /// Meteorological station, e.g. wind and temperature
        /// </summary>
        MeteoStation = 155,

        /// <summary>
        /// Live waypoint that is getting updated over the internet
        /// </summary>
        LiveWaypoint = 200,

        /// <summary>
        /// An undefined location; should not be used
        /// </summary>
        Undefined = 999,
    }
}
