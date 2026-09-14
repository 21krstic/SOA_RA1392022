namespace BlogService.Services;

public class FollowersRestClient
{
    private readonly HttpClient _http;

    public FollowersRestClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<string>> GetFollowingAsync(string userId)
    {
        var result = await _http.GetFromJsonAsync<List<string>>($"api/follows/{userId}/following");
        return result ?? [];
    }
}
