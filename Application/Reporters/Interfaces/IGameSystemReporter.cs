namespace Application.Reporters.Interfaces;

using Domain.Entities;

public interface IGameSystemReporter {
  Task<GameSystem> UploadGameSystemAsync(UploadGameSystemCommand command);
}
