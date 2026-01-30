using System.Threading.Tasks;
using OneBeyond.Studio.Crosscuts.Utilities.Templating;
using VerifyXunit;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests.Utilities.Templating;

[Collection(nameof(HandleBarsTemplateRenderer))]
public sealed class HandleBarsTemplateRendererLayoutTests
{
    private readonly ITemplateRenderer _renderer;

    public HandleBarsTemplateRendererLayoutTests(HandleBarsTemplateRendererFixture _)
    {
        _renderer = new HandleBarsTemplateRenderer();
    }

    public static TheoryData<string, string, object> GetRenderData()
        => new TheoryData<string, string, object>
        {
            {
                "Basic",
                 $$$"""
                 {{#> {{{HandleBarsTemplateRendererFixture.LayoutName}}}}}
                 Basic template with one {{variable}}
                 {{/{{{HandleBarsTemplateRendererFixture.LayoutName}}}}}
                 """,
                 new
                 {
                     variable = "replacement!",
                     systemName = "Test System"
                 }
            },
            {
                "Complex",
                $$$$"""
                {{#> {{{{HandleBarsTemplateRendererFixture.LayoutName}}}}}}
                <p>Hello {{userName}},</p>
                <p>You have been sent this email as an invitation to access Alexis' Test. In order to access the system you will first need to set a password. Please click <a href="{{callbackUrl}}">here</a> to set your password.</p>
                <p>If you're having trouble clicking the link, copy and paste the URL below into your web browser: {{callbackUrl}}.</p>
                <p>To log in to your account use the following user name: {{userName}}.</p>
                {{/{{{{HandleBarsTemplateRendererFixture.LayoutName}}}}}}
                """,
                new
                {
                    userName = "Alexis",
                    callbackUrl = "https://testserver/resetpassword=xxasjkalsjdalkjasdlkj",
                    systemName = "Test System"
                }
            }
        };

    [Theory]
    [MemberData(nameof(GetRenderData))]
    public Task Render_WithLayout_Verify(string testCase, string template, object parameters)
    {
        // Arrange & Act
        var output = _renderer.Render(template, parameters);

        // Assert
        return Verifier.Verify(output).UseParameters(testCase);
    }
}
