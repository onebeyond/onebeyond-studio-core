using EnsureThat;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource;

internal sealed class MixedSourceBinderProvider : IModelBinderProvider
{
    private readonly BindingSource _bindingSource;
    private readonly IModelBinder _modelBinder;

    public MixedSourceBinderProvider(
        BindingSource bindingSource,
        IModelBinder modelBinder)
    {
        EnsureArg.IsNotNull(bindingSource, nameof(bindingSource));
        EnsureArg.IsNotNull(modelBinder, nameof(modelBinder));

        _bindingSource = bindingSource;
        _modelBinder = modelBinder;
    }

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.BindingInfo?.BindingSource != null &&
            context.BindingInfo.BindingSource.CanAcceptDataFrom(_bindingSource)
            ? _modelBinder
            : null;
    }
}
