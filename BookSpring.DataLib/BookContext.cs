using System.Security.Cryptography;
using System.Text;
using BookSpring.DataLib.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookSpring.DataLib;

public sealed class BookContext : DbContext
{
    public BookContext(DbContextOptions<BookContext> options) : base(options)
    {
        Books = Set<BookModel>();
        Users = Set<UserModel>();
    }

    public DbSet<BookModel> Books { get; init; }
    public DbSet<UserModel> Users { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>().HasMany(x => x.CreatedBooks).WithOne(x => x.CreatedBy);
        modelBuilder.Entity<UserModel>().HasMany(x => x.LendBooks).WithOne(x => x.LendTo);
    }
}

public static class ContextStatic
{
    public static string HashEncryption(this string str)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(str + DateTime.Now.ToString("s"))))
            .Replace("/", "-");

    public static string Base64Encryption(this string str) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
}

// ReSharper disable once UnusedType.Global
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BookContext>
{
    public BookContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BookContext>();
        optionsBuilder.UseSqlite("Data Source=Data.db");
        return new BookContext(optionsBuilder.Options);
    }
}