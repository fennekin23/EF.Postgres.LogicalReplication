using Microsoft.EntityFrameworkCore;

namespace EF.Postgres.LogicalReplication.UnitTests;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
}