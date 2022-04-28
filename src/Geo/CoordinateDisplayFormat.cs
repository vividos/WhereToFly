namespace WhereToFly.Geo
{
    /// <summary>
    /// Display format for longitude/latitude coordinates
    /// </summary>
    public enum CoordinateDisplayFormat
    {
        /// <summary>
        /// Displays the coordinates in the format dd.ddddd°
        /// </summary>
        Format_dd_dddddd = 0,

        /// <summary>
        /// Displays the coordinates in the format dd° mm.mmm'
        /// </summary>
        Format_dd_mm_mmm = 1,

        /// <summary>
        /// Displays the coordinates in the format dd° mm' sss"
        /// </summary>
        Format_dd_mm_sss = 2,
    }
}
