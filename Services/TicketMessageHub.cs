using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using Microsoft.AspNetCore.SignalR;

public class TicketMessageHub : Hub
{
    private readonly ITicketService _ticketService;

    public TicketMessageHub(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public async Task BroadcastLatestMessage(Guid ticketId)
    {
        List<TicketMessageDto> result = _ticketService.GetTicketMessageById(ticketId, 0, 1);
        if (result != null)
        {
            await Clients.Group(ticketId.ToString()).SendAsync("ReceiveMessage", result);
        }
    }

    public override async Task OnConnectedAsync()
    {
        string? ticketId = Context.GetHttpContext()?.Request.Query["ticketId"];
        if (!string.IsNullOrEmpty(ticketId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? ticketId = Context.GetHttpContext()?.Request.Query["ticketId"];
        if (!string.IsNullOrEmpty(ticketId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
