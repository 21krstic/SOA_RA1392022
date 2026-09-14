using Followers.Api.Data;
using Followers.Grpc;
using Grpc.Core;

namespace Followers.Api.Services;

public class FollowersGrpcService : FollowersGrpc.FollowersGrpcBase
{
    private readonly FollowsRepository _repository;

    public FollowersGrpcService(FollowsRepository repository)
    {
        _repository = repository;
    }

    public override async Task<IsFollowingReply> IsFollowing(IsFollowingRequest request, ServerCallContext context)
    {
        var isFollowing = await _repository.IsFollowingAsync(request.FollowerId, request.FolloweeId);
        return new IsFollowingReply { IsFollowing = isFollowing };
    }
}
