using System;
using System.Numerics;

namespace Resonant
{
    internal static class Maths
    {
        static internal float PI = (float)Math.PI;
        static internal float TAU = PI * 2f;

        static internal float Radians(float degrees)
        {
            return PI * degrees / 180.0f;
        }

        static internal bool BetweenAngles(float test, float start, float end)
        {
            // check if the angle between START and TEST is between 0 and the angle between START and END
            var toEnd = NormalizeRadians(end - start);
            var toTest = NormalizeRadians(test - start);

            return toTest > 0 && toTest < toEnd;
        }

        static private float NormalizeRadians(float radians)
        {
            return (radians + TAU) % TAU;
        }

        static internal float DistanceXZ(Vector3 a, Vector3 b)
        {
            var dx = b.X - a.X;
            var dz = b.Z - a.Z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        static internal float AngleXZ(Vector3 a, Vector3 b)
        {
            return (float)Math.Atan2(b.X - a.X, b.Z - a.Z);
        }

        internal static Vector2 MinVector2(Vector2 a, Vector2 minimum)
        {
            return new(
                Math.Max(a.X, minimum.X),
                Math.Max(a.Y, minimum.Y)
            );
        }
    }
}
