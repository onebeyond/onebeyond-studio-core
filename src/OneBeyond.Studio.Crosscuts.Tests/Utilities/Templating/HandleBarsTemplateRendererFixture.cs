using System;
using System.IO;
using HandlebarsDotNet;
using OneBeyond.Studio.Crosscuts.Utilities.Templating;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests.Utilities.Templating;

[CollectionDefinition(nameof(HandleBarsTemplateRenderer))]
public sealed class HandleBarsTemplateRendererCollection : ICollectionFixture<HandleBarsTemplateRendererFixture>
{
}

public sealed class HandleBarsTemplateRendererFixture : IDisposable
{
    public const string LayoutName = "Layout";

    private const string LayoutTemplate = """
        <html>
        <body>
        {{{> @partial-block }}}
        <footer>
        <p>Best Regards,<br />
        {{systemName}}</p>
        </footer>
        </body>
        </html>
        """;

    public HandleBarsTemplateRendererFixture()
    {
        HandlebarsLayoutTemplateManager.RegisterLayout(LayoutName, LayoutTemplate);
    }

    public void Dispose()
    {
        Handlebars.RegisterTemplate(
            LayoutName,
            (HandlebarsTemplate<TextWriter, object, object>)null!);
    }
}
