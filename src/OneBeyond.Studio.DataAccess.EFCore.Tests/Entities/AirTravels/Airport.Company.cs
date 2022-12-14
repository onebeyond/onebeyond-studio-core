using EnsureThat;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

internal sealed partial class Airport
{
    public sealed class Company : OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels.Company
    {
        public Company(string iataCode)
            : this()
        {
            EnsureArg.IsNotNullOrWhiteSpace(iataCode, nameof(iataCode));

            Data = new Airport(iataCode);
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private Company()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        public Airport Data { get; private set; }

        public void AccountRunaway()
        {
            Data.RunawayCount++;
        }
    }
}
