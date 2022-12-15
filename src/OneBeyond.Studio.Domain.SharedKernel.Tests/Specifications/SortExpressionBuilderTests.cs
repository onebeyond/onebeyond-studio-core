using System.ComponentModel;
using System.Linq;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;
using Xunit;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.Specifications;

public sealed class SortExpressionBuilderTests : IClassFixture<LogManagerFixture>
{
    [Fact]
    public void Can_sort_by_nested_properties_of_any_level()
    {
        var sortByProperties = new[]
        {
                $"{nameof(RootModel.Level1)}.{nameof(RootModel.Level1.String)}",
                $"{nameof(RootModel.Level1)}.{nameof(RootModel.Level1.Level2)}.{nameof(RootModel.Level1.Level2.Int)}"
            };
        var sortings = SortExpressionBuilder<RootModel>.Build(sortByProperties, ListSortDirection.Ascending).ToArray();
        Assert.Equal(2, sortings.Length);
        Assert.Equal("Convert(entity.Level1.String, Object)", sortings[0].SortBy.Body.ToString());
        Assert.Equal("Convert(entity.Level1.Level2.Int, Object)", sortings[1].SortBy.Body.ToString());
    }

    private sealed record RootModel
    {
        public Level1Model? Level1 { get; init; }
    }

    private sealed record Level1Model
    {
        public string? String { get; init; }
        public Level2Model? Level2 { get; init; }
    }

    private sealed record Level2Model
    {
        public int Int { get; init; }
    }
}
