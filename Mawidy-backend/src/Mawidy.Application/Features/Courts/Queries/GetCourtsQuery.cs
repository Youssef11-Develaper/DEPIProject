using MediatR;
using Mawidy.Application.DTOs.Courts;
using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Application.Features.Courts.Queries;

public record GetCourtsQuery(string? FilterType, string? SearchQuery) : IRequest<List<CourtDto>>;


