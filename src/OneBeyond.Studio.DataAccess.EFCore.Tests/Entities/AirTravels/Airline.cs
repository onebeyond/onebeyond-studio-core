using EnsureThat;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

internal sealed partial class Airline
{
    private Airline(string iataCode)
        : this()
    {
        EnsureArg.IsNotNullOrWhiteSpace(iataCode, nameof(iataCode));
        EnsureArg.IsTrue(iataCode.Length == 2, nameof(iataCode));

        IataCode = iataCode;
    }

    private Airline()
    {
        IataCode = string.Empty;
        AircraftCount = 0;
    }

    public string IataCode { get; private set; }
    public int AircraftCount { get; private set; }
}
