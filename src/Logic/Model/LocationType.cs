namespace WhereToFly.Logic.Model
{
    /// <summary>
    /// Type of location
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// Location is a summit
        /// </summary>
        Summit = 0,

        /// <summary>
        /// A mountain pass, saddle or col
        /// </summary>
        Pass = 1,

        /// <summary>
        /// A lake access point
        /// </summary>
        Lake = 2,

        /// <summary>
        /// A bridge
        /// </summary>
        Bridge = 3,

        /// <summary>
        /// A viewpoint or scenic view
        /// </summary>
        Viewpoint = 4,

        /// <summary>
        /// Alpine hut
        /// </summary>
        AlpineHut = 5,

        /// <summary>
        /// A restaurant
        /// </summary>
        Restaurant = 6,

        /// <summary>
        /// Church, chapel or small shrine
        /// </summary>
        Church = 7,

        /// <summary>
        /// Castle or ruin
        /// </summary>
        Castle = 8,

        /// <summary>
        /// Cave or grotto
        /// </summary>
        Cave = 9,

        /// <summary>
        /// Information spot
        /// </summary>
        Information = 10,

        /// <summary>
        /// A spot for swimming, e.g. a beach or a river spot
        /// </summary>
        SwimmingSpot = 11,

        /// <summary>
        /// Public transport station for bus
        /// </summary>
        PublicTransportBus = 12,

        /// <summary>
        /// Public transport station for train
        /// </summary>
        PublicTransportTrain = 13,

        /// <summary>
        /// Parking area, usually for cars
        /// </summary>
        Parking = 14,

        /// <summary>
        /// Start or end point of a via ferrata, a climbing trail
        /// </summary>
        ViaFerrata = 100,

        /// <summary>
        /// Hanggliding or Paragliding location
        /// </summary>
        Paragliding = 101,

        /// <summary>
        /// Cable car station, either on top of the mountain, or in the valley
        /// </summary>
        CableCar = 102,

        /// <summary>
        /// An undefined location; should not be used
        /// </summary>
        Undefined = 999,
    }
}
