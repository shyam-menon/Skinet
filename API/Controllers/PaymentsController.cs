using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using API.Errors;
using System.IO;
using Stripe;
using Order = Core.Entities.OrderAggregate.Order;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<IPaymentService> _logger;

        // Web hooks. Course item 275
        private const string WhSecret = "whsec_MOkSzWdryquBBQXtkCNcrMjlmUn3saEr";
        public PaymentsController(IPaymentService paymentService, ILogger<IPaymentService> logger)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        // Authorization only for the method and not for the entire controller as stripe needs to access the controller. Course item 260
        [Authorize]
        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if (basket == null) return BadRequest(new ApiResponse(400, "Problem with your basket"));

            return basket;
        }

        // Use - stripe listen -f https://localhost:5001/api/payments/webhook
        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Parses a JSON string from a Stripe webhook into a Stripe.Event object while verifying
            // the webhook's signature. Ignore the version check
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"],
            WhSecret,300,false);

            PaymentIntent intent;
            Order order;

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogInformation("Payment Succeded: ", intent.Id);
                    order = await _paymentService.UpdateOrderPaymentSucceeded(intent.Id);
                    _logger.LogInformation("Order updated to payment received", order.Id);
                    break;
                
                case "payment_intent.payment_failed":
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogInformation("Payment Failed: ", intent.Id);
                     order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                    _logger.LogInformation("Order updated to payment failed", order.Id);
                    break;
            }

            //Confirms to Stripe that we have received the event or else Stripe keeps trying for days
            return new EmptyResult();
        }
    }
}