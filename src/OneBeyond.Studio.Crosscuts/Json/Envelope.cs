using System.Collections.Generic;
using EnsureThat;
using Newtonsoft.Json;

namespace OneBeyond.Studio.Crosscuts.Json;

/// <summary>
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class Envelope<TValue>
    where TValue : class
{
    private readonly TValue _value;

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    public Envelope(TValue value)
    {
        EnsureArg.IsNotNull(value, nameof(value));

        _value = value;
    }

    /// <summary>
    /// </summary>
    /// <param name="valueJson"></param>
    [JsonConstructor]
    protected Envelope(string valueJson)
        : this(
              JsonConvert.DeserializeObject<TValue>(
                  EnsureArg.IsNotNullOrWhiteSpace(
                      valueJson,
                      nameof(valueJson)),
                  JsonSerializerSettings)!)
    {
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new List<JsonConverter>
            {
            }
        };

    /// <summary>
    /// </summary>
    public string ValueJson =>
        JsonConvert.SerializeObject(
            _value,
            JsonSerializerSettings);

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public TValue GetValue()
        => _value;
}
