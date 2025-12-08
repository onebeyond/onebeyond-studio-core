using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Options;

namespace OneBeyond.Studio.Crosscuts.Tests;

[TestClass]
public sealed class OptionsTests : TestsBase
{
    [TestMethod]    
    public void TestOptionsExceptionThrownWhenSectionNotFound()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        Assert.Throws<OptionsException>(() => configuration.GetOptions<OptionsTestsOptions>("OptionsTestsNonExisting"));        
    }

    [TestMethod]    
    public void TestOptionsExceptionThrownWhenSectionEmpty()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        
        Assert.Throws<OptionsException>(() => configuration.GetOptions<OptionsTestsOptions>("OptionsTests:Empty\""));
    }

    [TestMethod]
    public void TestOptionsDataCanBeMappedToPrivateSetter()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        var someApiOptions = configuration.GetOptions<OptionsTestsOptions>("OptionsTests");

        Assert.AreEqual("Private Setter", someApiOptions.SecretKey);
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        IServiceCollection serviceCollection)
    {
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        ContainerBuilder containerBuilder)
    {
    }

    private sealed class OptionsTestsOptions
    {
        public string? SecretKey { get; private set; }
    }
}
