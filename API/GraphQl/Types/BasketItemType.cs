using Core.Entities;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.GraphQl.Types
{
    public class BasketItemType : ObjectGraphType<BasketItem>
    {
        public BasketItemType()
        {
            Field(bi => bi.Id);
            Field(bi => bi.ProductName);
            Field(bi => bi.Price);
            Field(bi => bi.Quantity);
            Field(bi => bi.PictureUrl);
            Field(bi => bi.Brand);
            Field(bi => bi.Type);
        }
    }
}
