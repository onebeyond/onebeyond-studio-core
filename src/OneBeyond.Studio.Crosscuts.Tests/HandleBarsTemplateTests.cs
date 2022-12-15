using FluentAssertions;
using OneBeyond.Studio.Crosscuts.Utilities.Templating;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests;

public sealed class HandleBarsTemplateTests
{
    [Fact]
    public void ComplexReplacementCase()
    {
        ITemplateRenderer renderer = new HandleBarsTemplateRenderer();

        var template = @"<html>
<body>
<p>Hello {{userName}},</p>
<p>You have been sent this email as an invitation to access Alexis' Test. In order to access the system you will first need to set a password. Please click <a href=""{{callbackUrl}}"">here</a> to set your password.</p>
<p>If you're having trouble clicking the link, copy and paste the URL below into your web browser: {{callbackUrl}}.</p>
<p>To log in to your account use the following user name: {{userName}}.</p>
<p>Best Regards,<br />
{{systemName}}</p></body></html>";

        var parameters = new
        {
            userName = "Alexis",
            callbackUrl = "https://testserver/resetpassword=xxasjkalsjdalkjasdlkj",
            systemName = "Test System"
        };

        var output = renderer.RenderTemplate(template, parameters);

        var expected = @"<html>
<body>
<p>Hello Alexis,</p>
<p>You have been sent this email as an invitation to access Alexis' Test. In order to access the system you will first need to set a password. Please click <a href=""https://testserver/resetpassword=xxasjkalsjdalkjasdlkj"">here</a> to set your password.</p>
<p>If you're having trouble clicking the link, copy and paste the URL below into your web browser: https://testserver/resetpassword=xxasjkalsjdalkjasdlkj.</p>
<p>To log in to your account use the following user name: Alexis.</p>
<p>Best Regards,<br />
Test System</p></body></html>";

        output.Should().Be(expected);
    }

    [Fact]
    public void MissingVariablesShouldBeRemoved()
    {
        ITemplateRenderer renderer = new HandleBarsTemplateRenderer();

        var template = @"<html>
<body>
<p>Hello {{userName}},</p>
<p>You have been sent this email as an invitation to access Alexis' Test. In order to access the system you will first need to set a password. Please click <a href=""{{callbackUrl}}"">here</a> to set your password.</p>
<p>If you're having trouble clicking the link, copy and paste the URL below into your web browser: {{callbackUrl}}.</p>
<p>To log in to your account use the following user name: {{userName}}.</p>
<p>Best Regards,<br />
{{systemName}}</p></body></html>";

        var parameters = new
        {
            userName = "Alexis",
            callbackUrl = "https://testserver/resetpassword=xxasjkalsjdalkjasdlkj"
        };

        var output = renderer.RenderTemplate(template, parameters);

        var expected = @"<html>
<body>
<p>Hello Alexis,</p>
<p>You have been sent this email as an invitation to access Alexis' Test. In order to access the system you will first need to set a password. Please click <a href=""https://testserver/resetpassword=xxasjkalsjdalkjasdlkj"">here</a> to set your password.</p>
<p>If you're having trouble clicking the link, copy and paste the URL below into your web browser: https://testserver/resetpassword=xxasjkalsjdalkjasdlkj.</p>
<p>To log in to your account use the following user name: Alexis.</p>
<p>Best Regards,<br />
</p></body></html>";

        output.Should().Be(expected);
    }

    [Fact]
    public void SimpleReplacementCase()
    {
        ITemplateRenderer renderer = new HandleBarsTemplateRenderer();
        var template = @"Basic template with one {{variable}}";

        var parameters = new
        {
            variable = "replacement variable!"
        };

        var output = renderer.RenderTemplate(template, parameters);
        var expected = "Basic template with one replacement variable!";
        output.Should().Be(expected);
    }
}
