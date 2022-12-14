using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.DataAccess.EFCore.Repositories;
using OneBeyond.Studio.DataAccess.EFCore.Tests.DbContexts;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;
using OneBeyond.Studio.Domain.SharedKernel.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

[TestClass]
public sealed class FilteredIncludesTests : InMemoryTestsBase
{
    public FilteredIncludesTests()
        : base(default)
    {
    }

    [TestMethod]
    public async Task TestFilteredIncludeLoadsOnlyRequestedChildren()
    {
        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = new PurchaseOrder();
            var purchaseOrderLine = purchaseOrder.AddLine("First");
            purchaseOrderLine.AddComment("1.1");
            purchaseOrderLine.AddComment("1.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine = purchaseOrder.AddLine("Second");
            purchaseOrderLine.AddComment("2.1");
            purchaseOrderLine.AddComment("2.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine = purchaseOrder.AddLine("Third");
            purchaseOrderLine.AddComment("3.1");
            purchaseOrderLine.AddComment("3.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, default);

            purchaseOrderId = purchaseOrder.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(3, purchaseOrder.Lines.Count());
            Assert.AreEqual(12, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(purchaseOrderId, default);

            Assert.AreEqual(0, purchaseOrder.Lines.Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.AreEqual(0, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.AreEqual(8, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            //var dbContext = serviceScope.ServiceProvider
            //    .GetRequiredService<DbContexts.DbContext>();

            //var purchaseOrdersQuery = dbContext.Set<PurchaseOrder>();

            //var purchaseOrders = purchaseOrdersQuery
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
            //    //.IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')).SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments.Where((purchaseOrderLineComment) => !purchaseOrderLineComment.IsArchived)))
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')).SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments.Where((purchaseOrderLineComment) => purchaseOrderLineComment.Text.Contains('.'))))
            //    .ToList();

            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                    //.Where((purchaseOrderLineComment) => !purchaseOrderLineComment.IsArchived);
                    .Where((purchaseOrderLineComment) => purchaseOrderLineComment.Text.Contains('.'));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            var purchaseOrderLineComments = purchaseOrder.Lines
                .SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments)
                .ToArray();
            Assert.AreEqual(4, purchaseOrderLineComments.Length);
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "1.1"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "1.2"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "3.1"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "3.2"));
        }
    }

    [TestMethod]
    public async Task TestFilteredIncludeWithMultiplePathsLoadsOnlyRequestedChildren()
    {
        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = new PurchaseOrder();

            var purchaseOrderLine = purchaseOrder.AddLine("First");
            purchaseOrderLine.AddComment("1.1");
            purchaseOrderLine.AddComment("1.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrderLine = purchaseOrder.AddLine("Second");
            purchaseOrderLine.AddComment("2.1");
            purchaseOrderLine.AddComment("2.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrderLine = purchaseOrder.AddLine("Third");
            purchaseOrderLine.AddComment("3.1");
            purchaseOrderLine.AddComment("3.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrder.AddTag("Tag.1.A");
            purchaseOrder.AddTag("Tag.1.B");
            purchaseOrder.AddTag("Tag.2.A");
            purchaseOrder.AddTag("Tag.2.B");

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, default);

            purchaseOrderId = purchaseOrder.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                .Include((purchaseOrder) => purchaseOrder.Tags);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(3, purchaseOrder.Lines.Count());
            Assert.AreEqual(12, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.AreEqual(4, purchaseOrder.Tags.Count);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(purchaseOrderId, default);

            Assert.AreEqual(0, purchaseOrder.Lines.Count());
            Assert.AreEqual(0, purchaseOrder.Tags.Count);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var dbContext = serviceScope.ServiceProvider
                .GetRequiredService<DbContext>();

            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'))
                .Include((purchaseOrder) => purchaseOrder.Tags);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.AreEqual(0, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.AreEqual(4, purchaseOrder.Tags.Count);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                .Include((purchaseOrder) => purchaseOrder.Tags)
                .Where((purchaseOrderTag) => purchaseOrderTag.Description.Contains(".A"));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.AreEqual(8, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.AreEqual(2, purchaseOrder.Tags.Count);
            Assert.IsTrue(purchaseOrder.Tags.Any((purchaseOrderTag) => purchaseOrderTag.Description == "Tag.1.A"));
            Assert.IsTrue(purchaseOrder.Tags.Any((purchaseOrderTag) => purchaseOrderTag.Description == "Tag.2.A"));
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            //var dbContext = serviceScope.ServiceProvider
            //    .GetRequiredService<DbContexts.DbContext>();

            //var purchaseOrdersQuery = dbContext.Set<PurchaseOrder>();

            //var purchaseOrders = purchaseOrdersQuery
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
            //    //.IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')).SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments.Where((purchaseOrderLineComment) => !purchaseOrderLineComment.IsArchived)))
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')).SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments.Where((purchaseOrderLineComment) => purchaseOrderLineComment.Text.Contains('.'))))
            //    .ToList();

            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                    //.Where((purchaseOrderLineComment) => !purchaseOrderLineComment.IsArchived);
                    .Where((purchaseOrderLineComment) => purchaseOrderLineComment.Text.Contains('.'))
                .Include((purchaseOrder) => purchaseOrder.Tags)
                .Where((purchaseOrderTag) => purchaseOrderTag.Description.Contains(".B"));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            var purchaseOrderLineComments = purchaseOrder.Lines
                .SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments)
                .ToArray();
            Assert.AreEqual(4, purchaseOrderLineComments.Length);
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "1.1"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "1.2"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "3.1"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "3.2"));
            Assert.AreEqual(2, purchaseOrder.Tags.Count);
            Assert.IsTrue(purchaseOrder.Tags.Any((purchaseOrderTag) => purchaseOrderTag.Description == "Tag.1.B"));
            Assert.IsTrue(purchaseOrder.Tags.Any((purchaseOrderTag) => purchaseOrderTag.Description == "Tag.2.B"));
        }
    }

    [TestMethod]
    public async Task TestFilteredIncludeWithSingleEntityPathLoadsOnlyRequestedChildren()
    {
        var vendorId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Vendor, Guid>>();

            var vendor = new Vendor("Vendor.1");

            await vendorRWRepository.CreateAsync(vendor, default);

            vendorId = vendor.Id;
        }

        var accountId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var accountRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Account, Guid>>();

            var account = new Account("Account.1");

            await accountRWRepository.CreateAsync(account, default);

            accountId = account.Id;
        }

        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Vendor, Guid>>();
            var accountRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Account, Guid>>();
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var vendor = await vendorRWRepository.GetByIdAsync(vendorId, default);

            var account = await accountRWRepository.GetByIdAsync(accountId, default);

            var purchaseOrder = new PurchaseOrder();

            purchaseOrder.SetVendor(vendor);

            var purchaseOrderLine = purchaseOrder.AddLine("First");
            purchaseOrderLine.SetAccount(account);
            purchaseOrderLine.AddComment("1.1");
            purchaseOrderLine.AddComment("1.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrderLine = purchaseOrder.AddLine("Second");
            purchaseOrderLine.SetAccount(account);
            purchaseOrderLine.AddComment("2.1");
            purchaseOrderLine.AddComment("2.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrderLine = purchaseOrder.AddLine("Third");
            purchaseOrderLine.SetAccount(account);
            purchaseOrderLine.AddComment("3.1");
            purchaseOrderLine.AddComment("3.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, default);

            purchaseOrderId = purchaseOrder.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.IsNull(purchaseOrder.Vendor);
            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.IsTrue(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is null));
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            //var dbContext = serviceScope.ServiceProvider
            //    .GetRequiredService<DbContexts.DbContext>();

            //var purchaseOrdersQuery = dbContext.Set<PurchaseOrder>();

            //var purchaseOrders = purchaseOrdersQuery
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')).Select((purchaseOrderLine) => purchaseOrderLine.Account))
            //    .ToList();

            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Account!);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.IsNull(purchaseOrder.Vendor);
            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.IsTrue(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is not null));
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            //var dbContext = serviceScope.ServiceProvider
            //    .GetRequiredService<DbContexts.DbContext>();

            //var purchaseOrdersQuery = dbContext.Set<PurchaseOrder>();

            //var purchaseOrders = purchaseOrdersQuery
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')).Select((purchaseOrderLine) => purchaseOrderLine.Account))
            //    .IncludeFilter((purchaseOrder) => purchaseOrder.Vendor)
            //    .ToList();

            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>(true)
                .Include((purchaseOrder) => purchaseOrder.Lines)
                .Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i'))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Account!)
                .Include((purchaseOrder) => purchaseOrder.Vendor!);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.IsNotNull(purchaseOrder.Vendor);
            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.IsTrue(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is not null));
        }
    }

