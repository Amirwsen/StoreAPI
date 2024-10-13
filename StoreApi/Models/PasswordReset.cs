using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace StoreApi.Models;
[Index("Email", IsUnique = true)]
public class PasswordReset
{
    public int Id { get; set; }
    [Required , MaxLength(100)]
    public string Email { get; set; }
    [MaxLength(100)]
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
}