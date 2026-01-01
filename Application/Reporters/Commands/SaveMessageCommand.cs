namespace Application.Reporters.Commands;

using Domain.Entities;

public record SaveMessageCommand(Guid OwnerId, Session Session, Role Role, string Content);
