using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PulseECS.Networking.ChangeTracking;

namespace PulseECS.Core
{
    /// <summary>
    /// Stores entities, components and global simulation state.
    /// </summary>
    public class World
    {
        private readonly Dictionary<EntityId, Dictionary<Type, object>> _entityComponents = new();
        private readonly HashSet<EntityId> _entities = new();
        private readonly ComponentChangeSet _pendingChanges = new();
        private int _nextEntityId = 1;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            IncludeFields = true
        };

        public ulong Tick { get; private set; }

        public IReadOnlyCollection<EntityId> Entities => _entities;

        public bool Contains(EntityId entityId) => _entities.Contains(entityId);

        public EntityId CreateEntity()
        {
            var id = new EntityId(_nextEntityId++);
            _entities.Add(id);
            _entityComponents[id] = new Dictionary<Type, object>();
            return id;
        }

        internal EntityId CreateEntityWithId(int id)
        {
            var entityId = new EntityId(id);
            if (_entities.Contains(entityId))
            {
                return entityId;
            }

            _entities.Add(entityId);
            _entityComponents[entityId] = new Dictionary<Type, object>();
            _nextEntityId = Math.Max(_nextEntityId, id + 1);
            return entityId;
        }

        public bool DestroyEntity(EntityId entityId)
        {
            if (!_entities.Remove(entityId))
            {
                return false;
            }

            if (_entityComponents.TryGetValue(entityId, out var components))
            {
                foreach (var component in components)
                {
                    TrackComponentChange(entityId, component.Key, component.Value, null, ComponentMetadata.GetFlags(component.Key));
                }

                _entityComponents.Remove(entityId);
            }

            return true;
        }

        public void AddComponent<T>(EntityId entityId, T component) where T : IComponent
        {
            var type = typeof(T);
            var flags = ComponentMetadata.GetFlags(type);
            var map = GetOrCreateComponentMap(entityId);
            if (map.ContainsKey(type))
            {
                throw new InvalidOperationException($"Entity {entityId} already contains component {type}.");
            }

            map[type] = component!;
            TrackComponentChange(entityId, type, null, component, flags);
        }

        public void SetComponent<T>(EntityId entityId, T component) where T : IComponent
        {
            var type = typeof(T);
            var flags = ComponentMetadata.GetFlags(type);
            var map = GetOrCreateComponentMap(entityId);
            map.TryGetValue(type, out var previous);
            map[type] = component!;
            TrackComponentChange(entityId, type, previous, component, flags);
        }

        public void SetComponent(EntityId entityId, IComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            var type = component.GetType();
            var flags = ComponentMetadata.GetFlags(type);
            var map = GetOrCreateComponentMap(entityId);
            map.TryGetValue(type, out var previous);
            map[type] = component;
            TrackComponentChange(entityId, type, previous, component, flags);
        }

        public bool RemoveComponent<T>(EntityId entityId) where T : IComponent
        {
            var type = typeof(T);
            if (!TryGetComponent(entityId, type, out var existing))
            {
                return false;
            }

            var map = GetOrCreateComponentMap(entityId);
            map.Remove(type);
            TrackComponentChange(entityId, type, existing, null, ComponentMetadata.GetFlags(type));
            return true;
        }

        public bool RemoveComponent(EntityId entityId, Type componentType)
        {
            if (!TryGetComponent(entityId, componentType, out var existing))
            {
                return false;
            }

            var map = GetOrCreateComponentMap(entityId);
            map.Remove(componentType);
            TrackComponentChange(entityId, componentType, existing, null, ComponentMetadata.GetFlags(componentType));
            return true;
        }

        public bool TryGetComponent<T>(EntityId entityId, out T component) where T : IComponent
        {
            if (TryGetComponent(entityId, typeof(T), out var boxed) && boxed is T typed)
            {
                component = typed;
                return true;
            }

            component = default!;
            return false;
        }

        public bool HasComponent<T>(EntityId entityId) where T : IComponent
        {
            return TryGetComponent(entityId, typeof(T), out _);
        }

        public T RequireComponent<T>(EntityId entityId) where T : IComponent
        {
            if (!TryGetComponent<T>(entityId, out var component))
            {
                throw new InvalidOperationException($"Entity {entityId} missing component {typeof(T)}.");
            }

            return component;
        }

        public IEnumerable<(EntityId entityId, T component)> View<T>() where T : IComponent
        {
            var targetType = typeof(T);
            foreach (var entityId in _entities)
            {
                if (TryGetComponent(entityId, targetType, out var boxed) && boxed is T component)
                {
                    yield return (entityId, component);
                }
            }
        }

        public ComponentChangeSet ConsumeChanges()
        {
            var copy = new ComponentChangeSet();
            foreach (var change in _pendingChanges.Changes)
            {
                copy.Add(change);
            }

            _pendingChanges.Clear();
            return copy;
        }

        public void AdvanceTick()
        {
            Tick++;
        }

        internal void SetTick(ulong tick)
        {
            Tick = tick;
        }

        internal IReadOnlyDictionary<Type, object> GetComponents(EntityId entityId)
        {
            if (!_entityComponents.TryGetValue(entityId, out var map))
            {
                throw new InvalidOperationException($"Unknown entity {entityId}.");
            }

            return map;
        }

        internal IEnumerable<KeyValuePair<Type, object>> EnumerateComponents(EntityId entityId)
        {
            return GetComponents(entityId);
        }

        private Dictionary<Type, object> GetOrCreateComponentMap(EntityId entityId)
        {
            if (!_entities.Contains(entityId))
            {
                throw new InvalidOperationException($"Unknown entity {entityId}.");
            }

            if (!_entityComponents.TryGetValue(entityId, out var map))
            {
                map = new Dictionary<Type, object>();
                _entityComponents[entityId] = map;
            }

            return map;
        }

        private bool TryGetComponent(EntityId entityId, Type componentType, out object? component)
        {
            component = null;
            if (!_entities.Contains(entityId))
            {
                return false;
            }

            if (_entityComponents.TryGetValue(entityId, out var map) && map.TryGetValue(componentType, out var boxed))
            {
                component = boxed;
                return true;
            }

            return false;
        }

        private void TrackComponentChange(EntityId entityId, Type componentType, object? previous, object? current, ComponentFlags flags)
        {
            if ((flags & (ComponentFlags.Replicated | ComponentFlags.TrackChanges | ComponentFlags.Persisted)) == 0)
            {
                return;
            }

            JsonElement? previousElement = previous != null ? JsonSerializer.SerializeToElement(previous, componentType, SerializerOptions) : null;
            JsonElement? currentElement = current != null ? JsonSerializer.SerializeToElement(current, componentType, SerializerOptions) : null;
            _pendingChanges.Add(new ComponentChange(entityId, componentType, previousElement, currentElement));
        }

        public IEnumerable<EntityId> Query(params Type[] componentTypes)
        {
            foreach (var entity in _entities)
            {
                if (componentTypes.All(type => TryGetComponent(entity, type, out _)))
                {
                    yield return entity;
                }
            }
        }
    }
}
