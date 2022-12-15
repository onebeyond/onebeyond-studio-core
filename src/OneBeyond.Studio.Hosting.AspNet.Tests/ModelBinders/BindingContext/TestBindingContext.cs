using System;
using System.Collections.Generic;
using System.IO;
using EnsureThat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Moq;
using OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource;

namespace OneBeyond.Studio.Hosting.AspNet.Tests.ModelBinders.BindingContext;

// Body: "{ 'id':'id_from_body', 'bodyVal': 'val_from_body'}";
// Route: "{ 'id':'id_from_route', 'routeVal': 'val_from_route'}";
// Depending on the order we should get either:
//   -    { 'id':'id_from_body',  'bodyVal': 'val_from_body', 'routeVal': 'val_from_route'}} (body values override route)
//   - or { 'id':'id_from_route', 'bodyVal': 'val_from_body', 'routeVal': 'val_from_route'}} (route values override body)
internal sealed class TestBindingContext : ModelBindingContext
{
    public const string ID_FROM_BODY = "id_from_body";
    public const string ID_FROM_ROUTE = "id_from_route";

    public const string VAL_FROM_ROUTE = "val_from_route";
    public const string VAL_FROM_BODY = "val_from_body";
    private const string TEST_REQUEST_BODY = "{ 'id':'" + ID_FROM_BODY + "', 'bodyVal': '" + VAL_FROM_BODY + "'}";

    private readonly IDictionary<string, string> _testRouteValues = new Dictionary<string, string>()
        {
            { "id", ID_FROM_ROUTE },
            { "routeVal", VAL_FROM_ROUTE }
        };

    private BindingSource? _bindingSource;
    private ActionContext _actionContext;
    private readonly HttpContext _httpContext;
    private readonly ModelMetadata _modelMetadata;

    public TestBindingContext(MixedSourceBinderSource binderSource, TestMetadata modelMetadata)
    {
        EnsureArg.IsNotNull(binderSource, nameof(binderSource));
        EnsureArg.IsNotNull(modelMetadata, nameof(modelMetadata));

        _bindingSource = binderSource;
        _httpContext = GenerateMockHttpContext();
        _actionContext = GenerateMockActionContext();
        _modelMetadata = modelMetadata;
    }

    public override BindingSource? BindingSource
    {
        get => _bindingSource;
        set => _bindingSource = value;
    }

    public override HttpContext HttpContext => _httpContext;

    public override ActionContext ActionContext
    {
        get => _actionContext;
        set => _actionContext = value;
    }

    public override ModelMetadata ModelMetadata
    {
        get => _modelMetadata;
        set => throw new NotImplementedException();
    }

    public override string? BinderModelName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string FieldName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override bool IsTopLevelObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override object? Model { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string ModelName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override ModelStateDictionary ModelState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override Func<ModelMetadata, bool>? PropertyFilter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override ValidationStateDictionary ValidationState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override IValueProvider ValueProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override ModelBindingResult Result { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override NestedScope EnterNestedScope(ModelMetadata modelMetadata, string fieldName, string modelName, object? model)
        => throw new NotImplementedException();

    public override NestedScope EnterNestedScope()
        => throw new NotImplementedException();

    protected override void ExitNestedScope()
        => throw new NotImplementedException();

    private ActionContext GenerateMockActionContext()
    {
        var routeDataValues = new RouteValueDictionary(_testRouteValues!);
        var routeData = new RouteData(routeDataValues);
        return new ActionContext(new Mock<HttpContext>().Object, routeData, new Mock<ActionDescriptor>().Object);
    }

    private static HttpContext GenerateMockHttpContext()
    {
        var request = new Mock<HttpRequest>();
        request.Setup(x => x.Body).Returns(GenerateStreamFromString(TEST_REQUEST_BODY));

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(x => x.Request).Returns(request.Object);
        return httpContext.Object;
    }

    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
