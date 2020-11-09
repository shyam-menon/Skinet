using API.GraphQl.Types;
using Core.Entities;
using Core.Interfaces;
using GraphQL;
using GraphQL.Types;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.GraphQl.Queries
{
    public class CustomerBasketQuery : ObjectGraphType
    {
        public CustomerBasketQuery(StoreContext storeContext)
        {
            Field<CustomerBasketType>("basket", arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "id" }),
               resolve: context =>
               {
                   //return storeContext.Set<Basket>().FindAsync(context.GetArgument<string>("id"));
                   return new Basket { BasketData = "Basket contents", Id = "1", lastUpdated = DateTime.Now };
               });            
        }
    }
}
