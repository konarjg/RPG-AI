namespace Application.Reporters.Interfaces;

using Commands;
using Domain.Entities;

public interface IMessageReporter {
  Task SaveMessageAsync(SaveMessageCommand command);
}
