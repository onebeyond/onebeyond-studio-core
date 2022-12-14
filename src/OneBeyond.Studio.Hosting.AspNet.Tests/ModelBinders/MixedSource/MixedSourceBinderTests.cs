using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource;
using OneBeyond.Studio.Hosting.AspNet.Tests.ModelBinders.BindingContext;
using OneBeyond.Studio.Hosting.AspNet.Tests.ModelBinders.BindingContext.BindingModels;

namespace OneBeyond.Studio.Hosting.AspNet.Tests.ModelBinders.MixedSource;

[TestClass]
public sealed class MixedSourceBinderTests
{
    [TestMethod]
    public async Task DefaultSourceOrderIsBodyRoute()
    {
        var context = new TestBindingContext(
            new MixedSourceBinderSource(),
            new TestMetadata(typeof(TestBindingModelMixed)));

        var values = await MixedSourceBinder.RetrieveValuesAsync(context);

        values.Should().NotBeNull();
        values.Count.Should().Be(3);
        values.Value<string>("id").Should().Be(TestBindingContext.ID_FROM_ROUTE); //Body values override the route ones
        values.Value<string>("bodyVal").Should().Be(TestBindingContext.VAL_FROM_BODY);
        values.Value<string>("routeVal").Should().Be(TestBindingContext.VAL_FROM_ROUTE);
    }

    [TestMethod]
    public async Task BodyValuesOverrideRoute()
    {
        var context = new TestBindingContext(new MixedSourceBinderSource(
            new string[] { Source.ROUTE, Source.BODY }),
            new TestMetadata(typeof(TestBindingModelMixed)));


        var values = await MixedSourceBinder.RetrieveValuesAsync(context);

        values.Should().NotBeNull();
        values.Count.Should().Be(3);
        values.Value<string>("id").Should().Be(TestBindingContext.ID_FROM_BODY); //Body values override the route ones
        values.Value<string>("bodyVal").Should().Be(TestBindingContext.VAL_FROM_BODY);
        values.Value<string>("routeVal").Should().Be(TestBindingContext.VAL_FROM_ROUTE);
    }

    [TestMethod]
    public async Task RouteValuesOverrideBody()
    {
        var context = new TestBindingContext(
            new MixedSourceBinderSource(new string[] { Source.BODY, Source.ROUTE }),
            new TestMetadata(typeof(TestBindingModelMixed)));

        var values = await MixedSourceBinder.RetrieveValuesAsync(context);

        values.Should().NotBeNull();
        values.Count.Should().Be(3);
        values.Value<string>("id").Should().Be(TestBindingContext.ID_FROM_ROUTE); //Route values override the body ones
        values.Value<string>("bodyVal").Should().Be(TestBindingContext.VAL_FROM_BODY);
        values.Value<string>("routeVal").Should().Be(TestBindingContext.VAL_FROM_ROUTE);
    }

    [TestMethod]
    public async Task BodyValuesOnly()
    {
        var context = new TestBindingContext(
            new MixedSourceBinderSource(new string[] { Source.BODY }),
            new TestMetadata(typeof(TestBindingModelMixed)));

        var values = await MixedSourceBinder.RetrieveValuesAsync(context);

        values.Should().NotBeNull();
        values.Count.Should().Be(2);
        values.Value<string>("id").Should().Be(TestBindingContext.ID_FROM_BODY);
        values.Value<string>("bodyVal").Should().Be(TestBindingContext.VAL_FROM_BODY);
    }

    [TestMethod]
    public async Task RouteValuesOnly()
    {
        var context = new TestBindingContext(
            new MixedSourceBinderSource(new string[] { Source.ROUTE }),
            new TestMetadata(typeof(TestBindingModelMixed)));

        var values = await MixedSourceBinder.RetrieveValuesAsync(context);

        values.Should().NotBeNull();
        values.Count.Should().Be(2);
        values.Value<string>("id").Should().Be(TestBindingContext.ID_FROM_ROUTE);
        values.Value<string>("routeVal").Should().Be(TestBindingContext.VAL_FROM_ROUTE);
    }
}
