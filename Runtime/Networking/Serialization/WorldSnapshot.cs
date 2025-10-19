using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PulseECS.Core;

namespace PulseECS.Networking.Serialization
{
    /// <summary>
    /// Serializable representation of the world state at a specific tick.
    /// </summary>
    public sealed class WorldSnapshot
    {
        public sealed class EntityState
        {
            public int EntityId { get; set; }
            public List<ComponentState> Components { get; set; } = new();
        }

        public sealed class ComponentState
        {
            public string Type { get; set; } = string.Empty;
            public JsonElement Data { get; set; }
        }

        public ulong Tick { get; set; }
        public List<EntityState> Entities { get; set; } = new();

        public static WorldSnapshot Capture(World world)
        {
            var snapshot = new WorldSnapshot
            {
                Tick = world.Tick
            };

            foreach (var entityId in world.Entities)
            {
                var entityState = new EntityState
                {
                    EntityId = entityId.Value
                };

                foreach (var component in world.GetComponents(entityId))
                {
                    var flags = ComponentMetadata.GetFlags(component.Key);
                    if ((flags & ComponentFlags.Persisted) == 0)
                    {
                        continue;
                    }

                    var element = JsonSerializer.SerializeToElement(component.Value, component.Key, JsonWorldSerializer.SerializerOptions);
                    entityState.Components.Add(new ComponentState
                    {
                        Type = component.Key.AssemblyQualifiedName ?? component.Key.FullName ?? component.Key.Name,
                        Data = element
                    });
                }

                if (entityState.Components.Count > 0)
                {
                    snapshot.Entities.Add(entityState);
                }
            }

            return snapshot;
        }

        public void Apply(World world)
        {
            var snapshotEntityIds = new HashSet<int>(Entities.Select(e => e.EntityId));
            foreach (var entity in world.Entities.ToList())
            {
                if (!snapshotEntityIds.Contains(entity.Value))
                {
                    world.DestroyEntity(entity);
                }
            }

            foreach (var entityState in Entities)
            {
                var entityId = world.Contains(new EntityId(entityState.EntityId))
                    ? new EntityId(entityState.EntityId)
                    : world.CreateEntityWithId(entityState.EntityId);

                var existingPersisted = world.EnumerateComponents(entityId)
                    .Where(pair => (ComponentMetadata.GetFlags(pair.Key) & ComponentFlags.Persisted) != 0)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                var snapshotComponentTypes = new HashSet<Type>();
                foreach (var componentState in entityState.Components)
                {
                    var type = Type.GetType(componentState.Type, throwOnError: true)!;
                    var component = (IComponent)JsonSerializer.Deserialize(componentState.Data, type, JsonWorldSerializer.SerializerOptions)!;
                    snapshotComponentTypes.Add(type);
                    world.SetComponent(entityId, component);
                }

                foreach (var kvp in existingPersisted)
                {
                    if (!snapshotComponentTypes.Contains(kvp.Key))
                    {
                        world.RemoveComponent(entityId, kvp.Key);
                    }
                }
            }

            world.SetTick(Tick);
        }
    }
}
