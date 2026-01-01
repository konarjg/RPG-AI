namespace Application.Core;

using Domain.Entities;

public interface IGameMasterMessageProcessor {
  Task<Message> ProcessMessageAsync(ProcessMessageCommand command);
}
