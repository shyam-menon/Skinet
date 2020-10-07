using System;
using System.Linq;
using System.Reflection;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base
        (options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }

        //Db set in case basket needs to be stored into DB instead of Redis
        //Comment : Storing basket in database instead of Redis
        public DbSet<Basket> Baskets { get; set; }

        //Add Db set for order. Course item 210
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DeliveryMethod> DeliveryMethods { get; set; }

        //This method is responsible for creating the migration and we can override this for the behavior that we need
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            //Sqlite does not have support for decimal and so use double instead
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType
                    == typeof(decimal));
                    
                    // Sql lite does not support date time offset and so use datetime. Course item 223
                    var dateTimeProperties = entityType.ClrType.GetProperties()
                        .Where(p => p.PropertyType == typeof(DateTimeOffset));

                    foreach (var property in properties)
                    {
                        modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion<double>();
                    }

                    foreach (var property in dateTimeProperties)
                    {
                         modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion(new DateTimeOffsetToBinaryConverter());   
                    }
                }
            }
        }
    }
}