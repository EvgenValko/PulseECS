using PulseECS.Core;
using PulseECS.Math;
using PulseECS.Networking.Attributes;

namespace PulseECS.InterestManagement
{
    /// <summary>
    /// Attach to entities that should be spatially filterable.
    /// </summary>
    [Replicated]
    [Persisted]
    [TrackChanges]
    public sealed record InterestSubjectComponent(Float3 Position, float Radius) : IComponent;

    /// <summary>
    /// Describes an observer that should receive updates for a zone of interest.
    /// </summary>
    public sealed record InterestObserver(Float3 Position, float Radius);
}
