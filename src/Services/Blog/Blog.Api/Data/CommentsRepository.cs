using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BlogService.Models;

namespace BlogService.Data;

public class CommentsRepository
{
    private readonly IMongoCollection<Comment> _comments;

    public CommentsRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _comments = database.GetCollection<Comment>("comments");
    }

    public Task CreateAsync(Comment comment) => _comments.InsertOneAsync(comment);

    public Task<List<Comment>> GetByBlogAsync(string blogId) =>
        _comments.Find(c => c.BlogId == blogId).SortBy(c => c.CreatedAt).ToListAsync();
}
