using TourService.Models;

namespace TourService.Dtos;

public record CreateTourRequest(string Name, string Description, TourDifficulty Difficulty, List<string> Tags, double LengthKm, int DurationMinutes);

public record UpdateTourStatusRequest(TourStatus Status);

public record UpdateTourPriceRequest(decimal Price);

public record CreateKeyPointRequest(KeyPointType Type, string Name, string Description, string? ImagePath, double Latitude, double Longitude);

public record SetPositionRequest(double Latitude, double Longitude);

public record StartExecutionRequest(string TourId);

public record CheckProgressRequest(double Latitude, double Longitude);

public record CreateReviewRequest(int Rating, string Comment);
