using Grpc.Core;
using Purchase.Api.Data;
using Purchase.Grpc;

namespace Purchase.Api.Services;

public class PurchaseGrpcService : PurchaseGrpc.PurchaseGrpcBase
{
    private readonly PurchaseTokensRepository _tokens;

    public PurchaseGrpcService(PurchaseTokensRepository tokens)
    {
        _tokens = tokens;
    }

    public override async Task<IsPurchasedReply> IsPurchased(IsPurchasedRequest request, ServerCallContext context)
    {
        var isPurchased = await _tokens.IsPurchasedAsync(request.TouristId, request.TourId);
        return new IsPurchasedReply { IsPurchased = isPurchased };
    }
}
