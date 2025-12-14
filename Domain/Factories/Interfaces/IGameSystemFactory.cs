namespace Domain.Factories.Interfaces;

using Dtos;
using Entities;
using Ports.Infrastructure.Dtos;

public interface IGameSystemFactory {
  GameSystem CreateGameSystem(CreateGameSystemCommand command);
}
