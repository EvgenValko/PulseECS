using System.Linq;
using PulseECS.Core;
using PulseECS.InterestManagement;
using PulseECS.Math;
using PulseECS.Networking.Attributes;
using PulseECS.Networking.Prediction;
using PulseECS.Networking.Serialization;
using Xunit;

namespace PulseECS.Tests
{
    [Replicated]
    [Persisted]
    [TrackChanges]
    public sealed record PositionComponent(float X, float Y) : IComponent;

    [Replicated]
    [Persisted]
    [TrackChanges]
    public sealed record VelocityComponent(float X, float Y) : IComponent;

    public class WorldTests
    {
        [Fact]
        public void ChangeSetIncludesReplicatedComponents()
        {
            var world = new World();
            var entity = world.CreateEntity();
            world.AddComponent(entity, new PositionComponent(0, 0));

            var firstChanges = world.ConsumeChanges();
            Assert.Single(firstChanges.Changes);

            world.SetComponent(entity, new PositionComponent(1, 0));
            var secondChanges = world.ConsumeChanges();
            Assert.Single(secondChanges.Changes);
            Assert.Equal(entity, secondChanges.Changes.First().EntityId);
        }

        [Fact]
        public void JsonSerializerRoundTripRestoresWorld()
        {
            var world = new World();
            var entity = world.CreateEntity();
            world.AddComponent(entity, new PositionComponent(4, 2));
            world.AdvanceTick();

            var json = JsonWorldSerializer.Save(world);

            var restored = new World();
            JsonWorldSerializer.Load(restored, json);

            var restoredEntity = new EntityId(entity.Value);
            Assert.True(restored.HasComponent<PositionComponent>(restoredEntity));
            var component = restored.RequireComponent<PositionComponent>(restoredEntity);
            Assert.Equal(new PositionComponent(4, 2), component);
            Assert.Equal(world.Tick, restored.Tick);
        }

        [Fact]
        public void RollbackRestoresSnapshot()
        {
            var world = new World();
            var entity = world.CreateEntity();
            world.AddComponent(entity, new PositionComponent(0, 0));

            var buffer = new PredictionBuffer<VelocityComponent>();
            buffer.Record(world.Tick, new VelocityComponent(1, 0), world);

            world.SetComponent(entity, new PositionComponent(10, 0));
            var frame = buffer.Latest!;

            var rollback = new RollbackManager(world);
            rollback.Rollback(frame.Snapshot);

            var restored = world.RequireComponent<PositionComponent>(entity);
            Assert.Equal(new PositionComponent(0, 0), restored);
        }

        [Fact]
        public void InterestTrackerFiltersChangesByDistance()
        {
            var world = new World();
            var tracker = new InterestTracker();
            var entity = world.CreateEntity();
            world.AddComponent(entity, new InterestSubjectComponent(new Float3(0, 0, 0), 1f));
            world.AddComponent(entity, new PositionComponent(0, 0));
            world.ConsumeChanges();

            var subscription = tracker.Subscribe(new InterestObserver(new Float3(0, 0, 0), 5f));

            world.SetComponent(entity, new PositionComponent(1, 1));
            var changes = world.ConsumeChanges();
            var filtered = tracker.FilterChanges(changes, subscription, world).ToList();
            Assert.Single(filtered);

            tracker.Update(subscription, new InterestObserver(new Float3(10, 0, 0), 1f));
            world.SetComponent(entity, new PositionComponent(2, 2));
            var farChanges = world.ConsumeChanges();
            var filteredFar = tracker.FilterChanges(farChanges, subscription, world).ToList();
            Assert.Empty(filteredFar);
        }
    }
}
