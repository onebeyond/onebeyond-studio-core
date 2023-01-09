using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

[TestClass]
public sealed class AnonymousTypesTests : InMemoryTestsBase
{
    public AnonymousTypesTests()
        : base(default)
    {
    }

    [TestMethod]
    public async Task TestAnonymousTypeGetById()
    {
        var product1 = new Product("ring", "jewelery", "cartier", 999.99M, "france");

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var productRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Product, Guid>>();

            await productRWRepository.CreateAsync(product1, default);
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
                },
                default);

            Assert.IsNotNull(product1Dto);
            Assert.AreEqual(product1.Id, product1Dto.ProductId);
            Assert.AreEqual(product1.Name, product1Dto.ProductName);

            var product2Dto = await productRORepository.GetByIdAsync(
                product1.Id,
                (product) => new
                {
                    ProductId = product.Id,
                    ProductType = product.Type,
                    ProductCountry = product.CountryOfOrigin,
                    ProductPrice = product.Price,
                    ProductBrand = product.Brand
                },
                default);

            Assert.IsNotNull(product2Dto);
            Assert.AreEqual(product1.Id, product2Dto.ProductId);
            Assert.AreEqual(product1.Type, product2Dto.ProductType);
            Assert.AreEqual(product1.CountryOfOrigin, product2Dto.ProductCountry);
            Assert.AreEqual(product1.Brand, product2Dto.ProductBrand);
            Assert.AreEqual(product1.Price, product2Dto.ProductPrice);
        }
    }

    [TestMethod]
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

            await productRWRepository.CreateAsync(product1, default);
            await productRWRepository.CreateAsync(product2, default);
            await productRWRepository.CreateAsync(product3, default);
            await productRWRepository.CreateAsync(product4, default);
            await productRWRepository.CreateAsync(product5, default);
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
                filter: (product) => product.Brand == "cartier");

            Assert.IsNotNull(productDtos);
            Assert.AreEqual(2, productDtos.Count);

            var product1Dto = productDtos.FirstOrDefault((product) => product.ProductId == product1.Id);
            Assert.IsNotNull(product1Dto);
            Assert.AreEqual(product1.Name, product1Dto!.ProductName);

            var product5Dto = productDtos.FirstOrDefault((product) => product.ProductId == product5.Id);
            Assert.IsNotNull(product5Dto);
            Assert.AreEqual(product5.Name, product5Dto!.ProductName);
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
                filter: (product) => product.CountryOfOrigin == "us" && product.Price > 1000M);

            Assert.IsNotNull(productDtos);
            Assert.AreEqual(2, productDtos.Count);

            var product2Dto = productDtos.FirstOrDefault((product) => product.ProductId == product2.Id);
            Assert.IsNotNull(product2Dto);
            Assert.AreEqual(product2.Type, product2Dto!.ProductType);
            Assert.AreEqual(product2.Price, product2Dto.ProductPrice);

            var product3Dto = productDtos.FirstOrDefault((product) => product.ProductId == product3.Id);
            Assert.IsNotNull(product3Dto);
            Assert.AreEqual(product3.Type, product3Dto!.ProductType);
            Assert.AreEqual(product3.Price, product3Dto.ProductPrice);
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
                sortings: new List<Sorting<Product>> { Sorting.CreateDescending<Product>(x => x.Price) })
                ).ToArray();


            Assert.IsNotNull(productDtos);
            Assert.AreEqual(3, productDtos.Length);

            var product3Dto = productDtos[0];
            Assert.IsNotNull(product3Dto);
            Assert.AreEqual(product3.Id, product3Dto.ProductId);

            var product2Dto = productDtos[1];
            Assert.IsNotNull(product2Dto);
            Assert.AreEqual(product2.Id, product2Dto.ProductId);

            var product5Dto = productDtos[2];
            Assert.IsNotNull(product5Dto);
            Assert.AreEqual(product5.Id, product5Dto.ProductId);
        }
    }
}
