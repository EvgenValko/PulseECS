using System.Text.Json;
using System.Text.Json.Serialization;
using PulseECS.Core;

namespace PulseECS.Networking.Serialization
{
    /// <summary>
    /// Serializes and deserializes world snapshots using System.Text.Json.
    /// </summary>
    public static class JsonWorldSerializer
    {
        internal static readonly JsonSerializerOptions SerializerOptions = new()
        {
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static string Save(World world)
        {
            var snapshot = WorldSnapshot.Capture(world);
            return JsonSerializer.Serialize(snapshot, SerializerOptions);
        }

        public static void Load(World world, string json)
        {
            var snapshot = JsonSerializer.Deserialize<WorldSnapshot>(json, SerializerOptions);
            if (snapshot == null)
            {
                return;
            }

            snapshot.Apply(world);
        }
    }
}
