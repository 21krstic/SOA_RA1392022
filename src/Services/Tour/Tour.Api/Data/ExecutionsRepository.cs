using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TourService.Models;

namespace TourService.Data;

public class ExecutionsRepository
{
    private readonly IMongoCollection<TourExecution> _executions;

    public ExecutionsRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _executions = database.GetCollection<TourExecution>("tour_executions");
    }

    public Task CreateAsync(TourExecution execution) => _executions.InsertOneAsync(execution);

    public Task<TourExecution?> GetByIdAsync(string id) => _executions.Find(e => e.Id == id).FirstOrDefaultAsync()!;

    public Task<TourExecution?> GetActiveAsync(string touristId, string tourId) =>
        _executions.Find(e => e.TouristId == touristId && e.TourId == tourId && e.Status == TourExecutionStatus.Active)
            .FirstOrDefaultAsync()!;

    public Task ReplaceAsync(TourExecution execution) =>
        _executions.ReplaceOneAsync(e => e.Id == execution.Id, execution);
}
