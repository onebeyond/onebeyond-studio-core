using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

[TestClass]
public sealed class AggregateRootTests : InMemoryTestsBase
{
    public AggregateRootTests()
        : base(default)
    {
    }

    [TestMethod]
    public async Task TestAggregateRootCreateEntity()
    {
        var vendorId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorsRWRepository = serviceScope.ServiceProvider.GetRequiredService<IAggregateRootRWRepository<Vendors, Vendor, Guid>>();

            var aggregateRoot = await vendorsRWRepository.GetAsync(x => true, default);

            var vendor = aggregateRoot.AddVendor("VendorVasya");

            await vendorsRWRepository.UpdateAsync(aggregateRoot, default);

            vendorId = vendor.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<Vendor, Guid>>();

            var vendor = await vendorRWRepository.GetByIdAsync(
                vendorId,
                default);

            Assert.AreEqual("VendorVasya", vendor.Name);
        }
    }

    [TestMethod]
    public async Task TestAggregateRootCreateMultipleEntitiesWithValidation()
    {
        var vendorVasyaId = default(Guid);
        var vendorPetyaId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorsRWRepository = serviceScope.ServiceProvider.GetRequiredService<IAggregateRootRWRepository<Vendors, Vendor, Guid>>();

            var aggregateRoot = await vendorsRWRepository.GetAsync(x => true, default);

            var vendorVasya = aggregateRoot.AddVendor("VendorVasya");

            Assert.ThrowsException<ValidationException>(() => aggregateRoot.AddVendor("VendorVasya"));

            var vendorPetya = aggregateRoot.AddVendor("VendorPetya");

            await vendorsRWRepository.UpdateAsync(aggregateRoot, default);

            vendorVasyaId = vendorVasya.Id;
            vendorPetyaId = vendorPetya.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRORepository<Vendor, Guid>>();

            var vendorV = await vendorRWRepository.GetByIdAsync(
                vendorVasyaId,
                default);

            Assert.AreEqual("VendorVasya", vendorV.Name);

            var vendorP = await vendorRWRepository.GetByIdAsync(
                vendorPetyaId,
                default);

            Assert.AreEqual("VendorPetya", vendorP.Name);
        }
    }

    [TestMethod]
    public async Task TestAggregateRootUpdateMultipleEntitiesWithValidation()
    {
        var vendorVasyaId = default(Guid);
        var vendorPetyaId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorsRWRepository = serviceScope.ServiceProvider.GetRequiredService<IAggregateRootRWRepository<Vendors, Vendor, Guid>>();

            var aggregateRoot = await vendorsRWRepository.GetAsync(x => true, default);

            var vendorVasya = aggregateRoot.AddVendor("VendorVasya");
            var vendorPetya = aggregateRoot.AddVendor("VendorPetya");

            await vendorsRWRepository.UpdateAsync(aggregateRoot, default);

            vendorVasyaId = vendorVasya.Id;
            vendorPetyaId = vendorPetya.Id;

            var updateAggregateRoot = await vendorsRWRepository.GetAsync(x => true, default);

            Assert.ThrowsException<ValidationException>(() => updateAggregateRoot.UpdateVendor(vendorPetyaId, "VendorVasya"));

            updateAggregateRoot.UpdateVendor(vendorVasyaId, "SuperVendorVasya");
            updateAggregateRoot.UpdateVendor(vendorPetyaId, "SuperVendorPetya");

            await vendorsRWRepository.UpdateAsync(aggregateRoot, default);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRORepository<Vendor, Guid>>();

            var vendorV = await vendorRWRepository.GetByIdAsync(
                vendorVasyaId,
                default);

            Assert.AreEqual("SuperVendorVasya", vendorV.Name);

            var vendorP = await vendorRWRepository.GetByIdAsync(
                vendorPetyaId,
                default);

            Assert.AreEqual("SuperVendorPetya", vendorP.Name);
        }
    }
}
