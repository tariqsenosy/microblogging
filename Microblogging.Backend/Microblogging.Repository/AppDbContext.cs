using Microsoft.EntityFrameworkCore;
using Microblogging.Domain.Entities;

namespace Microblogging.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
}
