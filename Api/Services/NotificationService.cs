using Data;
using Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class NotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationResultDTO>> GetNotificationsForPeriodAsync(int greenhouseId, DateTime start, DateTime end)
    {
        var notifications = await _context.Notifications
            .Where(n => n.GreenhouseId == greenhouseId &&
                        n.Timestamp >= start &&
                        n.Timestamp <= end)
            .Select(n => new NotificationResultDTO
            {
                Id = n.Id,
                Timestamp = n.Timestamp,
                Content = n.Content
            })
            .ToListAsync();

        return notifications;
    }
}