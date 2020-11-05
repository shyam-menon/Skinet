using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Order = Core.Entities.OrderAggregate.Order;
using Product = Core.Entities.Product;

namespace Infrastructure.Services
{
    //Payment service implementation. Course item 259
    public class PaymentService : IPaymentService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public PaymentService(IBasketRepository basketRepository, IUnitOfWork unitOfWork,       
        IConfiguration configuration, ILogger<PaymentService> logger)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _basketRepository = basketRepository;
            _logger = logger;
        }

        public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
        {
            // Needed for Payment intent
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

            var basket = await _basketRepository.GetBasketAsync(basketId);

            if (basket == null) return null;
            
            //Set shipping price to 0
            var shippingPrice = 0m;

            // Dont trust the value in the basket and always get the price from the database
            if (basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
                    .GetByIdAsync((int)basket.DeliveryMethodId);
                shippingPrice = deliveryMethod.Price;
            }

            foreach (var item in basket.Items)
            {
                var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);

                // When there is a mismatch in the price in the basket vs price in the DB
                // Set it to the price in the DB
                if (item.Price != productItem.Price)
                {
                    item.Price = productItem.Price;
                }

                // Stripe class for payment intent
                var service = new PaymentIntentService();
                PaymentIntent intent;

                // Check if no payment has been set already. If it is an update to the basket
                // then intent may already be available and the client makes a change after checkout
                if (string.IsNullOrEmpty(basket.PaymentIntentId))
                {
                    _logger.LogDebug("SERVICE ENTRY: Creating new Payment intent Id");
                    var options = new PaymentIntentCreateOptions
                    {
                        // Converting from decimal to long needs multiplication by 100
                        Amount = (long) basket.Items.Sum(i => i.Quantity * (i.Price * 100)) 
                                    + (long)shippingPrice * 100,
                        Currency = "inr",
                        PaymentMethodTypes = new List<string> {"card"}
                    };
                    // Create the intent on Stripe 
                    intent = await service.CreateAsync(options);

                    // Update basket with the payment intent Id and client secret
                    basket.PaymentIntentId = intent.Id;
                    basket.ClientSecret =  intent.ClientSecret;
                }
                // Update the payment intent
                else
                {
                    _logger.LogDebug("SERVICE ENTRY: Updating existing Payment intent Id {PaymentIntentId}", basket.PaymentIntentId);
                    var options = new PaymentIntentUpdateOptions
                    {
                        Amount = (long) basket.Items.Sum(i => i.Quantity * (i.Price * 100)) 
                                    + (long)shippingPrice * 100
                    };

                    await service.UpdateAsync(basket.PaymentIntentId, options);
                }                              
            }

            //Update the basket with the information from Stripe
            await _basketRepository.CreateOrUpdateBasketAsync(basket);
            return basket;
        }

        // Updating the Order with payment intent result from Stripe. Course item 276

        public async Task<Order> UpdateOrderPaymentFailed(string paymentIntentId)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
           var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

           if (order == null) return null;

           order.Status = OrderStatus.PaymentFailed;
           await _unitOfWork.Complete();

           return order;
        }

        public async Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentId)
        {
           var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
           var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

           if (order == null) return null;

           order.Status = OrderStatus.PaymentReceived;
           _unitOfWork.Repository<Order>().Update(order);

           await _unitOfWork.Complete();

           return order;
        }
    }
}