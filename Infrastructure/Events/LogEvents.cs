using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Events
{
    public class LogEvents
    {
        public static EventId OrderCreation = new EventId(10001, "OrderCreation");
    }
}
