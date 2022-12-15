using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Ardalis.SmartEnum;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;
using Xunit;

#nullable disable

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.Specifications;

public sealed class FilterExpressionBuilderTests : IClassFixture<LogManagerFixture>
{
    private enum TestEnum
    {
        TestValue = 1
    }

    private sealed class TestSmartEnum : SmartEnum<TestSmartEnum>
    {
        public static readonly TestSmartEnum One = new TestSmartEnum(1, "First");
        public static readonly TestSmartEnum Two = new TestSmartEnum(2, "Second");

        private TestSmartEnum(int value, string name)
            : base(name, value)
        {
        }
    }

    private sealed class TestModel
    {
        public string Name { get; set; }
        public int Int { get; set; }
        public int? NullableInt { get; set; }
        public decimal Decimal { get; set; }
        public decimal? NullableDecimal { get; set; }
        public long Long { get; set; }
        public long? NullableLong { get; set; }
        public Guid Guid { get; set; }
        public Guid? NullableGuid { get; set; }
        public DateTime Date { get; set; }
        public DateTime? NullableDate { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public DateTimeOffset? NullableDateTimeOffset { get; set; }
        public DateOnly DateOnly { get; set; }
        public DateOnly? NullableDateOnly { get; set; }
        public TestEnum Enum { get; set; }
        public TestEnum? NullableEnum { get; set; }
        public bool Bool { get; set; }
        public bool? NullableBool { get; set; }
        public TestSmartEnum SmartEnum { get; set; }
        public IEnumerable<string> Strings { get; set; }
        public Level1TestModel Level1 { get; set; }
    }

    private sealed class Level1TestModel
    {
        public string String { get; set; }
        public Level2TestModel Level2 { get; set; }
    }

    private sealed class Level2TestModel
    {
        public int Int { get; set; }
    }

