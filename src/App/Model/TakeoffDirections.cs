using System;
using System.Diagnostics.CodeAnalysis;

namespace WhereToFly.App.Model
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

        N = 1 << 1,
        NNE = 1 << 2,
        NE = 1 << 3,
        ENE = 1 << 4,
        E = 1 << 5,
        ESE = 1 << 6,
        SE = 1 << 7,
        SSE = 1 << 8,
        S = 1 << 9,
        SSW = 1 << 10,
        SW = 1 << 11,
        WSW = 1 << 12,
        W = 1 << 13,
        WNW = 1 << 14,
        NW = 1 << 15,
        NNW = 1 << 16,
    }
}
