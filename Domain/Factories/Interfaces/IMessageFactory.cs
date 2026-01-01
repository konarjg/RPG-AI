namespace Domain.Factories.Interfaces;

using Dtos;
using Entities;

public interface IMessageFactory {
  Message CreateMessage(CreateMessageCommand command);
}
