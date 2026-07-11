using Microsoft.AspNetCore.SignalR;

namespace Mawidy.API.Hubs.Hospitals
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
