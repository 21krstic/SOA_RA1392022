using Followers.Grpc;
using Grpc.Net.Client;

namespace BlogService.Services;

public class FollowersServiceClient
{
    private readonly FollowersGrpc.FollowersGrpcClient _client;

    public FollowersServiceClient(IConfiguration configuration)
    {
        var address = configuration["Grpc:FollowersServiceUrl"]
            ?? throw new InvalidOperationException("Grpc:FollowersServiceUrl is not configured.");
        var channel = GrpcChannel.ForAddress(address);
        _client = new FollowersGrpc.FollowersGrpcClient(channel);
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followeeId)
    {
        var reply = await _client.IsFollowingAsync(new IsFollowingRequest { FollowerId = followerId, FolloweeId = followeeId });
        return reply.IsFollowing;
    }
}
