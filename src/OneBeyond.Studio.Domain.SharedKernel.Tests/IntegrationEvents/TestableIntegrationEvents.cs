using System;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.IntegrationEvents;

internal static class TestableIntegrationEvents
{
    public const string ThisHappenedTypeName = "Domain1.ThisHappened";
    public const string ThatHappenedTypeName = "Domain2.ThatHappened";

    [IntegrationEventType(ThisHappenedTypeName)]
    [IntegrationEventVersion(1, 1)]
    public class ThisHappened_1_1 : IntegrationEvent
    {
        public ThisHappened_1_1(int intValue, DateTimeOffset raisedAt)
            : base(raisedAt)
        {
            IntValue = intValue;
        }

        public int IntValue { get; }
    }

    [IntegrationEventVersion(1, 2)]
    public class ThisHappened_1_2 : ThisHappened_1_1
    {
        public ThisHappened_1_2(string stringValue, int intValue, DateTimeOffset raisedAt)
            : base(intValue, raisedAt)
        {
            StringValue = stringValue;
        }

        public string StringValue { get; }
    }

    [IntegrationEventVersion(1, 4)]
    public class ThisHappened_1_4 : ThisHappened_1_2
    {
        public ThisHappened_1_4(bool boolValue, string stringValue, int intValue, DateTimeOffset raisedAt)
            : base(stringValue, intValue, raisedAt)
        {
            BoolValue = boolValue;
        }

        public bool BoolValue { get; }
    }

    [IntegrationEventType(ThisHappenedTypeName)]
    [IntegrationEventVersion(2, 1)]
    public class ThisHappened_2_1 : IntegrationEvent
    {
        public ThisHappened_2_1(DateTime dateTimeValue, DateTimeOffset raisedAt)
            : base(raisedAt)
        {
            DateTimeValue = dateTimeValue;
        }

        public DateTime DateTimeValue { get; }
    }

    [IntegrationEventType(ThatHappenedTypeName)]
    [IntegrationEventVersion(1, 0)]
    public class ThatHappened_1_0 : IntegrationEvent
    {
        public ThatHappened_1_0(string stringValue, DateTimeOffset raisedAt)
            : base(raisedAt)
        {
            StringValue = stringValue;
        }

        public string StringValue { get; }
    }


    [IntegrationEventType(ThisHappenedTypeName)]
    [IntegrationEventVersion(1, 0)]
    public class FakeHappened_1_0
    {
    }

    [IntegrationEventType(ThatHappenedTypeName)]
    public class FakeHappened_1_1 : FakeHappened_1_0
    {
    }

    public class FakeHappened_1_2 : FakeHappened_1_1
    {
    }
}