    [TestMethod]
    public async Task TestEFStyleFilteredIncludeLoadsOnlyRequestedChildren()
    {
        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = new PurchaseOrder();
            var purchaseOrderLine = purchaseOrder.AddLine("First");
            purchaseOrderLine.AddComment("1.1");
            purchaseOrderLine.AddComment("1.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine = purchaseOrder.AddLine("Second");
            purchaseOrderLine.AddComment("2.1");
            purchaseOrderLine.AddComment("2.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine = purchaseOrder.AddLine("Third");
            purchaseOrderLine.AddComment("3.1");
            purchaseOrderLine.AddComment("3.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, default);

            purchaseOrderId = purchaseOrder.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(3, purchaseOrder.Lines.Count());
            Assert.AreEqual(12, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(purchaseOrderId, default);

            Assert.AreEqual(0, purchaseOrder.Lines.Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.AreEqual(0, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.AreEqual(8, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments.Where((purchaseOrderLineComment) => purchaseOrderLineComment.Text.Contains('.')));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            var purchaseOrderLineComments = purchaseOrder.Lines
                .SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments)
                .ToArray();
            Assert.AreEqual(4, purchaseOrderLineComments.Length);
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "1.1"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "1.2"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "3.1"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "3.2"));
        }
    }

    [TestMethod]
    public async Task TestEFStyleFilteredIncludeWithMultiplePathsLoadsOnlyRequestedChildren()
    {
        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = new PurchaseOrder();

            var purchaseOrderLine = purchaseOrder.AddLine("First");
            purchaseOrderLine.AddComment("1.1");
            purchaseOrderLine.AddComment("1.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrderLine = purchaseOrder.AddLine("Second");
            purchaseOrderLine.AddComment("2.1");
            purchaseOrderLine.AddComment("2.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrderLine = purchaseOrder.AddLine("Third");
            purchaseOrderLine.AddComment("3.1");
            purchaseOrderLine.AddComment("3.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrder.AddTag("Tag.1.A");
            purchaseOrder.AddTag("Tag.1.B");
            purchaseOrder.AddTag("Tag.2.A");
            purchaseOrder.AddTag("Tag.2.B");

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, default);

            purchaseOrderId = purchaseOrder.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines)
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                .Include((purchaseOrder) => purchaseOrder.Tags);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(3, purchaseOrder.Lines.Count());
            Assert.AreEqual(12, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.AreEqual(4, purchaseOrder.Tags.Count);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(purchaseOrderId, default);

            Assert.AreEqual(0, purchaseOrder.Lines.Count());
            Assert.AreEqual(0, purchaseOrder.Tags.Count);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var dbContext = serviceScope.ServiceProvider
                .GetRequiredService<DbContext>();

            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
                .Include((purchaseOrder) => purchaseOrder.Tags);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.AreEqual(0, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.AreEqual(4, purchaseOrder.Tags.Count);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                .Include((purchaseOrder) => purchaseOrder.Tags.Where((purchaseOrderTag) => purchaseOrderTag.Description.Contains(".A")));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.AreEqual(8, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.AreEqual(2, purchaseOrder.Tags.Count);
            Assert.IsTrue(purchaseOrder.Tags.Any((purchaseOrderTag) => purchaseOrderTag.Description == "Tag.1.A"));
            Assert.IsTrue(purchaseOrder.Tags.Any((purchaseOrderTag) => purchaseOrderTag.Description == "Tag.2.A"));
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments.Where((purchaseOrderLineComment) => purchaseOrderLineComment.Text.Contains('.')))
                .Include((purchaseOrder) => purchaseOrder.Tags.Where((purchaseOrderTag) => purchaseOrderTag.Description.Contains(".B")));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            var purchaseOrderLineComments = purchaseOrder.Lines
                .SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments)
                .ToArray();
            Assert.AreEqual(4, purchaseOrderLineComments.Length);
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "1.1"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "1.2"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "3.1"));
            Assert.IsTrue(purchaseOrderLineComments.Any((purchaseOrderLineComment) => purchaseOrderLineComment.Text == "3.2"));
            Assert.AreEqual(2, purchaseOrder.Tags.Count);
            Assert.IsTrue(purchaseOrder.Tags.Any((purchaseOrderTag) => purchaseOrderTag.Description == "Tag.1.B"));
            Assert.IsTrue(purchaseOrder.Tags.Any((purchaseOrderTag) => purchaseOrderTag.Description == "Tag.2.B"));
        }
    }

    [TestMethod]
    public async Task TestEFStyleFilteredIncludeWithSingleEntityPathLoadsOnlyRequestedChildren()
    {
        var vendorId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Vendor, Guid>>();

            var vendor = new Vendor("Vendor.1");

            await vendorRWRepository.CreateAsync(vendor, default);

            vendorId = vendor.Id;
        }

        var accountId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var accountRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Account, Guid>>();

            var account = new Account("Account.1");

            await accountRWRepository.CreateAsync(account, default);

            accountId = account.Id;
        }

        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Vendor, Guid>>();
            var accountRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Account, Guid>>();
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var vendor = await vendorRWRepository.GetByIdAsync(vendorId, default);

