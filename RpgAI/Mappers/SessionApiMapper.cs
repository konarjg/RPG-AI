namespace RpgAI.Mappers;

using Domain.Entities;
using RpgAI.Controllers.Dtos;

public static class SessionApiMapper
{

    public static SessionResponse ToResponse(Session session)
    {
        return new SessionResponse(session.Id, session.CampaignId, session.SessionNumber, session.Summary, session.CurrentSceneId);
    }
}
