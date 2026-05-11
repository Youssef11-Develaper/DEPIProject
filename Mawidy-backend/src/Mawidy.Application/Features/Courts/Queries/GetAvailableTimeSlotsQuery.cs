using MediatR;
using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Application.Features.Courts.Queries;

public record GetAvailableTimeSlotsQuery(Guid CourtId, Guid DepartmentId, DateTime Date) : IRequest<List<TimeSpan>>;


