using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Skill>()
                .HasData(
                new Skill { Id = 1, Name = "Summon Fireball", Damage = 60 },
                new Skill { Id = 2, Name = "Summon Tornado", Damage = 85 },
                new Skill { Id = 3, Name = "Summon Meteor", Damage = 150 }
                );

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .IsRequired()
                .HasDefaultValue("Player");
        }

        public DbSet<Character> Characters => Set<Character>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Weapon> Weapons => Set<Weapon>();
        public DbSet<Skill> Skills => Set<Skill>();
    }
}
