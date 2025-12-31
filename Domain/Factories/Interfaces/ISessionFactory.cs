namespace Domain.Factories.Interfaces;

using Dtos;
using Entities;

public interface ISessionFactory {
  Session CreateSession(CreateSessionCommand command);
}
