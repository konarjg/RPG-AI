namespace Application;

using Configuration;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Dtos;
using Exceptions;
using Interfaces;

public class GameplayService(ICampaignRepository campaignRepository, IEmbeddingRepository embeddingRepository, IEventBroadcaster eventBroadcaster, IRuleEngine ruleEngine, IAiClient aiClient, ISchemaProvider schemaProvider, IUnitOfWork unitOfWork, AiConfiguration aiConfiguration) : IGameplayService {
  
  public async Task<Session> StartSessionAsync(StartSessionCommand command,
    CancellationToken cancellationToken = default) {
    
    Campaign? campaign = await campaignRepository.GetByIdAsync(command.CampaignId,  command.UserId, cancellationToken);

    if (campaign is null) {
      throw new NotFoundException<Campaign>(command.CampaignId);
    }

    Session? activeSession = campaign.Sessions.FirstOrDefault(s => s.Status is SessionStatus.Active);

    if (activeSession is not null) {
      throw new AlreadyExistsException<Session>(nameof(activeSession.Status), activeSession.Status.ToString());
    }
    
    Session session = new() {
      Title = command.Title,
      CampaignId = campaign.Id,
      SessionNumber = campaign.Sessions.Any() ? campaign.Sessions.Max(s => s.SessionNumber) + 1 : 1
    };
    
    campaign.Sessions.Add(session);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return session;
  }

  public async Task ProcessTurnAsync(ProcessTurnCommand command,
    CancellationToken cancellationToken = default) {
    
    Campaign? campaign = await campaignRepository.GetByIdAsync(command.CampaignId, command.UserId, cancellationToken);

    if (campaign is null) {
      throw new NotFoundException<Campaign>(command.CampaignId);
    }

    Session? activeSession = campaign.Sessions.FirstOrDefault(s => s.Status is SessionStatus.Active);

    if (activeSession is null) {
      throw new NotFoundException<Session>(nameof(activeSession.Status), SessionStatus.Active);
    }
    
    CursorResult<CampaignEvent> recentEvents = await campaignRepository.GetEventsAsync(campaign.Id,activeSession.Id, command.UserId, null,aiConfiguration.RecentEventsPageSize, cancellationToken);
    RelevantContext relevantContext = await RetrieveRelevantContextAsync(campaign, command.GameMasterMessage, recentEvents.Items.ToList(), cancellationToken);
    Character character = campaign.Characters.FirstOrDefault(c => c.Id == command.NextCharacterId) ?? throw new NotFoundException<Character>(command.NextCharacterId);

    ReactionContext context = new(campaign.GameSystem.CharacterSheetSchema, character,relevantContext,recentEvents,command.GameMasterMessage);
    AiActionResponse response = await GetEventReactionAsync(context, cancellationToken);
    
    CampaignEvent currentEvent = new() {
      CampaignId = campaign.Id,
      SessionId = activeSession.Id,
      OccuredAt = command.SentAt,
      Content = command.GameMasterMessage,
      ContentEmbedding = await aiClient.EmbedTextAsync(command.GameMasterMessage,cancellationToken)
    };

    CampaignEvent responseEvent = new() {
      CampaignId = campaign.Id,
      SessionId = activeSession.Id,
      CharacterId = character.Id,
      OccuredAt = DateTime.UtcNow,
      Content = response.Content,
      ContentEmbedding = await aiClient.EmbedTextAsync(response.Content,cancellationToken),
    };

    await eventBroadcaster.BroadcastEventAsync(responseEvent,cancellationToken);
    
    campaign.CampaignEvents.Add(currentEvent);
    campaign.CampaignEvents.Add(responseEvent);

    await unitOfWork.SaveChangesAsync(cancellationToken);
  }

  public async Task EndSessionAsync(EndSessionCommand command,
    CancellationToken cancellationToken = default) {
    
    Campaign? campaign = await campaignRepository.GetByIdAsync(command.CampaignId, command.UserId, cancellationToken);

    if (campaign is null) {
      throw new NotFoundException<Campaign>(command.CampaignId);
    }

    Session? activeSession = campaign.Sessions.FirstOrDefault(s => s.Status is SessionStatus.Active);

    if (activeSession is null) {
      throw new NotFoundException<Session>(nameof(activeSession.Status), SessionStatus.Active);
    }

    string summary = await aiClient.SummarizeSessionAsync(activeSession, cancellationToken);

    activeSession.Summary = summary;
    activeSession.SummaryEmbedding = await aiClient.EmbedTextAsync(summary, cancellationToken);
    activeSession.Status = SessionStatus.Completed;

    await unitOfWork.SaveChangesAsync(cancellationToken);
  }

  private async Task<AiActionResponse> GetEventReactionAsync(ReactionContext context, CancellationToken cancellationToken = default) {
    AiActionRequest actionRequest = new() {
      Character = context.Character,
      CurrentEvent = context.CurrentEvent,
      RecentEvents = context.RecentEvents.Items,
      RelevantSessions = context.RelevantContext.RelevantSessionSummaries,
      RelevantRules = context.RelevantContext.RelevantRules,
      RelevantPastEvents = context.RelevantContext.RelevantEvents
    };
    
    AiActionResponse response = await aiClient.ReactToEventAsync(actionRequest,cancellationToken);

    if (response.Action is not null) {
      ruleEngine.ExecuteRule(context.Character,response.Action);
      schemaProvider.ValidateWithSchema(context.Character.State,context.CharacterSheetSchema);
    }

    return response;
  }

  private async Task<RelevantContext> RetrieveRelevantContextAsync(Campaign campaign, string currentEvent, List<CampaignEvent> recentEvents,
    CancellationToken cancellationToken = default) {

    float[] query = await aiClient.GenerateRetrievalQueryEmbeddingAsync(currentEvent,recentEvents,cancellationToken);

    return new RelevantContext(
      await embeddingRepository.FindRelevantRulesAsync(campaign.GameSystemId, query, aiConfiguration.RulesRetrievalLimit, cancellationToken),
      await embeddingRepository.FindRelevantSessionSummariesAsync(campaign.Id, query, aiConfiguration.SessionSummariesRetrievalLimit,cancellationToken),
      await embeddingRepository.FindRelevantHistoryAsync(campaign.Id,query, aiConfiguration.EventsRetrievalLimit,cancellationToken)
    );
  }

  private record ReactionContext(Dictionary<string, object> CharacterSheetSchema, Character Character,RelevantContext RelevantContext,CursorResult<CampaignEvent> RecentEvents,string CurrentEvent);

  private record RelevantContext(List<GameRuleChunk> RelevantRules, List<Session> RelevantSessionSummaries, List<CampaignEvent> RelevantEvents);
}