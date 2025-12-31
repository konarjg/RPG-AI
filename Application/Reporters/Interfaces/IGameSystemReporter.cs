namespace Application.Reporters.Interfaces;

using Application.Reporters.Commands;

using Domain.Entities;

public interface IGameSystemReporter
{
  Task<GameSystem> UploadGameSystemAsync(UploadGameSystemCommand command);
}
