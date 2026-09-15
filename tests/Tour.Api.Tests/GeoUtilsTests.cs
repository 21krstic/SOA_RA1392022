using TourService.Services;
using Xunit;

namespace Tour.Api.Tests;

public class GeoUtilsTests
{
    [Fact]
    public void DistanceMeters_SamePoint_ReturnsZero()
    {
        var distance = GeoUtils.DistanceMeters(45.2671, 19.8335, 45.2671, 19.8335);

        Assert.Equal(0, distance, precision: 6);
    }

    [Fact]
    public void DistanceMeters_IsSymmetric()
    {
        var forward = GeoUtils.DistanceMeters(45.2671, 19.8335, 45.2551, 19.8419);
        var backward = GeoUtils.DistanceMeters(45.2551, 19.8419, 45.2671, 19.8335);

        Assert.Equal(forward, backward, precision: 6);
    }

    [Fact]
    public void DistanceMeters_OneDegreeOfLatitude_IsRoughlyOneHundredElevenKilometers()
    {
        // A well-known approximation: 1 degree of latitude is ~111.2 km on a
        // sphere with Earth's mean radius, which is the model this method uses.
        var distance = GeoUtils.DistanceMeters(0, 0, 1, 0);

        Assert.InRange(distance, 111_000, 111_400);
    }

    [Fact]
    public void DistanceMeters_PointsWithinProximityThreshold_AreCloserThanFiftyMeters()
    {
        // ~0.0003 degrees of latitude is roughly 33 meters — inside the
        // 50m proximity threshold used to mark a key point as reached.
        var distance = GeoUtils.DistanceMeters(45.2671, 19.8335, 45.26740, 19.8335);

        Assert.True(distance < 50, $"Expected < 50m, got {distance}m");
    }

    [Fact]
    public void DistanceMeters_PointsFarApart_ExceedProximityThreshold()
    {
        var distance = GeoUtils.DistanceMeters(45.2671, 19.8335, 45.2551, 19.8419);

        Assert.True(distance > 50, $"Expected > 50m, got {distance}m");
    }
}
