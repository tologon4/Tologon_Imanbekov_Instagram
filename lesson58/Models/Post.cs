using System.ComponentModel.DataAnnotations;

namespace lesson58.Models;

public class Post
{
    public int Id { get; set; }
    public string? FilePath { get; set; }
    public int? OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }
    [Required(ErrorMessage = "Provide a description!")]
    public string Description { get; set; }
    public DateTime? AddedDate { get; set; }
    public int? LikesCount { get; set; }
    public ICollection<UserPostLike>? LikeUsers { get; set; }
}