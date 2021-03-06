using System;
using System.Collections.Generic;

namespace Core.Entities.OrderAggregate
{
    public class Order : BaseEntity
    {
        public Order()
        {
        }

        public Order(IReadOnlyList<OrderItem> orderItems, string buyerEmail, Address shipToAddress,
         DeliveryMethod deliveryMethod, decimal subtotal, string paymentIntentId)
        {
            BuyerEmail = buyerEmail;           
            ShipToAddress = shipToAddress;
            DeliveryMethod = deliveryMethod;
            OrderItems = orderItems;
            Subtotal = subtotal;
            PaymentIntentId = paymentIntentId;           
        }

        public string BuyerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
        public Address ShipToAddress { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public IReadOnlyList<OrderItem> OrderItems { get; set; }
        
        //Sub total of the order item with the quantity
        public decimal Subtotal { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string PaymentIntentId { get; set; }
        public decimal GetTotal()
        {
            if (DeliveryMethod != null)
                return Subtotal + DeliveryMethod.Price;
            
            return Subtotal;
        }

        public override string ToString()
        {
            return $"BuyerEmail: {BuyerEmail}; OrderDate: {OrderDate};";
        }
    }
}