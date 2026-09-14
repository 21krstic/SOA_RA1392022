using System.Text.Json.Serialization;

namespace Purchase.Api.Services;

public record TourSummary([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("status")] string Status);

public class TourServiceClient
{
    private readonly HttpClient _http;

    public TourServiceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<TourSummary?> GetTourAsync(string tourId)
    {
        var response = await _http.GetAsync($"api/tours/{tourId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TourSummary>();
    }
}
