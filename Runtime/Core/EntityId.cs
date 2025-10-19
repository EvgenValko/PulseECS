using System;

namespace PulseECS.Core
{
    /// <summary>
    /// Strongly typed identifier for entities.
    /// </summary>
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public readonly int Value;

        public EntityId(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Entity identifiers must be positive.");
            }

            Value = value;
        }

        public bool Equals(EntityId other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is EntityId other && Equals(other);

        public override int GetHashCode() => Value;

        public override string ToString() => $"Entity({Value})";

        public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);

        public static bool operator !=(EntityId left, EntityId right) => !(left == right);
    }
}
