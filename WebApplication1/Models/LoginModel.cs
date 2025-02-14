using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public record LoginModel
    (
      [Required] string Username,
      [Required] string Password
    );
}
