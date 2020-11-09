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
    public class RootQuery : ObjectGraphType
    {
        //Keep adding queries to the root query
        public RootQuery(IBasketRepository basketRepository)
        {
            Field<CustomerBasketQuery>("basketQuery", resolve: context => new { });            
        }
    }
}
