using System.ComponentModel.DataAnnotations;

namespace IdentityService;

public class RegisterViewModel
{
    [Required]
    public string Email { get; set; } = default!;
    [Required]
    public string Password { get; set; } = default!;
    [Required]
    public string Username { get; set; } = default!;
    [Required]
    public string FullName { get; set; } = default!;
    public string ReturnUrl { get; set; } = default!;
    public string Button { get; set; } = default!;
}
