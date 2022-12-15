using System;
using System.Collections.Generic;
using EnsureThat;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FromMixedSourceAttribute : Attribute, IBindingSourceMetadata
{
    public FromMixedSourceAttribute(params string[] bindingOrder)
    {
        EnsureArg.IsNotNull(bindingOrder, nameof(bindingOrder));

        BindingOrder = bindingOrder;
    }

    public IReadOnlyCollection<string> BindingOrder { get; }
    public BindingSource BindingSource => new MixedSourceBinderSource(BindingOrder);
}
