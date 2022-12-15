using EnsureThat;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

internal sealed partial class Airport
{
    private Airport(string iataCode)
        : this()
    {
        EnsureArg.IsNotNullOrWhiteSpace(iataCode, nameof(iataCode));
        EnsureArg.IsTrue(iataCode.Length == 3, nameof(iataCode));

        IataCode = iataCode;
    }

    private Airport()
    {
        IataCode = string.Empty;
        RunawayCount = 1;
    }

    public string IataCode { get; private set; }
    public int RunawayCount { get; private set; }
}
