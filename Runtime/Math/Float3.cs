using System;
using System.Numerics;

namespace PulseECS.Math
{
    /// <summary>
    /// Simple float3 implementation that is compatible with both Unity and .NET.
    /// </summary>
    public readonly struct Float3 : IEquatable<Float3>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Float3 FromVector(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

        public static Float3 operator +(Float3 left, Float3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        public static Float3 operator -(Float3 left, Float3 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        public static Float3 operator *(Float3 value, float scalar) => new(value.X * scalar, value.Y * scalar, value.Z * scalar);

        public float MagnitudeSquared => X * X + Y * Y + Z * Z;

        public float Magnitude => (float)System.Math.Sqrt(MagnitudeSquared);

        public bool Equals(Float3 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

        public override bool Equals(object? obj) => obj is Float3 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public override string ToString() => $"({X:0.###}, {Y:0.###}, {Z:0.###})";
    }
}
