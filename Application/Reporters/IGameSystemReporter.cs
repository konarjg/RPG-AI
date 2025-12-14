namespace Application.Reporters;

using Domain.Entities;

public interface IGameSystemReporter {
  Task<GameSystem> UploadGameSystemAsync(UploadGameSystemCommand command);
}
