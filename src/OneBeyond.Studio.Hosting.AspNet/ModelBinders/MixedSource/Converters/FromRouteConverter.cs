using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource.Converters;

internal sealed class FromRouteConverter : IRequestToJTokenConverter
{
    public ValueTask<JToken> ConvertAsync(ModelBindingContext bindingContext)
    {
        EnsureArg.IsNotNull(bindingContext, nameof(bindingContext));

        //Only get route params that can be found on the type we are binding to
        var routeParams = bindingContext.ActionContext.RouteData.Values
            .Where((value) => bindingContext.ModelMetadata.UnderlyingOrModelType.GetProperties()
                .Select((prop) => prop.Name.ToLower())
                .Contains(value.Key.ToLower()))
            .ToDictionary(x => x.Key, x => x.Value);

        return new ValueTask<JToken>(JToken.FromObject(routeParams));
    }
}
