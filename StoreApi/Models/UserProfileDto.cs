using System.ComponentModel.DataAnnotations;

namespace StoreApi.Models;

public class UserProfileDto
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string FirstName { get; set; } = "";
    [MaxLength(100)]
    public string LastName { get; set; } = "";
    [MaxLength(100)]
    public string Email { get; set; } = "";
    [MaxLength(20)]
    public string Phone { get; set; } = "";
    [MaxLength(100)]
    public string Address { get; set; } = "";
    [MaxLength(20)]
    public string Role { get ; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}