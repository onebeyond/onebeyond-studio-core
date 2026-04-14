using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;
using Xunit;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

public sealed class AggregateRootTests : InMemoryTestsBase
{
    public AggregateRootTests()
        : base(default)
    {
    }

    [Fact]
    public async Task TestAggregateRootCreateEntity()
    {
        var vendorId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorsRWRepository = serviceScope.ServiceProvider.GetRequiredService<IAggregateRootRWRepository<Vendors, Vendor, Guid>>();

            var aggregateRoot = await vendorsRWRepository.GetAsync(x => true, TestContext.Current.CancellationToken);

            var vendor = aggregateRoot.AddVendor("VendorVasya");

            await vendorsRWRepository.UpdateAsync(aggregateRoot, TestContext.Current.CancellationToken);

            vendorId = vendor.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<Vendor, Guid>>();

            var vendor = await vendorRWRepository.GetByIdAsync(
                vendorId,                TestContext.Current.CancellationToken);

            Assert.Equal("VendorVasya", vendor.Name);
        }
    }

    [Fact]
    public async Task TestAggregateRootCreateMultipleEntitiesWithValidation()
    {
        var vendorVasyaId = default(Guid);
        var vendorPetyaId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorsRWRepository = serviceScope.ServiceProvider.GetRequiredService<IAggregateRootRWRepository<Vendors, Vendor, Guid>>();

            var aggregateRoot = await vendorsRWRepository.GetAsync(x => true, TestContext.Current.CancellationToken);

            var vendorVasya = aggregateRoot.AddVendor("VendorVasya");

            Assert.Throws<ValidationException>(() => aggregateRoot.AddVendor("VendorVasya"));

            var vendorPetya = aggregateRoot.AddVendor("VendorPetya");

            await vendorsRWRepository.UpdateAsync(aggregateRoot, TestContext.Current.CancellationToken);

            vendorVasyaId = vendorVasya.Id;
            vendorPetyaId = vendorPetya.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRORepository<Vendor, Guid>>();

            var vendorV = await vendorRWRepository.GetByIdAsync(
                vendorVasyaId,                TestContext.Current.CancellationToken);

            Assert.Equal("VendorVasya", vendorV.Name);

            var vendorP = await vendorRWRepository.GetByIdAsync(
                vendorPetyaId,                TestContext.Current.CancellationToken);

            Assert.Equal("VendorPetya", vendorP.Name);
        }
    }

    [Fact]
    public async Task TestAggregateRootUpdateMultipleEntitiesWithValidation()
    {
        var vendorVasyaId = default(Guid);
        var vendorPetyaId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorsRWRepository = serviceScope.ServiceProvider.GetRequiredService<IAggregateRootRWRepository<Vendors, Vendor, Guid>>();

            var aggregateRoot = await vendorsRWRepository.GetAsync(x => true, TestContext.Current.CancellationToken);

            var vendorVasya = aggregateRoot.AddVendor("VendorVasya");
            var vendorPetya = aggregateRoot.AddVendor("VendorPetya");

            await vendorsRWRepository.UpdateAsync(aggregateRoot, TestContext.Current.CancellationToken);

            vendorVasyaId = vendorVasya.Id;
            vendorPetyaId = vendorPetya.Id;

            var updateAggregateRoot = await vendorsRWRepository.GetAsync(x => true, TestContext.Current.CancellationToken);

            Assert.Throws<ValidationException>(() => updateAggregateRoot.UpdateVendor(vendorPetyaId, "VendorVasya"));

            updateAggregateRoot.UpdateVendor(vendorVasyaId, "SuperVendorVasya");
            updateAggregateRoot.UpdateVendor(vendorPetyaId, "SuperVendorPetya");

            await vendorsRWRepository.UpdateAsync(updateAggregateRoot, TestContext.Current.CancellationToken);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRORepository<Vendor, Guid>>();

            var vendorV = await vendorRWRepository.GetByIdAsync(
                vendorVasyaId,                TestContext.Current.CancellationToken);

            Assert.Equal("SuperVendorVasya", vendorV.Name);

            var vendorP = await vendorRWRepository.GetByIdAsync(
                vendorPetyaId,                TestContext.Current.CancellationToken);

            Assert.Equal("SuperVendorPetya", vendorP.Name);
        }
    }
}
