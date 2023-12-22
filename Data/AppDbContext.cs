using Microsoft.EntityFrameworkCore;

namespace SukaliCareApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
            SQLitePCL.Batteries_V2.Init();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string db_path = Path.Combine(FileSystem.AppDataDirectory, "sukalicaredb.db3");
            optionsBuilder.UseSqlite($"Data Source={db_path}");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CgmReading>()
                .OwnsOne(p => p.GlucoseValue);
            modelBuilder.Entity<CgmReading>()
                .Property(p => p.GlucoseValue)
                .IsRequired();
        }
        public DbSet<CgmReading> CgmReadings { get; set; }
        public DbSet<CGMDevice> Devices { get; set; }
        public DbSet<LibResource> LibResources { get; set; }
        
    }
}
