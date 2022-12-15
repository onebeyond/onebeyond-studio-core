using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource.Converters;

internal interface IRequestToJObjectConverter
{
    ValueTask<JObject> ConvertAsync(ModelBindingContext bindingContext);
}
