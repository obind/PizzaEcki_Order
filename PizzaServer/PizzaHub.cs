﻿using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SharedLibrary;


namespace PizzaServer
{
    public class PizzaHub : Hub
    {
        public async Task SendDish(string dish)
        {
            await Clients.All.SendAsync("ReceiveDish", dish);
        }

        public async Task SendOrderItems(List<OrderItem> orderItems)
        {
            await Clients.All.SendAsync("ReceiveOrderItems", orderItems);
        }

    }
}
