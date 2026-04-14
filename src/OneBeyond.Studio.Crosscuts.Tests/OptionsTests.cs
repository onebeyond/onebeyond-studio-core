using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Options;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests;


public sealed class OptionsTests : TestsBase
{

    [Fact]    
    public void TestOptionsExceptionThrownWhenSectionNotFound()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        Assert.Throws<OptionsException>(() => configuration.GetOptions<OptionsTestsOptions>("OptionsTestsNonExisting"));        
    }

    [Fact]    
    public void TestOptionsExceptionThrownWhenSectionEmpty()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        
        Assert.Throws<OptionsException>(() => configuration.GetOptions<OptionsTestsOptions>("OptionsTests:Empty"));
    }

    [Fact]
    public void TestOptionsDataCanBeMappedToPrivateSetter()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        var someApiOptions = configuration.GetOptions<OptionsTestsOptions>("OptionsTests");

        Assert.Equal("Private Setter", someApiOptions.SecretKey);
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

