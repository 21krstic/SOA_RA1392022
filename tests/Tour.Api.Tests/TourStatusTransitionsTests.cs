using TourService.Models;
using Xunit;

namespace Tour.Api.Tests;

public class TourStatusTransitionsTests
{
    [Theory]
    [InlineData(TourStatus.Draft, TourStatus.Published)]
    [InlineData(TourStatus.Published, TourStatus.Archived)]
    public void IsValid_AllowedTransitions_ReturnsTrue(TourStatus from, TourStatus to)
    {
        Assert.True(TourStatusTransitions.IsValid(from, to));
    }

    [Theory]
    [InlineData(TourStatus.Draft, TourStatus.Archived)]
    [InlineData(TourStatus.Published, TourStatus.Draft)]
    [InlineData(TourStatus.Archived, TourStatus.Draft)]
    [InlineData(TourStatus.Archived, TourStatus.Published)]
    [InlineData(TourStatus.Draft, TourStatus.Draft)]
    [InlineData(TourStatus.Published, TourStatus.Published)]
    [InlineData(TourStatus.Archived, TourStatus.Archived)]
    public void IsValid_DisallowedTransitions_ReturnsFalse(TourStatus from, TourStatus to)
    {
        Assert.False(TourStatusTransitions.IsValid(from, to));
    }
}
