namespace Domain.Factories;

using Dtos;
using Entities;
using Exceptions;
using Interfaces;
using Ports.Infrastructure;
using Ports.Persistence;

public class SessionFactory(IGuidGenerator guidGenerator, ISessionRepository sessionRepository) : ISessionFactory {

  public Session CreateSession(CreateSessionCommand command) {
    if (command.Campaign.CurrentSessionId is not null) {
      throw new AlreadyExistsException($"Campaign {command.Campaign.Id} already has a current session.");
    }

    Session session = new() {
      Id = guidGenerator.GenerateGuid(),
      CampaignId = command.Campaign.Id,
      SessionNumber = command.Campaign.LastSessionNumber + 1 ?? 1
    };
    
    sessionRepository.Add(session);
    return session;
  }
}
