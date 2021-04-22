using System.Diagnostics;

namespace WhereToFly.Geo.Airspace
{
    /// <summary>
    /// Altitude value; a combination of height and a reference specifier, e.g. AGL or AMSL
    /// </summary>
    public class Altitude
    {
        /// <summary>
        /// Altitude value, in feet; height reference is given in AltitudeType
        /// </summary>
        public double Value { get; private set; }

        /// <summary>
        /// Altitude type / height reference
        /// </summary>
        public AltitudeType Type { get; private set; }

        /// <summary>
        /// When the AltitudeType is Textual, this property has the text
        /// </summary>
        public string Textual { get; private set; }

        /// <summary>
        /// Opening times; may be null.
        /// </summary>
        public string OpeningTimes { get; set; }

        /// <summary>
        /// Creates a new altitude with a type that needs no value
        /// </summary>
        /// <param name="type">altitude type</param>
        public Altitude(AltitudeType type)
        {
            Debug.Assert(
                type == AltitudeType.GND || type == AltitudeType.Unlimited,
                "type must be GND or Unlimited");

            this.Type = type;
        }

        /// <summary>
        /// Creates a new altitude value with type
        /// </summary>
        /// <param name="valueInFeet">altitude value, in feet</param>
        /// <param name="type">altitude type</param>
        public Altitude(double valueInFeet, AltitudeType type)
        {
            this.Value = valueInFeet;
            this.Type = type;
        }

        /// <summary>
        /// Creates a new textual altitude value
        /// </summary>
        /// <param name="textual">textual value</param>
        public Altitude(string textual)
        {
            this.Type = AltitudeType.Textual;
            this.Textual = textual;
        }

        /// <summary>
        /// Formats a displayable text for the airspace
        /// </summary>
        /// <returns>airspace as text</returns>
        public override string ToString()
        {
            string altitudeValue = this.FormatValue();

            if (!string.IsNullOrEmpty(this.OpeningTimes))
            {
                altitudeValue += $" ({this.OpeningTimes})";
            }

            return altitudeValue;
        }

        /// <summary>
        /// Formats altitude value
        /// </summary>
        /// <returns>formatted atltitude text</returns>
        private string FormatValue()
        {
            switch (this.Type)
            {
                case AltitudeType.GND: return "GND";
                case AltitudeType.Unlimited: return "UNLIMITED";
                case AltitudeType.Textual: return this.Textual;
                case AltitudeType.AMSL: return $"{this.Value}ft AMSL";
                case AltitudeType.AGL: return $"{this.Value}ft AGL";
                case AltitudeType.FlightLevel:
                    return $"FL {this.Value}";
                default:
                    Debug.Assert(false, "invalid type");
                    return "???";
            }
        }
    }
}
