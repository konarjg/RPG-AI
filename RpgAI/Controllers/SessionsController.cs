namespace RpgAI.Controllers;

using Application.Providers.Interfaces;
using Application.Providers.Queries;
using Application.Reporters.Commands;
using Application.Reporters.Interfaces;
using Domain.Dtos;
using Domain.Entities;
using Domain.Exceptions;
using Dtos;
using Microsoft.AspNetCore.Mvc;
using RpgAI.Mappers;

[ApiController]
[Route(SessionRoutes.Base)]
public class SessionsController(ISessionReporter sessionReporter, ISessionProvider sessionProvider) : ControllerBase
{

    [HttpGet(SessionRoutes.Get)]
    [ActionName(nameof(GetAsync))]
    public async Task<ActionResult<SessionResponse>> GetAsync(Guid campaignId, Guid id)
    {
        Session? session = await sessionProvider.GetSessionByIdAsync(new GetSessionQuery(campaignId, id, Guid.AllBitsSet));

        if (session is null)
        {
            throw new NotFoundException<Session>(id);
        }

        return Ok(SessionApiMapper.ToResponse(session));
    }

    [HttpGet(SessionRoutes.Current)]
    public async Task<ActionResult<SessionResponse>> GetCurrentAsync(Guid campaignId)
    {
        Session? session = await sessionProvider.GetCurrentSessionAsync(new GetCurrentSessionQuery(campaignId, Guid.AllBitsSet));

        if (session is null)
        {
            throw new NotFoundException<Session>("CampaignId", campaignId);
        }

        return Ok(SessionApiMapper.ToResponse(session));
    }

    [HttpGet(SessionRoutes.Browse)]
    public async Task<ActionResult<CursorResult<SessionResponse>>> BrowseAsync(Guid campaignId, [FromQuery] BrowseSessionsRequest request)
    {
        CursorResult<Session> sessions = await sessionProvider.BrowseSessionsAsync(new BrowseSessionsQuery(campaignId, Guid.AllBitsSet, request.PageSize, request.Cursor));

        return Ok(new CursorResult<SessionResponse>(
          sessions.Items.Select(SessionApiMapper.ToResponse).ToList(),
          sessions.NextCursor,
          sessions.HasMoreItems));
    }

    [HttpPost(SessionRoutes.Start)]
    public async Task<ActionResult<SessionResponse>> StartAsync(Guid campaignId, [FromBody] StartSessionRequest request)
    {
        Session session = await sessionReporter.StartSessionAsync(new StartSessionCommand(campaignId, Guid.AllBitsSet));

        return CreatedAtAction(nameof(GetAsync), new { campaignId = session.CampaignId, id = session.Id }, SessionApiMapper.ToResponse(session));
    }

    [HttpPost(SessionRoutes.End)]
    public async Task<ActionResult> EndAsync(Guid campaignId, Guid id)
    {
        await sessionReporter.EndSessionAsync(new EndSessionCommand(campaignId, Guid.AllBitsSet));

        return NoContent();
    }
}
