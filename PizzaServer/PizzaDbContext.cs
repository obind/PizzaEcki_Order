using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using System.Collections.Generic;
using System.Net;
using System;
namespace PizzaServer
{

    public class PizzaDbContext : DbContext
    {

       // public DbSet<Customer> Customers { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        //public DbSet<Address> Addresses { get; set; }
        //public DbSet<Dish> Dishes { get; set; }
        //public DbSet<Extra> Extras { get; set; }
        public DbSet<OrderAssignment> OrderAssignments { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
       // public DbSet<Setting> Settings { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PizzaEckiDb", "database.sqlite");
            optionsBuilder.UseSqlite($"Data Source={path}");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>().HasKey(o => o.OrderItemId);
            // ... Konfiguration für andere Entitäten ...
        }

    }

}
