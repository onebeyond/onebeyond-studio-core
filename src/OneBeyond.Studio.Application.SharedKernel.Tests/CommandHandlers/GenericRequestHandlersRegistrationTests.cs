using System.Reflection;
using Autofac;
using FluentValidation;
using Xunit;
using Moq;
using OneBeyond.Studio.Application.SharedKernel.CommandHandlers;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Entities.Dto;
using OneBeyond.Studio.Application.SharedKernel.Entities.Queries;
using OneBeyond.Studio.Application.SharedKernel.QueryHandlers;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Core.Mediator;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.CommandHandlers;


public sealed class GenericRequestHandlersRegistrationTests
{
    [Fact]
    public void TestRequestHandlerDispatcherIsResolvedIfNoClosedImplementationsAndNoKeyProvided()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.AddMediatorRequestHandlers();
        var container = containerBuilder.Build();        

        var deleteHandler = container.Resolve<IRequestHandler<Delete<SomeAggregateRoot, int>, int>>();
        Assert.IsType<RequestHandlerDispatcher<Delete<SomeAggregateRoot, int>, int>>(deleteHandler);

        var getByIdHandler = container.Resolve<IRequestHandler<GetById<SomeDto, SomeEntity, int>, SomeDto>>();
        Assert.IsType<RequestHandlerDispatcher<GetById<SomeDto, SomeEntity, int>, SomeDto>>(getByIdHandler);

        var readHandler = container.Resolve<IRequestHandler<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>>();
        Assert.IsType<RequestHandlerDispatcher<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>>(readHandler);
    }

    [Fact]
    public void TestClosedRequestHandlerIsResolvedIfThereIsOneAndNoKeyProvided()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.AddMediatorRequestHandlers(Assembly.GetExecutingAssembly());
        var container = containerBuilder.Build();        

        var deleteHandler = container.Resolve<IRequestHandler<Delete<SomeAggregateRoot, int>, int>>();
        Assert.IsType<RequestHandlerDispatcher<Delete<SomeAggregateRoot, int>, int>>(deleteHandler);

        var getByIdHandler = container.Resolve<IRequestHandler<GetById<SomeDto, SomeEntity, int>, SomeDto>>();
        Assert.IsType<RequestHandlerDispatcher<GetById<SomeDto, SomeEntity, int>, SomeDto>>(getByIdHandler);

        var readHandler = container.Resolve<IRequestHandler<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>>();
        Assert.IsType<RequestHandlerDispatcher<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>>(readHandler);
    }

    [Fact]
    public void TestGenericRequestHandlerIsResolvedIfKeyIsProvided()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register((ctx) => new Mock<IRWRepository<SomeAggregateRoot, int>>().Object).AsImplementedInterfaces();
        containerBuilder.Register((ctx) => new Mock<IRORepository<SomeEntity, int>>().Object).AsImplementedInterfaces();
        containerBuilder.Register((ctx) => new Mock<IValidator<SomeAggregateRoot>>().Object).AsImplementedInterfaces();        
        containerBuilder.AddMediatorRequestHandlers(Assembly.GetExecutingAssembly());
        var container = containerBuilder.Build();
               

        var deleteHandler = container.ResolveKeyed<IRequestHandler<Delete<SomeAggregateRoot, int>, int>>(typeof(Delete<,>));
        Assert.IsType<DeleteHandler<SomeAggregateRoot, int>>(deleteHandler);

        var getByIdHandler = container.ResolveKeyed<IRequestHandler<GetById<SomeDto, SomeEntity, int>, SomeDto>>(typeof(GetById<,,>));
        Assert.IsType<GetByIdHandler<SomeDto, SomeEntity, int>>(getByIdHandler);

        var readHandler = container.ResolveKeyed<IRequestHandler<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>>(typeof(List<,,>));
        Assert.IsType<ListHandler<SomeDto, SomeEntity, int>>(readHandler);
    }
}


