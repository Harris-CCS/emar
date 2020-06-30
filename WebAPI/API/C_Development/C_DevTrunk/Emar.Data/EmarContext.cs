using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Data
{
    public class EmarContext : DbContext
    {
        private readonly DbContextOptions _options;

        public EmarContext(DbContextOptions options) : base(options)
        {
            _options = options;
        }
        public DbSet<ExternalId> ExternalIds { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderAdministration> OrderAdministrations { get; set; }
        public DbSet<OrderEvent> OrderEvents { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<User> Users { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        // Only used for development - will use more sophisticated methods when beyond dev...
        //        optionsBuilder.UseSqlServer(_options.Extensions);
        //            "Data Source = (localdb)\\SQL2016; Initial Catalog = EMAR");
        //    }
        //}
    }
}
