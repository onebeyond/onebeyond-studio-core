using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource.Converters;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource;

internal sealed class MixedSourceBinder : IModelBinder
{
    private static readonly IReadOnlyDictionary<string, IRequestToJObjectConverter> Converters =
        new Dictionary<string, IRequestToJObjectConverter>
        {
                { Source.ROUTE, new FromRouteConverter() },
                { Source.BODY, new FromBodyConverter() }
        };

    private static readonly JsonMergeSettings JsonMergeSettings =
        new()
        {
            MergeArrayHandling = MergeArrayHandling.Union
        };

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        EnsureArg.IsNotNull(bindingContext, nameof(bindingContext));

        var values = await RetrieveValuesAsync(bindingContext);

        var currentJsonOptions = bindingContext.HttpContext.RequestServices
            .GetService<IOptions<MvcNewtonsoftJsonOptions>>()?.Value
            ?? new MvcNewtonsoftJsonOptions();

        var model = values.ToObject(
            bindingContext.ModelMetadata.UnderlyingOrModelType,
            JsonSerializer.Create(currentJsonOptions.SerializerSettings));

        bindingContext.Result = ModelBindingResult.Success(model);
    }

    internal static async Task<JObject> RetrieveValuesAsync(ModelBindingContext bindingContext)
    {
        EnsureArg.IsNotNull(bindingContext, nameof(bindingContext));
        EnsureArg.IsNotNull(bindingContext.BindingSource, nameof(bindingContext.BindingSource));

        var bindingOrder = ((MixedSourceBinderSource)bindingContext.BindingSource).BindingOrder;

        var values = new JObject();

        foreach (var binderSource in bindingOrder)
        {
            if (Converters.TryGetValue(binderSource, out var converter))
            {
                values.Merge(
                    await converter.ConvertAsync(bindingContext),
                    JsonMergeSettings);
            }
        }

        return values;
    }
}
