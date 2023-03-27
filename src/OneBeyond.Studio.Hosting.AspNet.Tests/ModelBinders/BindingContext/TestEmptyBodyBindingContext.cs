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

internal sealed class TestBodyBindingContext : ModelBindingContext
{
    private readonly string _testRequestData = "";

    private BindingSource? _bindingSource;
    private ActionContext _actionContext;
    private readonly HttpContext _httpContext;
    private readonly ModelMetadata _modelMetadata;

    public TestBodyBindingContext(MixedSourceBinderSource binderSource, TestMetadata modelMetadata, string testRequestData)
    {
        EnsureArg.IsNotNull(binderSource, nameof(binderSource));
        EnsureArg.IsNotNull(modelMetadata, nameof(modelMetadata));

        _testRequestData = testRequestData;
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
        return new ActionContext(new Mock<HttpContext>().Object,
                                 new Mock<RouteData>().Object,
                                 new Mock<ActionDescriptor>().Object);
    }

    private HttpContext GenerateMockHttpContext()
    {
        var request = new Mock<HttpRequest>();
        request.Setup(x => x.Body).Returns(GenerateStreamFromString(_testRequestData));

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(x => x.Request).Returns(request.Object);
        return httpContext.Object;
    }

    private Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
