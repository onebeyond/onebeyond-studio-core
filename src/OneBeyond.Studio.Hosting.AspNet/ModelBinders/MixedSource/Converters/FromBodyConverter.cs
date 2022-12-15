using System.IO;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource.Converters;

internal sealed class FromBodyConverter : IRequestToJObjectConverter
{
    public async ValueTask<JObject> ConvertAsync(ModelBindingContext bindingContext)
    {
        EnsureArg.IsNotNull(bindingContext, nameof(bindingContext));

        using (var streamReader = new StreamReader(bindingContext.HttpContext.Request.Body))
        {
            var body = await streamReader.ReadToEndAsync();
            return JObject.Parse(body);
        }
    }
}
