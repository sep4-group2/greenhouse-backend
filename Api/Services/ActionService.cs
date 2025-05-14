using Data;
using Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ActionService(AppDbContext context)
{
    public async Task<List<ActionResultDTO>> GetActionsForPeriodAsync(int greenhouseId, DateTime start, DateTime end)
    {
        var actions = await context.Actions
            .Where(a => a.GreenhouseId == greenhouseId &&
                        a.Timestamp >= start &&
                        a.Timestamp <= end)
            .Select(a => new ActionResultDTO
            {
                Type = a.Type,
                Status = a.Status,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return actions;
    }
}