namespace Catalog.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
}
