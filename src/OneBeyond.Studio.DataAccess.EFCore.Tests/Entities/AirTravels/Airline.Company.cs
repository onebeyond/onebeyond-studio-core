using System;
using EnsureThat;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

internal sealed partial class Airline
{
    public sealed class Company : OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels.Company
    {
        public Company(string iataCode)
            : this()
        {
            EnsureArg.IsNotNullOrWhiteSpace(iataCode, nameof(iataCode));

            Data = new Airline(iataCode);
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private Company()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        public Airline Data { get; private set; }

        public void AccountAircrafts(int aircraftCount)
        {
            EnsureArg.IsGte(aircraftCount, 0, nameof(aircraftCount));

            Data.AircraftCount += aircraftCount;
        }

        public void WriteOffAircrafts(int aircraftCount)
        {
            EnsureArg.IsGte(aircraftCount, 0, nameof(aircraftCount));

            if (aircraftCount > Data.AircraftCount)
            {
                throw new Exception("Unable to write off more aircrafts than airline has");
            }

            Data.AircraftCount -= aircraftCount;
        }
    }
}
