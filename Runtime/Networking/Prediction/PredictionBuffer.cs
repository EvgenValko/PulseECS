using System;
using System.Collections.Generic;
using System.Linq;
using PulseECS.Core;
using PulseECS.Networking.Serialization;

namespace PulseECS.Networking.Prediction
{
    /// <summary>
    /// Stores client inputs and associated world snapshots for prediction/rollback.
    /// </summary>
    public sealed class PredictionBuffer<TInput>
        where TInput : notnull
    {
        private readonly SortedDictionary<ulong, PredictionFrame> _frames = new();

        public sealed class PredictionFrame
        {
            public required ulong Tick { get; init; }
            public required TInput Input { get; init; }
            public required WorldSnapshot Snapshot { get; init; }
        }

        public IReadOnlyCollection<PredictionFrame> Frames => _frames.Values.ToList();

        public void Record(ulong tick, TInput input, World world)
        {
            _frames[tick] = new PredictionFrame
            {
                Tick = tick,
                Input = input,
                Snapshot = WorldSnapshot.Capture(world)
            };
        }

        public bool TryGet(ulong tick, out PredictionFrame frame)
        {
            return _frames.TryGetValue(tick, out frame!);
        }

        public void DiscardUpTo(ulong tick)
        {
            var keysToRemove = _frames.Keys.Where(k => k <= tick).ToList();
            foreach (var key in keysToRemove)
            {
                _frames.Remove(key);
            }
        }

        public PredictionFrame? Latest => _frames.Count > 0 ? _frames.Last().Value : null;
    }
}
