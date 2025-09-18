using System.ComponentModel.DataAnnotations;

namespace Yippy.Web.Models;

public class CreatePostModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(128, ErrorMessage = "Title cannot exceed 128 characters")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(65535, ErrorMessage = "Content cannot exceed 65,535 characters")]
    [Display(Name = "Content")]
    public string Body { get; set; } = string.Empty;
}

public class CreatePostResponse
{
    public Guid Id { get; set; }
}