    [Fact]
    public void BuildExpressionTest()
    {
        var testGuid = Guid.NewGuid();
        var today = DateTime.Today.ToString();
        var dateTimeOffset = DateTimeOffset.UtcNow.ToString();
        var dateOnly = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date).ToString();
        var queryProps = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Name), new[] { "Test" } },
                { nameof(TestModel.Int), new[] { "2" } },
                { nameof(TestModel.NullableInt), new[] { "1" } },
                { nameof(TestModel.Guid), new[] { testGuid.ToString() } },
                { nameof(TestModel.NullableGuid), new[] { testGuid.ToString() } },
                { nameof(TestModel.Date), new[] { today } },
                { nameof(TestModel.NullableDate), new[] { today } },
                { nameof(TestModel.DateTimeOffset), new[] { dateTimeOffset } },
                { nameof(TestModel.NullableDateTimeOffset), new[] { dateTimeOffset } },
                { nameof(TestModel.DateOnly), new[] { dateOnly } },
                { nameof(TestModel.NullableDateOnly), new[] { dateOnly } },
                { nameof(TestModel.Enum), new[] { TestEnum.TestValue.ToString() } },
                { nameof(TestModel.NullableEnum), new[] { TestEnum.TestValue.ToString() } },
                { nameof(TestModel.Bool), new[] { "true" } },
                { nameof(TestModel.NullableBool), new[] { "false" } }
            };
        var sb = new StringBuilder();
        sb.Append("((((((((((((((entity.Name.Contains(\"test\") ");
        sb.Append($"AndAlso (entity.Int == 2)) ");
        sb.Append($"AndAlso ((entity.NullableInt != null) AndAlso (entity.NullableInt.Value == 1))) ");
        sb.Append($"AndAlso (entity.Guid == {testGuid})) ");
        sb.Append($"AndAlso ((entity.NullableGuid != null) AndAlso (entity.NullableGuid.Value == {testGuid}))) ");
        sb.Append($"AndAlso (entity.Date.Date == {today})) ");
        sb.Append($"AndAlso ((entity.NullableDate != null) AndAlso (entity.NullableDate.Value.Date == {today}))) ");
        sb.Append($"AndAlso (entity.DateTimeOffset == {dateTimeOffset})) ");
        sb.Append($"AndAlso ((entity.NullableDateTimeOffset != null) AndAlso (entity.NullableDateTimeOffset.Value == {dateTimeOffset}))) ");

        sb.Append($"AndAlso (entity.DateOnly == {dateOnly})) ");
        sb.Append($"AndAlso ((entity.NullableDateOnly != null) AndAlso (entity.NullableDateOnly.Value == {dateOnly}))) ");

        sb.Append($"AndAlso (entity.Enum == {TestEnum.TestValue})) ");
        sb.Append($"AndAlso ((entity.NullableEnum != null) AndAlso (entity.NullableEnum.Value == {TestEnum.TestValue}))) ");
        sb.Append($"AndAlso (entity.Bool == True)) ");
        sb.Append($"AndAlso ((entity.NullableBool != null) AndAlso (entity.NullableBool.Value == False)))");
        var expected = sb.ToString();
        var expression = FilterExpressionBuilder<TestModel>.Build(queryProps);
        Assert.Equal(expected, expression.Body.ToString());
    }

    [Fact]
    public void BuildExpressionEmptyTest()
    {
        var expression = FilterExpressionBuilder<TestModel>.Build(new Dictionary<string, IReadOnlyCollection<string>>());
        Assert.Null(expression);
    }

    [Fact]
    public void TestNestedProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { $"{nameof(TestModel.Level1)}.{nameof(TestModel.Level1.String)}", new[] { "coNtaIns(My Name)" } },
                { $"{nameof(TestModel.Level1)}.{nameof(TestModel.Level1.Level2)}.{nameof(TestModel.Level1.Level2.Int)}", new[] { "10" } }
            };
        var expression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expected = $"(entity.Level1.String.ToLower().Contains(\"my name\") AndAlso (entity.Level1.Level2.Int == 10))";
        Assert.Equal(expected, expression.Body.ToString());
    }

    [Fact]
    public void TestDateRangeProducesCorrectExpression()
    {
        var queryProps = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Date), new[] { $"{DateTime.Today} & {DateTime.Today.AddDays(1)}" } }
            };
        var expression = FilterExpressionBuilder<TestModel>.Build(queryProps);
        var expected = $"((entity.Date.Date >= {DateTime.Today}) AndAlso (entity.Date.Date <= {DateTime.Today.AddDays(1)}))";
        Assert.Equal(expected, expression.Body.ToString());
    }

    [Fact]
    public void TestNullableDateRangeProducesCorrectExpression()
    {
        var queryProps = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableDate), new[] { $"{DateTime.Today} & {DateTime.Today.AddDays(1)}" } }
            };
        var expression = FilterExpressionBuilder<TestModel>.Build(queryProps);
        var expected = $"((entity.NullableDate != null) AndAlso"
                     + $" ((entity.NullableDate.Value.Date >= {DateTime.Today}) AndAlso (entity.NullableDate.Value.Date <= {DateTime.Today.AddDays(1)})))";
        Assert.Equal(expected, expression.Body.ToString());
    }

    [Fact]
    public void TestDateOnOrAfterProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Date), new[] { $"{DateTime.Today} &" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.Date.Date >= {DateTime.Today})";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestDateOnOrBeforeProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Date), new[] { $"& {DateTime.Today}" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.Date.Date <= {DateTime.Today})";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestNullableDateOnOrBeforeProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableDate), new[] { $"& {DateTime.Today}" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"((entity.NullableDate != null) AndAlso (entity.NullableDate.Value.Date <= {DateTime.Today}))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestDateTimeOffsetDateRangeProducesCorrectExpression()
    {
        var now = DateTimeOffset.UtcNow;
        var queryProps = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.DateTimeOffset), new[] { $"{now} & {now.AddMinutes(1)}" } }
            };
        var expression = FilterExpressionBuilder<TestModel>.Build(queryProps);
        var expected = $"((entity.DateTimeOffset >= {now}) AndAlso (entity.DateTimeOffset <= {now.AddMinutes(1)}))";
        Assert.Equal(expected, expression.Body.ToString());
    }

    [Fact]
    public void TestNullableDateTimeOffsetDateRangeProducesCorrectExpression()
    {
        var now = DateTimeOffset.UtcNow;
        var queryProps = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableDateTimeOffset), new[] { $"{now} & {now.AddMinutes(1)}" } }
            };
        var expression = FilterExpressionBuilder<TestModel>.Build(queryProps);
        var expected = $"((entity.NullableDateTimeOffset != null) AndAlso"
                     + $" ((entity.NullableDateTimeOffset.Value >= {now}) AndAlso (entity.NullableDateTimeOffset.Value <= {now.AddMinutes(1)})))";
        Assert.Equal(expected, expression.Body.ToString());
    }

    [Fact]
    public void TestDateTimeOffsetDateOnOrAfterProducesCorrectExpression()
    {
        var now = DateTimeOffset.UtcNow;
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.DateTimeOffset), new[] { $"{now} &" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.DateTimeOffset >= {now})";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestDateTimeOffsetDateOnOrBeforeProducesCorrectExpression()
    {
        var now = DateTimeOffset.UtcNow;
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.DateTimeOffset), new[] { $"& {now}" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.DateTimeOffset <= {now})";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestDateTimeOffsetNullableDateOnOrBeforeProducesCorrectExpression()
    {
        var now = DateTimeOffset.UtcNow;
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableDateTimeOffset), new[] { $"& {now}" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"((entity.NullableDateTimeOffset != null) AndAlso (entity.NullableDateTimeOffset.Value <= {now}))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestDateOnlyDateRangeProducesCorrectExpression()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var queryProps = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.DateOnly), new[] { $"{today} & {today.AddDays(3)}" } }
            };
        var expression = FilterExpressionBuilder<TestModel>.Build(queryProps);
        var expected = $"((entity.DateOnly >= {today}) AndAlso (entity.DateOnly <= {today.AddDays(3)}))";
        Assert.Equal(expected, expression.Body.ToString());
    }

    [Fact]
    public void TestNullableDateOnlyDateRangeProducesCorrectExpression()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var queryProps = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableDateOnly), new[] { $"{today} & {today.AddDays(3)}" } }
            };
        var expression = FilterExpressionBuilder<TestModel>.Build(queryProps);
        var expected = $"((entity.NullableDateOnly != null) AndAlso"
                     + $" ((entity.NullableDateOnly.Value >= {today}) AndAlso (entity.NullableDateOnly.Value <= {today.AddDays(3)})))";
        Assert.Equal(expected, expression.Body.ToString());
    }

    [Fact]
    public void TestDateOnlyDateOnOrAfterProducesCorrectExpression()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.DateOnly), new[] { $"{today} &" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.DateOnly >= {today})";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestDateOnlyDateOnOrBeforeProducesCorrectExpression()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.DateOnly), new[] { $"& {today}" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.DateOnly <= {today})";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestDateOnlyNullableDateOnOrBeforeProducesCorrectExpression()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableDateOnly), new[] { $"& {today}" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"((entity.NullableDateOnly != null) AndAlso (entity.NullableDateOnly.Value <= {today}))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestNumericRangeProducesCorrectExpression()
    {
        var queryProps = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableInt), new[] { $"1 & 3" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProps);
        var expectedExpressionString = $"((entity.NullableInt != null) AndAlso"
                                      + " ((entity.NullableInt.Value >= 1) AndAlso (entity.NullableInt.Value <= 3)))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestNumericGteProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Decimal), new[] { $"1.5 &" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.Decimal >= 1.5)";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestNumericLteProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableLong), new[] { $"&42" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"((entity.NullableLong != null) AndAlso (entity.NullableLong.Value <= 42))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestStringStartsWithProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Name), new[] { "startsWith(My Name)" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = "entity.Name.ToLower().StartsWith(\"my name\")";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestStringEndsWithProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Name), new[] { " EndsWith(My Name) " } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = "entity.Name.ToLower().EndsWith(\"my name\")";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestStringContainsProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Name), new[] { "coNtaIns(My Name)"} }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = "entity.Name.ToLower().Contains(\"my name\")";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestStringEqualsProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Name), new[] { "equAls(My Name)"} }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = "entity.Name.ToLower().Equals(\"my name\")";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestListOfStringValuesProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Name), new[] { "startsWith(Suffix)", "endsWith(Prefix)", "equals(Exact)", "middle"} }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = "(((entity.Name.ToLower().StartsWith(\"suffix\") OrElse "
                                        + "entity.Name.ToLower().EndsWith(\"prefix\")) OrElse "
                                        + "entity.Name.ToLower().Equals(\"exact\")) OrElse "
                                        + "entity.Name.Contains(\"middle\"))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestListOfNullableGuidsProducesCorrectExpression()
    {
        var guid1 = Guid.NewGuid().ToString();
        var guid2 = Guid.NewGuid().ToString();
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableGuid), new[] { guid1, guid2 } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(((entity.NullableGuid != null) AndAlso (entity.NullableGuid.Value == {guid1})) OrElse "
                                      + $"((entity.NullableGuid != null) AndAlso (entity.NullableGuid.Value == {guid2})))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestListOfNumericRangesProducesCorrectExpression()
    {
        var int1 = 10.ToString();
        var int2 = 20.ToString();
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Int), new[] { $"&{int1}", $"{int2}&" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var exprectedExpressionString = $"((entity.Int <= {int1}) OrElse (entity.Int >= {int2}))";
        Assert.Equal(exprectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestNotForSingleValuesProducesCorrectExpression()
    {
        var guid = Guid.NewGuid().ToString();
        var @decimal = 10.5m.ToString();
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableGuid), new[] { $"nOt({guid})" } },
                { nameof(TestModel.Decimal), new[] { @decimal } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(Not(((entity.NullableGuid != null) AndAlso (entity.NullableGuid.Value == {guid}))) "
                                     + $"AndAlso (entity.Decimal == {@decimal}))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestListOfNotValuesAndRangesProducesCorrectExpression()
    {
        var int1 = 10.ToString();
        var int2 = 15.ToString();
        var int3 = 20.ToString();
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Int), new[] { $"not({int2})", $"not({int1}&)", $"not(&{int3})" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"((Not((entity.Int == {int2})) OrElse Not((entity.Int >= {int1}))) OrElse Not((entity.Int <= {int3})))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestListOfNotStringValuesProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Name), new[] { "not(Tesco)", "not(equals(Asda))" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = "(Not(entity.Name.Contains(\"tesco\")) OrElse Not(entity.Name.ToLower().Equals(\"asda\")))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestQuotedStringValuesProduceCorrectExpression()
    {
        var string1 = "\"startsWith()\"";
        var string2 = "\"\"endsWith()\"\"";
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Name), new[] { string1, string2 } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.Name.Contains({string1.ToLower()}) OrElse "
                                      + $"entity.Name.Contains({string2.ToLower()}))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestEqualsFunctionForNonStringValuesIsPassedThroughAndProduceCorrectExpression()
    {
        var guid1 = Guid.NewGuid().ToString();
        var guid2 = Guid.NewGuid().ToString();
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.NullableGuid), new[] { $"equals({guid1})", $"not(Equals({guid2}))" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(((entity.NullableGuid != null) AndAlso (entity.NullableGuid.Value == {guid1})) OrElse "
                                     + $"Not(((entity.NullableGuid != null) AndAlso (entity.NullableGuid.Value == {guid2}))))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestSmartEnumValueProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.SmartEnum), new[] { $"equals({TestSmartEnum.One.Value})" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = $"(entity.SmartEnum == {TestSmartEnum.One.Name})"; // Note that smart enum ToString returns its name
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void TestStringCollectionValuesProducesCorrectExpression()
    {
        var queryProperties = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { nameof(TestModel.Strings), new[] { "equals(value1)", "equals(value2)" } }
            };
        var actualExpression = FilterExpressionBuilder<TestModel>.Build(queryProperties);
        var expectedExpressionString = "entity.Strings.Any(item => (item.ToLower().Equals(\"value1\") OrElse item.ToLower().Equals(\"value2\")))";
        Assert.Equal(expectedExpressionString, actualExpression.Body.ToString());
    }

    [Fact]
    public void SortExpressionTest()
    {
        SortExpressionBuilder<TestModel>.Build(new[] { nameof(TestModel.Name) }, ListSortDirection.Ascending);
        Assert.True(true);
    }
}
