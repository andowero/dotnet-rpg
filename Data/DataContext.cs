using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
         
        } 
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Skill>().HasData(
                new Skill() {Id=1, Name="Magic missile", Damage=3},
                new Skill() {Id=2, Name="Mundane missile", Damage=1},
                new Skill() {Id=3, Name="Fireball", Damage=7},
                new Skill() {Id=4, Name="Mighty blow", Damage=5},
                new Skill() {Id=5, Name="Salto mortale", Damage=2}
            );
        }

        public DbSet<Character> Characters => Set<Character>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Weapon> Weapons => Set<Weapon>();
        public DbSet<Skill> Skills => Set<Skill>();
    }
}