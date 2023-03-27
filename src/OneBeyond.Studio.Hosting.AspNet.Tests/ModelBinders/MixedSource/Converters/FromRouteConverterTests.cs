using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource;
using OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource.Converters;
using OneBeyond.Studio.Hosting.AspNet.Tests.ModelBinders.BindingContext;
using OneBeyond.Studio.Hosting.AspNet.Tests.ModelBinders.BindingContext.BindingModels;

namespace OneBeyond.Studio.Hosting.AspNet.Tests.ModelBinders.MixedSource.Converters;

[TestClass]
public sealed class FromRouteConverterTests
{
    [TestMethod]
    public async Task BindRouteModelPropertiesOnly()
    {
        //route: id: '', routeVal: ''
        //TestBindingModelRoute: {id, routeVal}
        //Both id and routeVal should be bound
        var context = new TestBindingContext(new MixedSourceBinderSource(), new TestMetadata(typeof(TestBindingModelRoute)));

        var converter = new FromRouteConverter();

        var token = await converter.ConvertAsync(context);
        token.Type.Should().Be(JTokenType.Object);
        var values = (JObject)token;
        values.Should().NotBeNull();
        values.Count.Should().Be(2);
        values.Value<string>("id").Should().Be(TestBindingContext.ID_FROM_ROUTE);
        values.Value<string>("routeVal").Should().Be(TestBindingContext.VAL_FROM_ROUTE);
    }

    [TestMethod]
    public async Task BindBodyModelPropertiesOnly()
    {
        //route: id: '', routeVal: ''
        //TestBindingModelBody: {id, bodyVal}
        //Only id should be bound
        var context = new TestBindingContext(new MixedSourceBinderSource(), new TestMetadata(typeof(TestBindingModelBody)));

        var converter = new FromRouteConverter();

        var token = await converter.ConvertAsync(context);
        token.Type.Should().Be(JTokenType.Object);
        var values = (JObject)token;
        values.Should().NotBeNull();
        values.Count.Should().Be(1);
        values.Value<string>("id").Should().Be(TestBindingContext.ID_FROM_ROUTE);
    }

    [TestMethod]
    public async Task BindEmptyBody()
    {
        const string testRequestBody = "";
        var context = new TestBodyBindingContext(new MixedSourceBinderSource(), 
                                                 new TestMetadata(typeof(TestBindingModelBody)),
                                                 testRequestBody);

        var converter = new FromBodyConverter();

        var token = await converter.ConvertAsync(context);
        token.Type.Should().Be(JTokenType.Object);
        var values = (JObject)token;
        values.Should().NotBeNull();
        values.Count.Should().Be(0);
    }

    [TestMethod]
    public async Task BindEmptyStringBody()
    {
        const string testRequestBody = "\"\"";
        const string expectedResult = "";
        var context = new TestBodyBindingContext(new MixedSourceBinderSource(),
                                                 new TestMetadata(typeof(TestBindingModelBody)),
                                                 testRequestBody);

        var converter = new FromBodyConverter();

        var token = await converter.ConvertAsync(context);

        token.Type.Should().Be(JTokenType.String);
        var value = (JValue)token;
        value.Value<string>().Should().NotBeNull();
        value.Value<string>().Should().Be(expectedResult);
    }
}
