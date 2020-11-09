using Core.Entities;
using Core.Interfaces;
using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.GraphQl.Types
{
    public class CustomerBasketComplexType : ObjectGraphType<CustomerBasket>
    {
        public CustomerBasketComplexType(IBasketRepository repo)
        {
            Field(m => m.Id);
            Field(m => m.DeliveryMethodId);
            Field(m => m.ShippingPrice);
            Field<ListGraphType<BasketItemType>>("items", arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "id" })
                , resolve: context => { return repo.GetBasketAsync(context.GetArgument<string>("id")).Result.Items; });
        }
    }
}
