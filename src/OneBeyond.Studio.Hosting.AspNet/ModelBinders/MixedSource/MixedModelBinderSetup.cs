using EnsureThat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource;

internal sealed class MixedModelBinderSetup : IPostConfigureOptions<MvcOptions>
{
    public void PostConfigure(string? name, MvcOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));

        var provider = new MixedSourceBinderProvider(
            new MixedSourceBinderSource(),
            new MixedSourceBinder());

        options.ModelBinderProviders.Insert(0, provider);
    }
}
