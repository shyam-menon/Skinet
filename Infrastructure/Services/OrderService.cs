using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specification;
using Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class OrderService : IOrderService
    {
      
        private readonly IBasketRepository _basketRepo;      
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;

        public OrderService(IBasketRepository basketRepo, IUnitOfWork unitOfWork, IPaymentService
        paymentService, ILoggerFactory loggerFactory)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _basketRepo = basketRepo;
            _logger = loggerFactory.CreateLogger("Orders");
        }

        public async Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId,
         Address shippingAddress)
        {
            //get basket from the repo
            var basket = await _basketRepo.GetBasketAsync(basketId);

            //when the delivery method is not set then set it to free shipping
            if (deliveryMethodId == 0) deliveryMethodId = 4;

            //get the items from the product repo
            var items = new List<OrderItem>();
            foreach (var item in basket.Items)
            {
                //Replacing generic repositories with UOW. Course item 219
                var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                var itemOrdered = new ProductItemOrdered(productItem.Id, productItem.Name,
                productItem.PictureUrl);
                //Get the quantity from the client but get the price from the product in the database
                var orderItem = new OrderItem(itemOrdered, productItem.Price, item.Quantity);
                items.Add(orderItem);
            }

            //get the delivery method
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

            //calc subtotal
            var subtotal = items.Sum(item => item.Price * item.Quantity);

            // check to see if order exists. Course item 270
            var spec = new OrderByPaymentIntentIdSpecification(basket.PaymentIntentId);
            var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

            if (existingOrder != null)
            {
                _unitOfWork.Repository<Order>().Delete(existingOrder);
                // Update the payment intent before creating a replacement order. Good practice when
                // adjusting order or even when changing items in basket
                await _paymentService.CreateOrUpdatePaymentIntent(basket.PaymentIntentId);
            }

            //create order
            var order = new Order(items, buyerEmail, shippingAddress, deliveryMethod, subtotal, basket.PaymentIntentId);

            // Create a log event with an event Id
            _logger.LogInformation(LogEvents.OrderCreation,"SERVICE ENTRY: Creating a new order: {Order}", order);
            _unitOfWork.Repository<Order>().Add(order);

            //save to db. Course item 219
            var result = await _unitOfWork.Complete();

            //Save was not successful. UOW takes care of rolling back the transaction
            if (result <= 0) return null;      

            //return order
            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            return await _unitOfWork.Repository<DeliveryMethod>().ListAllAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id, string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(id, buyerEmail);
            
            return await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            _logger.LogInformation("SERVICE ENTRY: Inside get orders for user.");

            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);

            return await _unitOfWork.Repository<Order>().ListAsync(spec);
        }
    }
}