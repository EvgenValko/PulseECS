using System;
using System.Collections.Generic;
using System.Text.Json;
using PulseECS.Core;

namespace PulseECS.Networking.ChangeTracking
{
    /// <summary>
    /// Represents a change to a single component.
    /// </summary>
    public sealed class ComponentChange
    {
        public EntityId EntityId { get; }
        public Type ComponentType { get; }
        public JsonElement? PreviousState { get; }
        public JsonElement? CurrentState { get; }

        public ComponentChange(EntityId entityId, Type componentType, JsonElement? previousState, JsonElement? currentState)
        {
            EntityId = entityId;
            ComponentType = componentType;
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    /// <summary>
    /// Collects component changes since the last snapshot.
    /// </summary>
    public sealed class ComponentChangeSet
    {
        private readonly List<ComponentChange> _changes = new();

        public IReadOnlyList<ComponentChange> Changes => _changes;

        public bool HasChanges => _changes.Count > 0;

        public void Clear() => _changes.Clear();

        public void Add(ComponentChange change) => _changes.Add(change);
    }
}
