using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogService.Data;
using BlogService.Dtos;
using BlogService.Models;
using BlogService.Services;

namespace BlogService.Controllers;

[ApiController]
[Route("api/blogs")]
public class BlogsController : ControllerBase
{
    private readonly BlogsRepository _blogs;
    private readonly FollowersRestClient _followersRest;
    private readonly FollowersServiceClient _followersGrpc;

    public BlogsController(BlogsRepository blogs, FollowersRestClient followersRest, FollowersServiceClient followersGrpc)
    {
        _blogs = blogs;
        _followersRest = followersRest;
        _followersGrpc = followersGrpc;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Blog>> Create(CreateBlogRequest request)
    {
        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var blog = new Blog
        {
            AuthorId = authorId,
            Title = request.Title,
            Description = request.Description,
            ImagePaths = request.ImagePaths
        };

        await _blogs.CreateAsync(blog);
        return Ok(blog);
    }

    // Readable only by the author or a follower of the author (requirement 9).
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Blog>> GetById(string id)
    {
        var blog = await _blogs.GetByIdAsync(id);
        if (blog is null) return NotFound();

        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (callerId != blog.AuthorId && !await _followersGrpc.IsFollowingAsync(callerId, blog.AuthorId))
            return Forbid();

        return Ok(blog);
    }

    // Only blogs from authors the caller follows (requirement 9).
    [Authorize]
    [HttpGet("feed")]
    public async Task<ActionResult<List<Blog>>> GetFeed()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var following = await _followersRest.GetFollowingAsync(userId);
        if (following.Count == 0) return Ok(new List<Blog>());

        return Ok(await _blogs.GetByAuthorsAsync(following));
    }
}
