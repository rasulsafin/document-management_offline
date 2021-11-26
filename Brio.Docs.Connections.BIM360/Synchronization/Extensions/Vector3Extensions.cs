using Brio.Docs.Common;

namespace Brio.Docs.Connections.Bim360.Synchronization.Extensions
{
    public static class Vector3Extensions
    {
        private const double FOOT = 0.3048;

        public static (double x, double y, double z) ToTuple(this Vector3d vector3)
            => (vector3.X, vector3.Y, vector3.Z);

        public static Vector3d ToVector(this (double x, double y, double z) tuple)
            => new Vector3d(tuple.x, tuple.y, tuple.z);

        public static Vector3d GetUpwardVector(this Vector3d vector)
        {
            const double EPSILON = 1e-15;

            var forward = vector.Normalized;
            var worldUp = Vector3d.UnitZ;
            var dot = Vector3d.Dot(forward, worldUp);

            if (dot > 0 && (1 - dot) < EPSILON)
                return -Vector3d.UnitY;
            else if (dot < 0 && (1 + dot) < EPSILON)
                return Vector3d.UnitY;

            var left = Vector3d.Cross(worldUp, forward).Normalized;
            return Vector3d.Cross(forward, left).Normalized;
        }

        public static Vector3d ToXZY(this Vector3d vector)
            => new Vector3d(vector.X, vector.Z, vector.Y);

        public static Vector3d ToMeters(this Vector3d vector3)
            => vector3 * FOOT;

        public static Vector3d ToFeet(this Vector3d vector3)
            => vector3 / FOOT;
    }
}
