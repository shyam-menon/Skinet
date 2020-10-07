using Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config
{
    // Configure the order item. Course item 209
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.OwnsOne(i => i.ItemOrdered, io => {io.WithOwner();});

            builder.Property(i => i.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}