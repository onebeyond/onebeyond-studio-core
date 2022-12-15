using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;
using OneBeyond.Studio.Domain.SharedKernel.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

[TestClass]
public sealed class DesignIdeasTests : InMemoryTestsBase
{
    public DesignIdeasTests()
        : base(default)
    {
    }

    [TestMethod]
    public async Task TestIncludesWorkWithInMemoryDatabase()
    {
        var purchaseOrderId = default(Guid);

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = new PurchaseOrder();
            purchaseOrder.AddLine("First");
            purchaseOrder.AddLine("Second");

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, default);

            purchaseOrderId = purchaseOrder.Id;
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var purchaseOrderRORepository = serviceScope.ServiceProvider.GetRequiredService<IRORepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRORepository.GetByIdAsync(purchaseOrderId, default);

            Assert.AreEqual(0, purchaseOrder.Lines.Count());

            purchaseOrder = await purchaseOrderRORepository.GetByIdAsync(
                purchaseOrderId,
                Includes.Create((PurchaseOrder purchaseOrder) => purchaseOrder.Lines),
                default);

            Assert.AreEqual(2, purchaseOrder.Lines.Count());
            Assert.AreEqual(1, purchaseOrder.Lines.Count((purchaseOrderLine) => purchaseOrderLine.ItemName == "First"));
            Assert.AreEqual(1, purchaseOrder.Lines.Count((purchaseOrderLine) => purchaseOrderLine.ItemName == "Second"));
        }
    }

    [TestMethod]
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

            await airlineRWRepository.CreateAsync(baAirline, default);

            var lhAirline = new Airline.Company("LH");
            lhAirline.AccountAircrafts(120);
            lhAirlineId = lhAirline.Id;

            await airlineRWRepository.CreateAsync(lhAirline, default);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var airportRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Airport.Company, Guid>>();

            var lgwAirport = new Airport.Company("LGW");
            lgwAirportId = lgwAirport.Id;

            await airportRWRepository.CreateAsync(lgwAirport, default);

            var lhrAirport = new Airport.Company("LHR");
            lhrAirport.AccountRunaway();
            lhrAirportId = lhrAirport.Id;

            await airportRWRepository.CreateAsync(lhrAirport, default);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var companyRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Company, Guid>>();
            var surveyRWRepository = serviceScope.ServiceProvider.GetRequiredService<IRWRepository<Survey, Guid>>();

            var baAirline = await companyRWRepository.GetByIdAsync(baAirlineId, default);

            var baSurvey = new Survey("How BA did?", baAirline);

            await surveyRWRepository.CreateAsync(baSurvey, default);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var surveyRORepository = serviceScope.ServiceProvider.GetRequiredService<IRORepository<Survey, Guid>>();

            var surveys = (await surveyRORepository.ListAsync(
                    default,
                    Includes.Create((Survey survey) => survey.Company)))
                .ToList();

            Assert.AreEqual(1, surveys.Count);
            Assert.AreEqual("How BA did?", surveys[0].Name);
            Assert.IsNotNull(surveys[0].Company);
            Assert.IsInstanceOfType(surveys[0].Company, typeof(Airline.Company));
            Assert.IsNotNull(((Airline.Company)surveys[0].Company).Data);
            Assert.AreEqual("BA", ((Airline.Company)surveys[0].Company).Data.IataCode);
            Assert.AreEqual(100, ((Airline.Company)surveys[0].Company).Data.AircraftCount);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var companyRORepository = serviceScope.ServiceProvider.GetRequiredService<IRORepository<Company, Guid>>();

            var companies = (await companyRORepository.ListAsync())
                .ToDictionary((company) => company.Id);

            Assert.AreEqual(4, companies.Count);
            Assert.IsTrue(companies.ContainsKey(baAirlineId));
            Assert.IsTrue(companies.ContainsKey(lhAirlineId));
            Assert.IsTrue(companies.ContainsKey(lgwAirportId));
            Assert.IsTrue(companies.ContainsKey(lhrAirportId));
            Assert.IsInstanceOfType(companies[baAirlineId], typeof(Airline.Company));
            Assert.IsInstanceOfType(companies[lhAirlineId], typeof(Airline.Company));
            Assert.IsInstanceOfType(companies[lgwAirportId], typeof(Airport.Company));
            Assert.IsInstanceOfType(companies[lhrAirportId], typeof(Airport.Company));
            Assert.IsNotNull(((Airline.Company)companies[baAirlineId]).Data);
            Assert.IsNotNull(((Airline.Company)companies[lhAirlineId]).Data);
            Assert.IsNotNull(((Airport.Company)companies[lgwAirportId]).Data);
            Assert.IsNotNull(((Airport.Company)companies[lhrAirportId]).Data);
            Assert.AreEqual("BA", ((Airline.Company)companies[baAirlineId]).Data.IataCode);
            Assert.AreEqual(100, ((Airline.Company)companies[baAirlineId]).Data.AircraftCount);
            Assert.AreEqual("LH", ((Airline.Company)companies[lhAirlineId]).Data.IataCode);
            Assert.AreEqual(120, ((Airline.Company)companies[lhAirlineId]).Data.AircraftCount);
            Assert.AreEqual("LGW", ((Airport.Company)companies[lgwAirportId]).Data.IataCode);
            Assert.AreEqual(1, ((Airport.Company)companies[lgwAirportId]).Data.RunawayCount);
            Assert.AreEqual("LHR", ((Airport.Company)companies[lhrAirportId]).Data.IataCode);
            Assert.AreEqual(2, ((Airport.Company)companies[lhrAirportId]).Data.RunawayCount);
        }
    }
}
