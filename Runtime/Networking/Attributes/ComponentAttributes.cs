using System;

namespace PulseECS.Networking.Attributes
{
    /// <summary>
    /// Marks a component as participating in network replication.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ReplicatedAttribute : Attribute
    {
    }

    /// <summary>
    /// Marks a component as being predicted on the client.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class PredictedAttribute : Attribute
    {
    }

    /// <summary>
    /// Marks a component as being persisted when exporting the world state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class PersistedAttribute : Attribute
    {
    }

    /// <summary>
    /// Marks a component whose changes should be tracked for incremental updates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class TrackChangesAttribute : Attribute
    {
    }
}
