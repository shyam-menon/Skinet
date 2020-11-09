using Core.Entities;
using GraphQL.Types;

namespace API.GraphQl.Types
{
    public class CustomerBasketType : ObjectGraphType<Basket>
    {
        public CustomerBasketType()
        {
            Field(m => m.Id);
            Field(m => m.BasketData);
            Field(m => m.lastUpdated);           
        }
    }
}
