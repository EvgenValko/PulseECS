# Pulse ECS Getting Started

This sample demonstrates how to create a world, attach components and run a simple system.

```csharp
using PulseECS.Core;
using PulseECS.Networking.Attributes;

[Replicated]
[Persisted]
[TrackChanges]
public sealed record Position(float X, float Y) : IComponent;

public sealed class MovementSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        foreach (var (entity, position) in world.View<Position>())
        {
            world.SetComponent(entity, position with { X = position.X + 1f * deltaTime });
        }
    }
}
```