            var account = await accountRWRepository.GetByIdAsync(accountId, default);

            var purchaseOrder = new PurchaseOrder();

            purchaseOrder.SetVendor(vendor);

            var purchaseOrderLine = purchaseOrder.AddLine("First");
            purchaseOrderLine.SetAccount(account);
            purchaseOrderLine.AddComment("1.1");
            purchaseOrderLine.AddComment("1.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrderLine = purchaseOrder.AddLine("Second");
            purchaseOrderLine.SetAccount(account);
            purchaseOrderLine.AddComment("2.1");
            purchaseOrderLine.AddComment("2.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            purchaseOrderLine = purchaseOrder.AddLine("Third");
            purchaseOrderLine.SetAccount(account);
            purchaseOrderLine.AddComment("3.1");
            purchaseOrderLine.AddComment("3.2");
            purchaseOrderLine.AddComment("___").Archive();
            purchaseOrderLine.AddComment("___").Archive();

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, default);

            purchaseOrderId = purchaseOrder.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.IsNull(purchaseOrder.Vendor);
            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.IsTrue(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is null));
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Account!);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.IsNull(purchaseOrder.Vendor);
            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.IsTrue(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is not null));
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>(true)
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')))
                    .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Account!)
                .Include((purchaseOrder) => purchaseOrder.Vendor!);

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,
                default);

            Assert.IsNotNull(purchaseOrder.Vendor);
            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.IsTrue(purchaseOrder.Lines.Any((purchaseOrderLine) => purchaseOrderLine.ItemName == "Third"));
            Assert.IsTrue(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is not null));
        }
    }

    [TestMethod]
    public void TestIncludesTraitsWhereClauseDetection()
    {
        var includes = new Includes<PurchaseOrder>()
            .Include((purchaseOrder) => purchaseOrder.Lines)
                .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments) as Includes<PurchaseOrder>;

        var includesTraits = new IncludesTraits<PurchaseOrder>();
        includesTraits = includes.Replay(includesTraits);

        Assert.IsFalse(includesTraits.HaveWhereClause);

        includes = new Includes<PurchaseOrder>()
            .Include((purchaseOrder) => purchaseOrder.Lines)
            .Where((purchaseOrderLine) => purchaseOrderLine.ItemName == "dd")
                .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments);

        includesTraits = new IncludesTraits<PurchaseOrder>();
        includesTraits = includes.Replay(includesTraits);

        Assert.IsTrue(includesTraits.HaveWhereClause);

        includes = new Includes<PurchaseOrder>()
            .Include((purchaseOrder) => purchaseOrder.Lines)
                .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                .Where((purchaseOrderLineComment) => purchaseOrderLineComment.CreatedAt > DateTimeOffset.Now);

        includesTraits = new IncludesTraits<PurchaseOrder>();
        includesTraits = includes.Replay(includesTraits);

        Assert.IsTrue(includesTraits.HaveWhereClause);

        includes = new Includes<PurchaseOrder>()
            .Include((purchaseOrder) => purchaseOrder.Lines)
            .Where((purchaseOrderLine) => purchaseOrderLine.ItemName == "dd")
                .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                .Where((purchaseOrderLineComment) => purchaseOrderLineComment.CreatedAt > DateTimeOffset.Now);

        includesTraits = new IncludesTraits<PurchaseOrder>();
        includesTraits = includes.Replay(includesTraits);

        Assert.IsTrue(includesTraits.HaveWhereClause);
    }
}
