using System.Reflection;
using Autofac;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OneBeyond.Studio.Application.SharedKernel.CommandHandlers;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Entities.Dto;
using OneBeyond.Studio.Application.SharedKernel.Entities.Queries;
using OneBeyond.Studio.Application.SharedKernel.QueryHandlers;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.CommandHandlers;

[TestClass]
public sealed class GenericRequestHandlersRegistrationTests
{
    [TestMethod]
    public void TestRequestHandlerDispatcherIsResolvedIfNoClosedImplementationsAndNoKeyProvided()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.AddMediatorRequestHandlers();
        var container = containerBuilder.Build();

        var createHandler = container.Resolve<IRequestHandler<Create<SomeDto, SomeAggregateRoot, int>, int>>();
        Assert.IsInstanceOfType(createHandler, typeof(RequestHandlerDispatcher<Create<SomeDto, SomeAggregateRoot, int>, int>));

        var updateHandler = container.Resolve<IRequestHandler<Update<SomeDto, SomeAggregateRoot, int>, int>>();
        Assert.IsInstanceOfType(updateHandler, typeof(RequestHandlerDispatcher<Update<SomeDto, SomeAggregateRoot, int>, int>));

        var deleteHandler = container.Resolve<IRequestHandler<Delete<SomeAggregateRoot, int>, int>>();
        Assert.IsInstanceOfType(deleteHandler, typeof(RequestHandlerDispatcher<Delete<SomeAggregateRoot, int>, int>));

        var getByIdHandler = container.Resolve<IRequestHandler<GetById<SomeDto, SomeEntity, int>, SomeDto>>();
        Assert.IsInstanceOfType(getByIdHandler, typeof(RequestHandlerDispatcher<GetById<SomeDto, SomeEntity, int>, SomeDto>));

        var readHandler = container.Resolve<IRequestHandler<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>>();
        Assert.IsInstanceOfType(readHandler, typeof(RequestHandlerDispatcher<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>));
    }

    [TestMethod]
    public void TestClosedRequestHandlerIsResolvedIfThereIsOneAndNoKeyProvided()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.AddMediatorRequestHandlers(Assembly.GetExecutingAssembly());
        var container = containerBuilder.Build();

        var createHandler = container.Resolve<IRequestHandler<Create<SomeDto, SomeAggregateRoot, int>, int>>();
        Assert.IsInstanceOfType(createHandler, typeof(ClosedCreateHandler));

        var updateHandler = container.Resolve<IRequestHandler<Update<SomeDto, SomeAggregateRoot, int>, int>>();
        Assert.IsInstanceOfType(updateHandler, typeof(RequestHandlerDispatcher<Update<SomeDto, SomeAggregateRoot, int>, int>));

        var deleteHandler = container.Resolve<IRequestHandler<Delete<SomeAggregateRoot, int>, int>>();
        Assert.IsInstanceOfType(deleteHandler, typeof(RequestHandlerDispatcher<Delete<SomeAggregateRoot, int>, int>));

        var getByIdHandler = container.Resolve<IRequestHandler<GetById<SomeDto, SomeEntity, int>, SomeDto>>();
        Assert.IsInstanceOfType(getByIdHandler, typeof(RequestHandlerDispatcher<GetById<SomeDto, SomeEntity, int>, SomeDto>));

        var readHandler = container.Resolve<IRequestHandler<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>>();
        Assert.IsInstanceOfType(readHandler, typeof(RequestHandlerDispatcher<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>));
    }

    [TestMethod]
    public void TestGenericRequestHandlerIsResolvedIfKeyIsProvided()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register((ctx) => new Mock<IRWRepository<SomeAggregateRoot, int>>().Object).AsImplementedInterfaces();
        containerBuilder.Register((ctx) => new Mock<IRORepository<SomeEntity, int>>().Object).AsImplementedInterfaces();
        containerBuilder.Register((ctx) => new Mock<IValidator<SomeAggregateRoot>>().Object).AsImplementedInterfaces();
        containerBuilder.Register((ctx) => new Mock<IMapper>().Object).AsImplementedInterfaces();
        containerBuilder.AddMediatorRequestHandlers(Assembly.GetExecutingAssembly());
        var container = containerBuilder.Build();

        var createHandler = container.ResolveKeyed<IRequestHandler<Create<SomeDto, SomeAggregateRoot, int>, int>>(typeof(Create<,,>));
        Assert.IsInstanceOfType(createHandler, typeof(CreateHandler<SomeDto, SomeAggregateRoot, int>));

        var updateHandler = container.ResolveKeyed<IRequestHandler<Update<SomeDto, SomeAggregateRoot, int>, int>>(typeof(Update<,,>));
        Assert.IsInstanceOfType(updateHandler, typeof(UpdateHandler<SomeDto, SomeAggregateRoot, int>));

        var deleteHandler = container.ResolveKeyed<IRequestHandler<Delete<SomeAggregateRoot, int>, int>>(typeof(Delete<,>));
        Assert.IsInstanceOfType(deleteHandler, typeof(DeleteHandler<SomeAggregateRoot, int>));

        var getByIdHandler = container.ResolveKeyed<IRequestHandler<GetById<SomeDto, SomeEntity, int>, SomeDto>>(typeof(GetById<,,>));
        Assert.IsInstanceOfType(getByIdHandler, typeof(GetByIdHandler<SomeDto, SomeEntity, int>));

        var readHandler = container.ResolveKeyed<IRequestHandler<List<SomeDto, SomeEntity, int>, PagedList<SomeDto>>>(typeof(List<,,>));
        Assert.IsInstanceOfType(readHandler, typeof(ListHandler<SomeDto, SomeEntity, int>));
    }
}
