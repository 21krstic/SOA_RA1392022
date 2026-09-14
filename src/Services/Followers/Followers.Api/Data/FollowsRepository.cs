using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace Followers.Api.Data;

public class FollowsRepository : IAsyncDisposable
{
    private readonly IDriver _driver;

    public FollowsRepository(IOptions<Neo4jSettings> settings)
    {
        var s = settings.Value;
        _driver = GraphDatabase.Driver(s.Uri, AuthTokens.Basic(s.Username, s.Password));
    }

    public async Task FollowAsync(string followerId, string followeeId)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                "MERGE (a:User {id: $followerId}) " +
                "MERGE (b:User {id: $followeeId}) " +
                "MERGE (a)-[:FOLLOWS]->(b)",
                new { followerId, followeeId });
            await cursor.ConsumeAsync();
        });
    }

    public async Task UnfollowAsync(string followerId, string followeeId)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                "MATCH (a:User {id: $followerId})-[r:FOLLOWS]->(b:User {id: $followeeId}) DELETE r",
                new { followerId, followeeId });
            await cursor.ConsumeAsync();
        });
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followeeId)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                "MATCH (a:User {id: $followerId})-[:FOLLOWS]->(b:User {id: $followeeId}) RETURN count(*) > 0 AS result",
                new { followerId, followeeId });
            var record = await cursor.SingleAsync();
            return record["result"].As<bool>();
        });
    }

    public async Task<List<string>> GetFollowingAsync(string userId)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                "MATCH (:User {id: $userId})-[:FOLLOWS]->(followee:User) RETURN followee.id AS id",
                new { userId });
            var records = await cursor.ToListAsync();
            return records.Select(r => r["id"].As<string>()).ToList();
        });
    }

    public async Task<List<string>> GetRecommendationsAsync(string userId, int limit = 10)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                "MATCH (me:User {id: $userId})-[:FOLLOWS]->(:User)-[:FOLLOWS]->(rec:User) " +
                "WHERE NOT (me)-[:FOLLOWS]->(rec) AND rec.id <> $userId " +
                "RETURN DISTINCT rec.id AS id LIMIT $limit",
                new { userId, limit });
            var records = await cursor.ToListAsync();
            return records.Select(r => r["id"].As<string>()).ToList();
        });
    }

    public async ValueTask DisposeAsync() => await _driver.DisposeAsync();
}
