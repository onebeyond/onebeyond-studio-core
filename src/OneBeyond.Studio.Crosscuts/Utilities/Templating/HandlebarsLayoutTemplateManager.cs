using System;
using System.Threading;
using HandlebarsDotNet;

namespace OneBeyond.Studio.Crosscuts.Utilities.Templating;

/// <summary>
/// Provides thread-safe registration of <see cref="HandlebarsDotNet"/> templates.
/// Ensures a template is registered only once in multi-threaded scenarios.
/// </summary>
public static class HandlebarsLayoutTemplateManager
{
    private static Lazy<bool>? Registration
    {
        get => Volatile.Read(ref field);
        set => Interlocked.Exchange(ref field, value);
    }

    internal static bool TryApplyLayout() => Registration?.Value ?? false;

    /// <summary>
    /// Registers a layout template with the specified name and content.
    /// Registration is performed in a thread-safe manner and will only occur once.
    /// </summary>
    /// <param name="name">The unique name of the layout template.</param>
    /// <param name="template">The content of the layout template.</param>
    public static void RegisterLayout(string name, string template)
    {
        Registration = new Lazy<bool>(
            () =>
            {
                Handlebars.RegisterTemplate(name, template);
                return true;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
}
