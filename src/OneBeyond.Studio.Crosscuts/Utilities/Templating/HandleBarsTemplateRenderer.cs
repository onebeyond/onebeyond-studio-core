using EnsureThat;
using HandlebarsDotNet;

namespace OneBeyond.Studio.Crosscuts.Utilities.Templating;

/// <summary>
///     Renders a template written using mustache {{}} syntax
/// </summary>
public class HandleBarsTemplateRenderer : ITemplateRenderer
{
    /// <summary>
    /// </summary>
    /// <param name="template"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public string RenderTemplate(string template, object parameters)
    {
        EnsureArg.IsNotNullOrWhiteSpace(template, nameof(template));

        var compiledTemplate = Handlebars.Compile(template);
        return compiledTemplate(parameters);
    }
}
