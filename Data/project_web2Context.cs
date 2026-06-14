using Microsoft.EntityFrameworkCore;
using project_web2.Models;

namespace project_web2.Data
{
    public class project_web2Context : DbContext
    {
        public project_web2Context(DbContextOptions<project_web2Context> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<CustomerProfiles> CustomerProfiles { get; set; } = default!;
        public DbSet<Products> Products { get; set; } = default!;
        public DbSet<Orders> Orders { get; set; } = default!;
        public DbSet<OrderItems> OrderItems { get; set; } = default!;
    }
}
