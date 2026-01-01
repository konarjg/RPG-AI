namespace Domain.Ports.Infrastructure;

using Entities;

public interface IMessageBroadcaster {
  Task BroadcastMessageAsync(Message message);
}
