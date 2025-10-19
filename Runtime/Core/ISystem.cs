namespace PulseECS.Core
{
    /// <summary>
    /// Systems advance the world simulation by operating on entity views.
    /// </summary>
    public interface ISystem
    {
        void Update(World world, float deltaTime);
    }
}
