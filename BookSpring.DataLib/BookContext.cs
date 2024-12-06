using System.Reflection;
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
    public DbSet<CategoryModel> Categories { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>().HasMany(x => x.CreatedBooks)
            .WithOne(x => x.CreatedBy)
            .HasForeignKey(x => x.CreatedById).IsRequired(false);
        modelBuilder.Entity<UserModel>().HasMany(x => x.LendBooks)
            .WithOne(x => x.LendTo)
            .HasForeignKey(x => x.LendToId).IsRequired(false);
        modelBuilder.Entity<BookModel>()
            .HasMany(x => x.Categories)
            .WithMany(x => x.Books);
    }
}

public static class DataTool
{
    public static string StringToHash(string s)
    {
        var data = Encoding.UTF8.GetBytes(s);
        var hash = MD5.HashData(data);
        var hashStringBuilder = new StringBuilder();
        foreach (var t in hash)
            hashStringBuilder.Append(t.ToString("x2"));
        return hashStringBuilder.ToString();
    }

    public static string GetProperties<T>(T t)
    {
        StringBuilder builder = new StringBuilder();
        if (t == null) return builder.ToString();

        var properties = t.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        if (properties.Length <= 0) return builder.ToString();

        foreach (var item in properties)
        {
            var name = item.Name;
            var value = item.GetValue(t, null);
            if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
            {
                builder.Append($"{name}:{value ?? "null"},");
            }
        }

        return builder.ToString();
    }
}

public abstract class DataModel
{
    public override string ToString() => $"{GetType()} : {DataTool.GetProperties(this)}";
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