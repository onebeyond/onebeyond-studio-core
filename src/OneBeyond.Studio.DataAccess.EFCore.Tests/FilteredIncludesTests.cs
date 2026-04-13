using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.DataAccess.EFCore.Repositories;
using OneBeyond.Studio.DataAccess.EFCore.Tests.DbContexts;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;
using Xunit;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

public sealed class FilteredIncludesTests : InMemoryTestsBase
{
    public FilteredIncludesTests()
        : base(default)
    {
    }

    [Fact]
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

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, TestContext.Current.CancellationToken);

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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(3, purchaseOrder.Lines.Count());
            Assert.Equal(12, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(purchaseOrderId, TestContext.Current.CancellationToken);

            Assert.Empty(purchaseOrder.Lines);
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.Empty(purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments));
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.Equal(8, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            var purchaseOrderLineComments = purchaseOrder.Lines
                .SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments)
                .ToArray();
            Assert.Equal(4, purchaseOrderLineComments.Count());
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "1.1");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "1.2");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "3.1");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "3.2");
        }
    }

    [Fact]
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

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, TestContext.Current.CancellationToken);

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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(3, purchaseOrder.Lines.Count());
            Assert.Equal(12, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.Equal(4, purchaseOrder.Tags.Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(purchaseOrderId, TestContext.Current.CancellationToken);

            Assert.Empty(purchaseOrder.Lines);
            Assert.Empty(purchaseOrder.Tags);
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.Empty(purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments));
            Assert.Equal(4, purchaseOrder.Tags.Count());
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.Equal(8, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.Equal(2, purchaseOrder.Tags.Count());
            Assert.Contains(purchaseOrder.Tags, purchaseOrderTag => purchaseOrderTag.Description == "Tag.1.A");
            Assert.Contains(purchaseOrder.Tags, purchaseOrderTag => purchaseOrderTag.Description == "Tag.2.A");
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            var purchaseOrderLineComments = purchaseOrder.Lines
                .SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments)
                .ToArray();
            Assert.Equal(4, purchaseOrderLineComments.Count());
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "1.1");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "1.2");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "3.1");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "3.2");
            Assert.Equal(2, purchaseOrder.Tags.Count());
            Assert.Contains(purchaseOrder.Tags, purchaseOrderTag => purchaseOrderTag.Description == "Tag.1.B");
            Assert.Contains(purchaseOrder.Tags, purchaseOrderTag => purchaseOrderTag.Description == "Tag.2.B");
        }
    }

    [Fact]
    public async Task TestFilteredIncludeWithSingleEntityPathLoadsOnlyRequestedChildren()
    {
        var vendorId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Vendor, Guid>>();

            var vendor = new Vendor("Vendor.1");

            await vendorRWRepository.CreateAsync(vendor, TestContext.Current.CancellationToken);

            vendorId = vendor.Id;
        }

        var accountId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var accountRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Account, Guid>>();

            var account = new Account("Account.1");

            await accountRWRepository.CreateAsync(account, TestContext.Current.CancellationToken);

            accountId = account.Id;
        }

        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Vendor, Guid>>();
            var accountRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Account, Guid>>();
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var vendor = await vendorRWRepository.GetByIdAsync(vendorId, TestContext.Current.CancellationToken);

            var account = await accountRWRepository.GetByIdAsync(accountId, TestContext.Current.CancellationToken);

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

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, TestContext.Current.CancellationToken);

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
                includes,                TestContext.Current.CancellationToken);

            Assert.Null(purchaseOrder.Vendor);
            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.True(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is null));
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Null(purchaseOrder.Vendor);
            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.True(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is not null));
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
                includes,                TestContext.Current.CancellationToken);

            Assert.NotNull(purchaseOrder.Vendor);
            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.True(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is not null));
        }
    }

    [Fact]
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

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, TestContext.Current.CancellationToken);

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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(3, purchaseOrder.Lines.Count());
            Assert.Equal(12, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(purchaseOrderId, TestContext.Current.CancellationToken);

            Assert.Empty(purchaseOrder.Lines);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var includes = new Includes<PurchaseOrder>()
                .Include((purchaseOrder) => purchaseOrder.Lines.Where((purchaseOrderLine) => purchaseOrderLine.ItemName.Contains('i')));

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.Empty(purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments));
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.Equal(8, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            var purchaseOrderLineComments = purchaseOrder.Lines
                .SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments)
                .ToArray();
            Assert.Equal(4, purchaseOrderLineComments.Count());
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "1.1");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "1.2");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "3.1");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "3.2");
        }
    }

    [Fact]
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

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, TestContext.Current.CancellationToken);

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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(3, purchaseOrder.Lines.Count());
            Assert.Equal(12, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.Equal(4, purchaseOrder.Tags.Count());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider
                .GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(purchaseOrderId, TestContext.Current.CancellationToken);

            Assert.Empty(purchaseOrder.Lines);
            Assert.Empty(purchaseOrder.Tags);
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.Empty(purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments));
            Assert.Equal(4, purchaseOrder.Tags.Count());
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.Equal(8, purchaseOrder.Lines.SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments).Count());
            Assert.Equal(2, purchaseOrder.Tags.Count());
            Assert.Contains(purchaseOrder.Tags, purchaseOrderTag => purchaseOrderTag.Description == "Tag.1.A");
            Assert.Contains(purchaseOrder.Tags, purchaseOrderTag => purchaseOrderTag.Description == "Tag.2.A");
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            var purchaseOrderLineComments = purchaseOrder.Lines
                .SelectMany((purchaseOrderLine) => purchaseOrderLine.Comments)
                .ToArray();
            Assert.Equal(4, purchaseOrderLineComments.Count());
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "1.1");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "1.2");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "3.1");
            Assert.Contains(purchaseOrderLineComments, purchaseOrderLineComment => purchaseOrderLineComment.Text == "3.2");
            Assert.Equal(2, purchaseOrder.Tags.Count());
            Assert.Contains(purchaseOrder.Tags, purchaseOrderTag => purchaseOrderTag.Description == "Tag.1.B");
            Assert.Contains(purchaseOrder.Tags, purchaseOrderTag => purchaseOrderTag.Description == "Tag.2.B");
        }
    }

    [Fact]
    public async Task TestEFStyleFilteredIncludeWithSingleEntityPathLoadsOnlyRequestedChildren()
    {
        var vendorId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Vendor, Guid>>();

            var vendor = new Vendor("Vendor.1");

            await vendorRWRepository.CreateAsync(vendor, TestContext.Current.CancellationToken);

            vendorId = vendor.Id;
        }

        var accountId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var accountRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Account, Guid>>();

            var account = new Account("Account.1");

            await accountRWRepository.CreateAsync(account, TestContext.Current.CancellationToken);

            accountId = account.Id;
        }

        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var vendorRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Vendor, Guid>>();
            var accountRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Account, Guid>>();
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var vendor = await vendorRWRepository.GetByIdAsync(vendorId, TestContext.Current.CancellationToken);

            var account = await accountRWRepository.GetByIdAsync(accountId, TestContext.Current.CancellationToken);

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

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, TestContext.Current.CancellationToken);

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
                includes,                TestContext.Current.CancellationToken);

            Assert.Null(purchaseOrder.Vendor);
            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.True(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is null));
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
                includes,                TestContext.Current.CancellationToken);

            Assert.Null(purchaseOrder.Vendor);
            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.True(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is not null));
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
                includes,                TestContext.Current.CancellationToken);

            Assert.NotNull(purchaseOrder.Vendor);
            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "First");
            Assert.Contains(purchaseOrder.Lines, purchaseOrderLine => purchaseOrderLine.ItemName == "Third");
            Assert.True(purchaseOrder.Lines.All((purchaseOrderLine) => purchaseOrderLine.Account is not null));
        }
    }

    [Fact]
    public void TestIncludesTraitsWhereClauseDetection()
    {
        var includes = new Includes<PurchaseOrder>()
            .Include((purchaseOrder) => purchaseOrder.Lines)
                .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments) as Includes<PurchaseOrder>;

        var includesTraits = new IncludesTraits<PurchaseOrder>();
        includesTraits = includes.Replay(includesTraits);

        Assert.False(includesTraits.HaveWhereClause);

        includes = new Includes<PurchaseOrder>()
            .Include((purchaseOrder) => purchaseOrder.Lines)
            .Where((purchaseOrderLine) => purchaseOrderLine.ItemName == "dd")
                .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments);

        includesTraits = new IncludesTraits<PurchaseOrder>();
        includesTraits = includes.Replay(includesTraits);

        Assert.True(includesTraits.HaveWhereClause);

        includes = new Includes<PurchaseOrder>()
            .Include((purchaseOrder) => purchaseOrder.Lines)
                .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                .Where((purchaseOrderLineComment) => purchaseOrderLineComment.CreatedAt > DateTimeOffset.Now);

        includesTraits = new IncludesTraits<PurchaseOrder>();
        includesTraits = includes.Replay(includesTraits);

        Assert.True(includesTraits.HaveWhereClause);

        includes = new Includes<PurchaseOrder>()
            .Include((purchaseOrder) => purchaseOrder.Lines)
            .Where((purchaseOrderLine) => purchaseOrderLine.ItemName == "dd")
                .ThenInclude((purchaseOrderLine) => purchaseOrderLine.Comments)
                .Where((purchaseOrderLineComment) => purchaseOrderLineComment.CreatedAt > DateTimeOffset.Now);

        includesTraits = new IncludesTraits<PurchaseOrder>();
        includesTraits = includes.Replay(includesTraits);

        Assert.True(includesTraits.HaveWhereClause);
    }
}

