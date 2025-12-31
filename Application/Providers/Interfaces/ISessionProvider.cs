namespace Application.Providers.Interfaces;

using Domain.Dtos;
using Domain.Entities;
using Queries;

public interface ISessionProvider
{
  Task<Session?> GetCurrentSessionAsync(GetCurrentSessionQuery query);
  Task<Session?> GetSessionByIdAsync(GetSessionQuery query);
  Task<CursorResult<Session>> BrowseSessionsAsync(BrowseSessionsQuery query);
}
