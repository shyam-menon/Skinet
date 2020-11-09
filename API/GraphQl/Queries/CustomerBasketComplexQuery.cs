using API.GraphQl.Types;
using Core.Interfaces;
using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.GraphQl.Queries
{
    public class CustomerBasketComplexQuery : ObjectGraphType
    {
        public CustomerBasketComplexQuery(IBasketRepository repo)
        {
            Field<CustomerBasketComplexType>("basketitems", arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "id" }),
               resolve: context =>
               {                   
                   return repo.GetBasketAsync(context.GetArgument<string>("id"));
               });
        }
    }
}
