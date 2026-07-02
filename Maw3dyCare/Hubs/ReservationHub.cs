using Microsoft.AspNetCore.SignalR;

namespace Maw3dyCare.Hubs
{
    public class ReservationHub : Hub
    {
        public async Task JoinReservation(string reservationId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                reservationId);
        }
    }
}