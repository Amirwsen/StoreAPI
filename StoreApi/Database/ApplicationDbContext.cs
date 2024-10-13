using Microsoft.EntityFrameworkCore;
using StoreApi.Models;

namespace StoreApi.Services;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<PasswordReset> PasswordResets { get; set; }
}