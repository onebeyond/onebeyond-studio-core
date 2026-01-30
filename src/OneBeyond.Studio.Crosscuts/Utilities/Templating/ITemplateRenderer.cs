using System;

namespace OneBeyond.Studio.Crosscuts.Utilities.Templating;

/// <summary>
/// Defines a contract for rendering templates with parameterized content.
/// </summary>
/// <remarks>Implementations of this interface provide methods to render templates by replacing placeholder tokens with values from supplied parameters.</remarks>
public interface ITemplateRenderer
{
    [Obsolete($"Use {nameof(Render)} instead.")]
    public string RenderTemplate(string template, object parameters);

    /// <summary>
    /// Renders the specified template using the provided parameters replacing placeholder tokens.
    /// </summary>
    /// <param name="template">The template <see cref="string"/> to render.</param>
    /// <param name="parameters">
    /// An <see cref="object"/> containing the parameters to use during rendering. Properties of this object are used to replace placeholders in the template.
    /// Can be <see langword="null"/> if the template does not require parameters.
    /// </param>
    /// <returns>A <see cref="string"/> containing the rendered output with all placeholders replaced by the corresponding parameter values.</returns>
    public string Render(string template, object? parameters);
}
