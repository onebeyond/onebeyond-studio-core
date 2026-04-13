using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;
using Xunit;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

public sealed class DesignIdeasTests : InMemoryTestsBase
{
    public DesignIdeasTests()
        : base(default)
    {
    }

    [Fact]
    public async Task TestIncludesWorkWithInMemoryDatabase()
    {
        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = new PurchaseOrder();
            purchaseOrder.AddLine("First");
            purchaseOrder.AddLine("Second");

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, TestContext.Current.CancellationToken);

            purchaseOrderId = purchaseOrder.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRORepository = serviceScope.ServiceProvider.GetRequiredService<IRORepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRORepository.GetByIdAsync(purchaseOrderId, TestContext.Current.CancellationToken);

            Assert.Empty(purchaseOrder.Lines);

            purchaseOrder = await purchaseOrderRORepository.GetByIdAsync(
                purchaseOrderId,
                Includes.Create((PurchaseOrder purchaseOrder) => purchaseOrder.Lines),                TestContext.Current.CancellationToken);

            Assert.Equal(2, purchaseOrder.Lines.Count());
            Assert.Equal(1, purchaseOrder.Lines.Count((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.Equal(1, purchaseOrder.Lines.Count((purchaseOrderLine) => purchaseOrderLine.ItemName == "Second"));
        }
    }

    [Fact]
    public async Task TestTPTLikeHierarchy()
    {
        var baAirlineId = default(Guid);
        var lhAirlineId = default(Guid);

        var lgwAirportId = default(Guid);
        var lhrAirportId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var airlineRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Airline.Company, Guid>>();

            var baAirline = new Airline.Company("BA");
            baAirline.AccountAircrafts(100);
            baAirlineId = baAirline.Id;

            await airlineRWRepository.CreateAsync(baAirline, TestContext.Current.CancellationToken);

            var lhAirline = new Airline.Company("LH");
            lhAirline.AccountAircrafts(120);
            lhAirlineId = lhAirline.Id;

            await airlineRWRepository.CreateAsync(lhAirline, TestContext.Current.CancellationToken);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var airportRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Airport.Company, Guid>>();

            var lgwAirport = new Airport.Company("LGW");
            lgwAirportId = lgwAirport.Id;

            await airportRWRepository.CreateAsync(lgwAirport, TestContext.Current.CancellationToken);

            var lhrAirport = new Airport.Company("LHR");
            lhrAirport.AccountRunaway();
            lhrAirportId = lhrAirport.Id;

            await airportRWRepository.CreateAsync(lhrAirport, TestContext.Current.CancellationToken);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var companyRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Company, Guid>>();
            var surveyRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Survey, Guid>>();

            var baAirline = await companyRWRepository.GetByIdAsync(baAirlineId, TestContext.Current.CancellationToken);

            var baSurvey = new Survey("How BA did?", baAirline);

            await surveyRWRepository.CreateAsync(baSurvey, TestContext.Current.CancellationToken);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var surveyRORepository = serviceScope.ServiceProvider.GetRequiredService<IRORepository<Survey, Guid>>();

            var surveys = (await surveyRORepository.ListAsync(
                    default,
                    Includes.Create((Survey survey) => survey.Company), 
                    cancellationToken: TestContext.Current.CancellationToken))
                .ToList();

            Assert.Single(surveys);
            Assert.Equal("How BA did?", surveys[0].Name);
            Assert.NotNull(surveys[0].Company);
            Assert.IsType<Airline.Company>(surveys[0].Company);
            Assert.NotNull(((Airline.Company)surveys[0].Company).Data);
            Assert.Equal("BA", ((Airline.Company)surveys[0].Company).Data.IataCode);
            Assert.Equal(100, ((Airline.Company)surveys[0].Company).Data.AircraftCount);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var companyRORepository = serviceScope.ServiceProvider.GetRequiredService<IRORepository<Company, Guid>>();

            var companies = (await companyRORepository.ListAsync(cancellationToken: TestContext.Current.CancellationToken))
                .ToDictionary((company) => company.Id);

            Assert.Equal(4, companies.Count);
            Assert.True(companies.ContainsKey(baAirlineId));
            Assert.True(companies.ContainsKey(lhAirlineId));
            Assert.True(companies.ContainsKey(lgwAirportId));
            Assert.True(companies.ContainsKey(lhrAirportId));
            Assert.IsType<Airline.Company>(companies[baAirlineId]);
            Assert.IsType<Airline.Company>(companies[lhAirlineId]);
            Assert.IsType<Airport.Company>(companies[lgwAirportId]);
            Assert.IsType<Airport.Company>(companies[lhrAirportId]);
            Assert.NotNull(((Airline.Company)companies[baAirlineId]).Data);
            Assert.NotNull(((Airline.Company)companies[lhAirlineId]).Data);
            Assert.NotNull(((Airport.Company)companies[lgwAirportId]).Data);
            Assert.NotNull(((Airport.Company)companies[lhrAirportId]).Data);
            Assert.Equal("BA", ((Airline.Company)companies[baAirlineId]).Data.IataCode);
            Assert.Equal(100, ((Airline.Company)companies[baAirlineId]).Data.AircraftCount);
            Assert.Equal("LH", ((Airline.Company)companies[lhAirlineId]).Data.IataCode);
            Assert.Equal(120, ((Airline.Company)companies[lhAirlineId]).Data.AircraftCount);
            Assert.Equal("LGW", ((Airport.Company)companies[lgwAirportId]).Data.IataCode);
            Assert.Equal(1, ((Airport.Company)companies[lgwAirportId]).Data.RunawayCount);
            Assert.Equal("LHR", ((Airport.Company)companies[lhrAirportId]).Data.IataCode);
            Assert.Equal(2, ((Airport.Company)companies[lhrAirportId]).Data.RunawayCount);
        }
    }
}
