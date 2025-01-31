using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        public static string ToCzml(IEnumerable<CzmlBase> objectList)
        {
            return JsonSerializer.Serialize(
                objectList,
                SerializerContext.Default.IEnumerableCzmlBase);
        }

        /// <summary>
        /// Reads name and description fields from the CZML file's packet header, if present.
        /// </summary>
        /// <param name="czml">CZML JSON text</param>
        /// <param name="name">document name to read</param>
        /// <param name="description">document description to read</param>
        public static void ReadCzmlNameAndDescription(
            string czml,
            out string name,
            out string description)
        {
            var rootObject = JsonArray.Parse(czml);
            if (rootObject is JsonArray rootArray &&
                rootArray.Count > 0)
            {
                var headerObject = rootArray[0]?.Deserialize(
                    SerializerContext.Default.PacketHeader);

                if (headerObject != null &&
                    headerObject.Id == "document")
                {
                    name = headerObject.Name;
                    description = headerObject.Description ?? string.Empty;

                    return;
                }
            }

            name = string.Empty;
            description = string.Empty;
        }
    }
}
