using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector2Utility
    {
        // https://stackoverflow.com/a/2259502/486974
        public static Vector2 RotateAroundPoint(in Vector2 axis, Vector2 point, float angle)
        {
            var sin = MathUtility.Sin(angle);
            var cos = MathUtility.Cos(angle);

            point -= axis;
            var rotated = new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y  * cos);
            return rotated + axis;
        }

        /// <summary>
        /// z-coordinate of the cross product (where vectors are interpreted as lying in xy plane)
        /// see also https://github.com/dotnet/corefx/issues/35434
        /// </summary>
        public static float Cross(in Vector2 value1, in Vector2 value2)
        {
            return value1.X * value2.Y - value1.Y * value2.X;
        }
    }
}
