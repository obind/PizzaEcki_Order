using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SharedLibrary;


namespace PizzaServer
{
    public class PizzaHub : Hub
    {
        public async Task getDish(string dish)
        {
            await Clients.All.SendAsync("ReceiveDish", dish);
        }

        public async Task getAssignments(Order order)
        {
            await Clients.All.SendAsync("ReceiveOrder", order);
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
