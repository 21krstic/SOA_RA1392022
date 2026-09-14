namespace BlogService.Dtos;

public record CreateBlogRequest(string Title, string Description, List<string> ImagePaths);

public record CreateCommentRequest(string Text);
