namespace Domain.Dtos;

using Entities;

public record CreateMessageCommand(Scene Scene, Role Sender, string Content);
