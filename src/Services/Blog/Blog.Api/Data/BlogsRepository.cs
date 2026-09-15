using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using BlogService.Models;

namespace BlogService.Data;

public class BlogsRepository
{
    private readonly IMongoCollection<Blog> _blogs;

    public BlogsRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _blogs = database.GetCollection<Blog>("blogs");
    }

    public Task CreateAsync(Blog blog) => _blogs.InsertOneAsync(blog);

    public async Task<Blog?> GetByIdAsync(string id) =>
        ObjectId.TryParse(id, out _) ? await _blogs.Find(b => b.Id == id).FirstOrDefaultAsync() : null;

    public Task<List<Blog>> GetByAuthorsAsync(IEnumerable<string> authorIds) =>
        _blogs.Find(Builders<Blog>.Filter.In(b => b.AuthorId, authorIds))
            .SortByDescending(b => b.CreatedAt)
            .ToListAsync();
}
