namespace Application.Reporters.Interfaces;

using Application.Reporters.Commands;

using Domain.Entities;

public interface ISessionReporter
{
  Task<Session> StartSessionAsync(StartSessionCommand command);
  Task EndSessionAsync(EndSessionCommand command);
}
