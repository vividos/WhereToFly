using Newtonsoft.Json;
using System.Collections.Generic;

namespace WhereToFly.Geo.DataFormats.Czml
{
    /// <summary>
    /// Serializer for CZML format
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Converts a list of CZML objects (starting with <see cref="PacketHeader"/>, followed
        /// by one or more <see cref="Object"/> instances, to CZML.
        /// </summary>
        /// <param name="objectList">object list to convert</param>
        /// <returns>JSON formatted CZML</returns>
        public static string ToCzml(IEnumerable<object> objectList)
        {
            return JsonConvert.SerializeObject(objectList);
        }
    }
}
