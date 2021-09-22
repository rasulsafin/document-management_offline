using System;
using System.Numerics;

namespace Brio.Docs.Connections.Bim360.Synchronization.Extensions
{
    public static class Vector3Extensions
    {
        private const float FOOT = 0.3048f;
        private static readonly float EPSILON = 5.4211e-20f;

        public static (float x, float y, float z) ToTuple(this Vector3 vector3)
            => (vector3.X, vector3.Y, vector3.Z);

        public static Vector3 ToVector(this (float x, float y, float z) tuple)
            => new Vector3(tuple.x, tuple.y, tuple.z);

        public static Vector3 GetUpwardVector(this Vector3 vector)
        {
            vector = Vector3.Normalize(vector);
            var worldUp = Vector3.UnitZ;
            var cos = Vector3.Dot(vector, worldUp) / vector.Length() / worldUp.Length();
            var angle = (float)Math.Acos(cos);
            if (angle < EPSILON)
                return -Vector3.UnitY;

            var up = ((float)(-1.0 / Math.Tan(angle)) * vector) + (worldUp / (float)Math.Sin(angle));
            return Vector3.Normalize(up);
        }

        public static Vector3 ToXZY(this Vector3 vector)
            => new Vector3(vector.X, vector.Z, vector.Y);

        public static Vector3 ToMeters(this Vector3 vector3)
            => vector3 * FOOT;

        public static Vector3 ToFeet(this Vector3 vector3)
            => vector3 / FOOT;
    }
}
