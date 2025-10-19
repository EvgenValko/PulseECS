namespace PulseECS.Core
{
    /// <summary>
    /// Marker interface for all components. Components should be immutable records or structs
    /// whenever possible to simplify diffing and deterministic simulation.
    /// </summary>
    public interface IComponent
    {
    }
}
