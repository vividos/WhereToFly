namespace WhereToFly.Shared.Base
{
    /// <summary>
    /// Math functions
    /// </summary>
    public static class Math
    {
        /// <summary>
        /// Interpolates between two values for a given value delta.
        /// </summary>
        /// <param name="value1">value 1</param>
        /// <param name="value2">value 2</param>
        /// <param name="delta">delta value, usually from 0.0 to 1.0</param>
        /// <returns>interpolated value between p1 and p2</returns>
        public static double Interpolate(double value1, double value2, double delta)
        {
            return value1 + ((value2 - value1) * delta);
        }
    }
}
