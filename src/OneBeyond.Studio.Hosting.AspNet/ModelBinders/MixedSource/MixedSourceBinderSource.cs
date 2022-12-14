using System.Collections.Generic;
using EnsureThat;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource;

internal sealed class MixedSourceBinderSource : BindingSource
{
    private static readonly IReadOnlyCollection<string> DefaultBindingOrder = new[] { Source.BODY, Source.ROUTE };

    public MixedSourceBinderSource(IReadOnlyCollection<string> bindingOrder)
        : base("Mixed", "Mixed", true, true)
    {
        EnsureArg.IsNotNull(bindingOrder, nameof(bindingOrder));

        BindingOrder = bindingOrder.Count > 0
            ? bindingOrder
            : DefaultBindingOrder;
    }

    public MixedSourceBinderSource()
        : this(DefaultBindingOrder)
    {
    }

    public IReadOnlyCollection<string> BindingOrder { get; }

    public override bool CanAcceptDataFrom(BindingSource bindingSource)
        => bindingSource.Id == "Mixed";
}
