using Dto;
using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts.IServices
{
    public interface ITicketService
    {
        /// <summary>
        /// Creates a new ticket. 
        /// </summary>
        /// <param name="ticket">TicketRequestDto containing ticket details.</param>
        Guid CreateTicket(TicketRequestDto ticket);

        /// <summary>
        /// Creates a new ticket using the V2 structure (supports multiple assets).
        /// </summary>
        /// <param name="ticket">TicketRequestV2Dto containing ticket details and multiple assets.</param>
        Guid CreateTicketV2(TicketRequestV2Dto ticket);

        /// <summary> 
        /// Retrieves all tickets for a specific user.
        /// </summary>
        /// <param name="index">The starting index for pagination.</param>  
        /// <param name="limit">The maximum number of tickets to retrieve.</param>
        /// <param name="userId">The ID of the user whose tickets are to be retrieved.</param>
        /// <param name="roleId">The ID of the user whose tickets are to be retrieved.</param>
        /// 
        List<TicketDetailDto> GetAllTickets(Guid userId, TicketFilterDto filters);

        /// <summary>
        /// Retrieves a ticket by its ID.  
        /// </summary>
        /// <param name="id">The ID of the ticket to retrieve.</param>
        TicketDetailDto GetTicketById(Guid Id);

        /// <summary>
        /// Updates a ticket's fields based on user role.
        /// </summary>
        /// <param name="dto">The DTO containing updatable fields.</param>
        Guid UpdateTicket(TicketUpdateDto dto);

        /// <summary>
        /// Create Ticket for the Ariba User
        /// </summary>
        /// <param name="ticketCreateAribaDto">Dto Containing Details </param>
        Guid CreateTicketAriba(TicketCreateAribaDto ticketCreateAribaDto);

        /// <summary>
        /// Get tickets Messages and assets for the given ticket with Id                     
        /// </summary>
        /// <param name="ticketId">Ticket Id</param>                  
        List<TicketMessageDto> GetTicketMessageById(Guid ticketId, int skip, int take);

        /// <summary>
        /// Returns dashboard summary for tickets based on user role.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="role">The user's role name.</param>
        TicketDashboardSummaryDto GetDashboardSummary(Guid userId);
        /// <summary>
        /// Create ticket Message for the given ticket with Id                     
        /// </summary>
        /// <param name="ticketMessage">Ticket Message Details</param>                  
        Guid CreateTicketMessage(TicketMessageRequestDto ticketMessage);
        /// <summary>
        /// Retrieves all tickets for a specific user with pagination.
        /// </summary>
        Guid CreateTicketPR(List<TicketPRCreateDto> dto, Guid ticketId);
        /// <summary>
        /// Creates the Email Notification for the Ticket.
        /// </summary>
        void CreateEmailNotification(Ticket ticket, string emailKey, string entityType);
        ExportExcelResultDto ExportTicketDetailsExcel(Guid userId, TicketFilterDto filters);
    }
}