using System;
using System.Collections.Concurrent;
using System.Reflection;
using PulseECS.Networking.Attributes;

namespace PulseECS.Core
{
    internal static class ComponentMetadata
    {
        private static readonly ConcurrentDictionary<Type, ComponentFlags> Cache = new();

        public static ComponentFlags GetFlags(Type componentType)
        {
            return Cache.GetOrAdd(componentType, type =>
            {
                var flags = ComponentFlags.None;
                if (type.GetCustomAttribute<ReplicatedAttribute>() != null)
                {
                    flags |= ComponentFlags.Replicated;
                }

                if (type.GetCustomAttribute<PredictedAttribute>() != null)
                {
                    flags |= ComponentFlags.Predicted;
                }

                if (type.GetCustomAttribute<PersistedAttribute>() != null)
                {
                    flags |= ComponentFlags.Persisted;
                }

                if (type.GetCustomAttribute<TrackChangesAttribute>() != null)
                {
                    flags |= ComponentFlags.TrackChanges;
                }

                return flags;
            });
        }
    }

    [Flags]
    internal enum ComponentFlags
    {
        None = 0,
        Replicated = 1 << 0,
        Predicted = 1 << 1,
        Persisted = 1 << 2,
        TrackChanges = 1 << 3
    }
}
