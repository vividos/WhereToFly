using System;
using System.Diagnostics.CodeAnalysis;

namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// Takeoff direction as flags that can be combined. Used to mark takeoff locations with the
    /// actual takeoff directions possible.
    /// </summary>
    [Flags]
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1602:EnumerationItemsMustBeDocumented",
        Justification = "No need to document the obvious cardinal directions")]
    public enum TakeoffDirections
    {
        /// <summary>
        /// No takeoff direction, none known, or no takeoff location
        /// </summary>
        None = 0,
        First = N,
        Last = NNW,
        All = N | NNE | NE | ENE | E | ESE | SE | SSE | S | SSW | SW | WSW | W | WNW | NW | NNW,

        N = 1 << 0,
        NNE = 1 << 1,
        NE = 1 << 2,
        ENE = 1 << 3,
        E = 1 << 4,
        ESE = 1 << 5,
        SE = 1 << 6,
        SSE = 1 << 7,
        S = 1 << 8,
        SSW = 1 << 9,
        SW = 1 << 10,
        WSW = 1 << 11,
        W = 1 << 12,
        WNW = 1 << 13,
        NW = 1 << 14,
        NNW = 1 << 15,
    }
}
