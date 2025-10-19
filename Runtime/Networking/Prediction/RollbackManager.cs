using System;
using PulseECS.Core;
using PulseECS.Networking.Serialization;

namespace PulseECS.Networking.Prediction
{
    /// <summary>
    /// Applies stored snapshots to restore the world when authoritative updates arrive.
    /// </summary>
    public sealed class RollbackManager
    {
        private readonly World _world;

        public RollbackManager(World world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public void Rollback(WorldSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            snapshot.Apply(_world);
        }

        public void Rollback(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            JsonWorldSerializer.Load(_world, json);
        }
    }
}
