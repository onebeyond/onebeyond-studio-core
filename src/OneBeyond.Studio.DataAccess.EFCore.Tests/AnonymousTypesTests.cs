using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;
using Xunit;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

public sealed class AnonymousTypesTests : InMemoryTestsBase
{
    public AnonymousTypesTests()
        : base(default)
    {
    }

    [Fact]
    public async Task TestAnonymousTypeGetById()
    {
        var product1 = new Product("ring", "jewelery", "cartier", 999.99M, "france");

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var productRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Product, Guid>>();

            await productRWRepository.CreateAsync(product1, TestContext.Current.CancellationToken);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var productRORepository = serviceScope.ServiceProvider
                .GetRequiredService<IRORepository<Product, Guid>>();

            var product1Dto = await productRORepository.GetByIdAsync(
                product1.Id,
                (product) => new
                {
                    ProductId = product.Id,
                    ProductName = product.Name
                },                TestContext.Current.CancellationToken);

            Assert.NotNull(product1Dto);
            Assert.Equal(product1.Id, product1Dto.ProductId);
            Assert.Equal(product1.Name, product1Dto.ProductName);

            var product2Dto = await productRORepository.GetByIdAsync(
                product1.Id,
                (product) => new
                {
                    ProductId = product.Id,
                    ProductType = product.Type,
                    ProductCountry = product.CountryOfOrigin,
                    ProductPrice = product.Price,
                    ProductBrand = product.Brand
                },                TestContext.Current.CancellationToken);

            Assert.NotNull(product2Dto);
            Assert.Equal(product1.Id, product2Dto.ProductId);
            Assert.Equal(product1.Type, product2Dto.ProductType);
            Assert.Equal(product1.CountryOfOrigin, product2Dto.ProductCountry);
            Assert.Equal(product1.Brand, product2Dto.ProductBrand);
            Assert.Equal(product1.Price, product2Dto.ProductPrice);
        }
    }

    [Fact]
    public async Task TestAnonymousList()
    {
        var product1 = new Product("ring", "jewelery", "cartier", 999.99M, "france");
        var product2 = new Product("ring", "jewelery", "tiffany", 1999.99M, "us");
        var product3 = new Product("diadem", "jewelery", "tiffany", 9999.99M, "us");
        var product4 = new Product("cup", "tableware", "tiffany", 145.00M, "us");
        var product5 = new Product("bracelet", "jewelery", "cartier", 1500.00M, "france");

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var productRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Product, Guid>>();

            await productRWRepository.CreateAsync(product1, TestContext.Current.CancellationToken);
            await productRWRepository.CreateAsync(product2, TestContext.Current.CancellationToken);
            await productRWRepository.CreateAsync(product3, TestContext.Current.CancellationToken);
            await productRWRepository.CreateAsync(product4, TestContext.Current.CancellationToken);
            await productRWRepository.CreateAsync(product5, TestContext.Current.CancellationToken);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var productRORepository = serviceScope.ServiceProvider
                .GetRequiredService<IRORepository<Product, Guid>>();

            var productDtos = await productRORepository.ListAsync(
                projection: (product) => new
                {
                    ProductId = product.Id,
                    ProductName = product.Name
                },
                filter: (product) => product.Brand == "cartier",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(productDtos);
            Assert.Equal(2, productDtos.Count());

            var product1Dto = productDtos.FirstOrDefault((product) => product.ProductId == product1.Id);
            Assert.NotNull(product1Dto);
            Assert.Equal(product1.Name, product1Dto!.ProductName);

            var product5Dto = productDtos.FirstOrDefault((product) => product.ProductId == product5.Id);
            Assert.NotNull(product5Dto);
            Assert.Equal(product5.Name, product5Dto!.ProductName);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var productRORepository = serviceScope.ServiceProvider
                .GetRequiredService<IRORepository<Product, Guid>>();

            var productDtos = await productRORepository.ListAsync(
                projection: (product) => new
                {
                    ProductId = product.Id,
                    ProductType = product.Type,
                    ProductPrice = product.Price
                },
                filter: (product) => product.CountryOfOrigin == "us" && product.Price > 1000M,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(productDtos);
            Assert.Equal(2, productDtos.Count());

            var product2Dto = productDtos.FirstOrDefault((product) => product.ProductId == product2.Id);
            Assert.NotNull(product2Dto);
            Assert.Equal(product2.Type, product2Dto!.ProductType);
            Assert.Equal(product2.Price, product2Dto.ProductPrice);

            var product3Dto = productDtos.FirstOrDefault((product) => product.ProductId == product3.Id);
            Assert.NotNull(product3Dto);
            Assert.Equal(product3.Type, product3Dto!.ProductType);
            Assert.Equal(product3.Price, product3Dto.ProductPrice);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var productRORepository = serviceScope.ServiceProvider
                .GetRequiredService<IRORepository<Product, Guid>>();

            var productDtos = (await productRORepository.ListAsync(
                projection: (product) => new
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductBrand = product.Brand,
                    ProductPrice = product.Price
                },
                filter: (product) => product.Price > 1000M,
                sortings: new List<Sorting<Product>> { Sorting.CreateDescending<Product>(x => x.Price) },
                cancellationToken: TestContext.Current.CancellationToken)
                ).ToArray();


            Assert.NotNull(productDtos);
            Assert.Equal(3, productDtos.Length);

            var product3Dto = productDtos[0];
            Assert.NotNull(product3Dto);
            Assert.Equal(product3.Id, product3Dto.ProductId);

            var product2Dto = productDtos[1];
            Assert.NotNull(product2Dto);
            Assert.Equal(product2.Id, product2Dto.ProductId);

            var product5Dto = productDtos[2];
            Assert.NotNull(product5Dto);
            Assert.Equal(product5.Id, product5Dto.ProductId);
        }
    }
}
