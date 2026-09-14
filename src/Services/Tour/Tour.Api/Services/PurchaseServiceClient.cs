using Grpc.Net.Client;
using Purchase.Grpc;

namespace TourService.Services;

public class PurchaseServiceClient
{
    private readonly PurchaseGrpc.PurchaseGrpcClient _client;

    public PurchaseServiceClient(IConfiguration configuration)
    {
        var address = configuration["Grpc:PurchaseServiceUrl"]
            ?? throw new InvalidOperationException("Grpc:PurchaseServiceUrl is not configured.");
        var channel = GrpcChannel.ForAddress(address);
        _client = new PurchaseGrpc.PurchaseGrpcClient(channel);
    }

    public async Task<bool> IsPurchasedAsync(string touristId, string tourId)
    {
        var reply = await _client.IsPurchasedAsync(new IsPurchasedRequest { TouristId = touristId, TourId = tourId });
        return reply.IsPurchased;
    }
}
