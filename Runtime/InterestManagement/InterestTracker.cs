using System;
using System.Collections.Generic;
using PulseECS.Core;
using PulseECS.Math;
using PulseECS.Networking.ChangeTracking;

namespace PulseECS.InterestManagement
{
    /// <summary>
    /// Tracks interest observers and filters entity changes according to their zones of interest.
    /// </summary>
    public sealed class InterestTracker
    {
        private readonly Dictionary<Guid, InterestObserver> _observers = new();

        public Guid Subscribe(InterestObserver observer)
        {
            var id = Guid.NewGuid();
            _observers[id] = observer;
            return id;
        }

        public void Update(Guid subscriptionId, InterestObserver observer)
        {
            if (!_observers.ContainsKey(subscriptionId))
            {
                throw new InvalidOperationException($"Unknown subscription {subscriptionId}.");
            }

            _observers[subscriptionId] = observer;
        }

        public void Unsubscribe(Guid subscriptionId)
        {
            _observers.Remove(subscriptionId);
        }

        public IReadOnlyCollection<EntityId> Query(World world, Guid subscriptionId)
        {
            if (!_observers.TryGetValue(subscriptionId, out var observer))
            {
                throw new InvalidOperationException($"Unknown subscription {subscriptionId}.");
            }

            var results = new List<EntityId>();
            foreach (var (entityId, subject) in world.View<InterestSubjectComponent>())
            {
                if (IsWithin(observer, subject))
                {
                    results.Add(entityId);
                }
            }

            return results;
        }

        public IEnumerable<ComponentChange> FilterChanges(ComponentChangeSet changeSet, Guid subscriptionId, World world)
        {
            if (!_observers.TryGetValue(subscriptionId, out var observer))
            {
                yield break;
            }

            var relevantEntities = new HashSet<EntityId>(Query(world, subscriptionId));

            foreach (var change in changeSet.Changes)
            {
                if (relevantEntities.Contains(change.EntityId))
                {
                    yield return change;
                }
            }
        }

        private static bool IsWithin(InterestObserver observer, InterestSubjectComponent subject)
        {
            var delta = subject.Position - observer.Position;
            var distance = delta.Magnitude;
            return distance <= observer.Radius + subject.Radius;
        }
    }
}
