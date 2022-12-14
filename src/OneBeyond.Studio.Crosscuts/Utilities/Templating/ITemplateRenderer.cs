namespace OneBeyond.Studio.Crosscuts.Utilities.Templating;

/// <summary>
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// </summary>
    /// <param name="template"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    string RenderTemplate(string template, object parameters);
}
