using System;
using EnsureThat;
using HandlebarsDotNet;

namespace OneBeyond.Studio.Crosscuts.Utilities.Templating;

/// <summary>
/// Renders a template written in mustache {{}} syntax using the <see langword="static"/> instance of <see cref="HandlebarsDotNet"/>.
/// Templates registered via <see cref="HandlebarsLayoutTemplateManager"/> will be applied during rendering.
/// </summary>
public class HandleBarsTemplateRenderer : ITemplateRenderer
{
    [Obsolete($"Use {nameof(Render)} instead.")]
    public string RenderTemplate(string template, object? parameters)
        => Render(template, parameters);

    /// <inheritdoc/>
    public string Render(string template, object? parameters)
    {
        EnsureArg.IsNotNullOrWhiteSpace(template, nameof(template));
        HandlebarsLayoutTemplateManager.TryApplyLayout();

        var compiledTemplate = Handlebars.Compile(template);
        return compiledTemplate(parameters);
    }
}
