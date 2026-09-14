using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogService.Data;
using BlogService.Dtos;
using BlogService.Models;
using BlogService.Services;

namespace BlogService.Controllers;

[ApiController]
[Route("api/blogs/{blogId}/comments")]
public class CommentsController : ControllerBase
{
    private readonly BlogsRepository _blogs;
    private readonly CommentsRepository _comments;
    private readonly FollowersServiceClient _followersGrpc;

    public CommentsController(BlogsRepository blogs, CommentsRepository comments, FollowersServiceClient followersGrpc)
    {
        _blogs = blogs;
        _comments = comments;
        _followersGrpc = followersGrpc;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Comment>> Create(string blogId, CreateCommentRequest request)
    {
        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var blog = await _blogs.GetByIdAsync(blogId);
        if (blog is null) return NotFound("Blog not found.");

        // Commenting requires already following the blog's author (requirement 9).
        // This check is an RPC call to the Followers service.
        if (!await _followersGrpc.IsFollowingAsync(authorId, blog.AuthorId))
            return Forbid();

        var comment = new Comment { BlogId = blogId, AuthorId = authorId, Text = request.Text };
        await _comments.CreateAsync(comment);
        return Ok(comment);
    }

    [HttpGet]
    public async Task<ActionResult<List<Comment>>> GetAll(string blogId) =>
        Ok(await _comments.GetByBlogAsync(blogId));
}
