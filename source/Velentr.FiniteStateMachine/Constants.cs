using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Velentr.FiniteStateMachine;

/// <summary>
///     Provides constants and serialization settings for the FiniteStateMachine class.
/// </summary>
public static class Constants
{
    /// <summary>
    ///     Defines serialization settings for JSON serialization of finite state machine objects.
    /// </summary>
    public static readonly JsonSerializerOptions SerializationOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
}
