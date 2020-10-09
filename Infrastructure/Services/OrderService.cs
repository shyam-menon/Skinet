using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specification;

namespace Infrastructure.Services
{
    public class OrderService : IOrderService
    {
      
        private readonly IBasketRepository _basketRepo;      
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IBasketRepository basketRepo, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;            
            _basketRepo = basketRepo;           
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

            //create order
            var order = new Order(items, buyerEmail, shippingAddress, deliveryMethod, subtotal);
            _unitOfWork.Repository<Order>().Add(order);

            //save to db. Course item 219
            var result = await _unitOfWork.Complete();

            //Save was not successful. UOW takes care of rolling back the transaction
            if (result <= 0) return null;

            //delete the basket if the order was saved
            await _basketRepo.DeleteBasketAsync(basketId);

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
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);

            return await _unitOfWork.Repository<Order>().ListAsync(spec);
        }
    }
}