using System.ComponentModel.DataAnnotations;

namespace StoreApi.Models;

public class ContactDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; } 
    public string? Email { get; set; } 
    public string? Phone { get; set; } 
    public int SubjectId { get; set; } 
    public string? Message { get; set; } 
}