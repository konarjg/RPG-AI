namespace Application.Interfaces;

using Domain.Entities;
using Dtos;

public interface IGameplayService {
  Task<Session> StartSessionAsync(StartSessionCommand command, CancellationToken cancellationToken = default);
  Task ProcessTurnAsync(ProcessTurnCommand command,
    CancellationToken cancellationToken = default);
  Task EndSessionAsync(EndSessionCommand command,
    CancellationToken cancellationToken = default);
}
