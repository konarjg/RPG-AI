namespace Domain.Factories;

using Dtos;
using Entities;
using Exceptions;
using Interfaces;
using Ports.Infrastructure;
using Ports.Persistence;

public class MessageFactory(IGuidGenerator guidGenerator, IDateTimeProvider dateTimeProvider) : IMessageFactory {

  public Message CreateMessage(CreateMessageCommand command) {
    if (command.Scene.TotalMessages == 20) {
      throw new SceneMessageLimitException();
    }

    Message message = new() {
      Id = guidGenerator.GenerateGuid(),
      SceneId = command.Scene.Id,
      SentAt = dateTimeProvider.GetCurrentDateTime(),
      Content = command.Content,
      Sender = command.Sender
    };

    command.Scene.Messages.Add(message);
    command.Scene.TotalMessages++;
    
    return message;
  }
}
