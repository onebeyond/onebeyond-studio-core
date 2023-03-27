using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource.Converters;

internal interface IRequestToJTokenConverter
{
    ValueTask<JToken> ConvertAsync(ModelBindingContext bindingContext);
}
