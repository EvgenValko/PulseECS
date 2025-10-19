# Pulse ECS

Pulse ECS is a deterministic, network-focused entity component system that supports client prediction, rollback, change tracking, interest management and JSON world persistence. The core runtime is a pure .NET Standard assembly and can be consumed both inside Unity (via the package manager) and in standalone .NET applications.

## Features

- **Deterministic world core** – lightweight entity/component storage with change tracking for replicated data.
- **Client prediction buffers** – record local inputs alongside world snapshots for later reconciliation.
- **Authoritative rollback** – fast state restoration using JSON snapshots when the server sends corrections.
- **Interest management** – zone of interest filtering for subscriptions and visualization logic.
- **State persistence** – export and restore the full persisted world state as JSON.
- **Unity package support** – installable through the Unity package manager via Git URL.

## Installation in Unity

1. Open the Unity Package Manager.
2. Choose **Add package from git URL...**
3. Paste the repository URL, for example:
   ```
   https://github.com/your-org/PulseECS.git
   ```
4. Press **Add**. Unity will import the runtime assembly definition automatically.

## Usage

Create a world, define components with the provided attributes and run your systems or networking loop.

```csharp
using PulseECS.Core;
using PulseECS.Networking.Attributes;

[Replicated]
[Persisted]
[TrackChanges]
public sealed record Position(float X, float Y) : IComponent;

var world = new World();
var entity = world.CreateEntity();
world.AddComponent(entity, new Position(0, 0));

// Serialize to JSON for persistence or rollback
var json = JsonWorldSerializer.Save(world);
```

See the [`Samples~/GettingStarted`](Samples~/GettingStarted/README.md) folder for a step-by-step introduction.

## Testing

This repository includes xUnit tests for the deterministic world core and networking utilities. Run them with:

```bash
dotnet test tests/PulseECS.Tests/PulseECS.Tests.csproj
```

> **Note**: If your environment does not provide the .NET SDK you will need to install it before running the tests.
