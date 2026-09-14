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

    public BlogsController(BlogsRepository blogs, FollowersRestClient followersRest)
    {
        _blogs = blogs;
        _followersRest = followersRest;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Blog>> GetById(string id)
    {
        var blog = await _blogs.GetByIdAsync(id);
        return blog is null ? NotFound() : Ok(blog);
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
