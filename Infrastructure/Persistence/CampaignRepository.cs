namespace Infrastructure.Persistence;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CampaignRepository(DatabaseContext context) : ICampaignRepository
{

  public async Task<CursorResult<Campaign>> BrowseAsync(int pageSize,
    Guid ownerId,
    string? searchPhrase = null,
    Guid? cursor = null)
  {

    IQueryable<Campaign> query = context.Campaigns.Include(c => c.GameSystem)
      .Where(c => c.OwnerId == ownerId)
      .Where(c => searchPhrase == null || EF.Functions.ILike(c.Title, $"%{searchPhrase}%"))
      .OrderByDescending(c => c.Id);

    if (cursor is not null)
    {
      query = query.Where(c => c.Id < cursor);
    }

    List<Campaign> items = await query.Take(pageSize + 1).ToListAsync();

    bool hasMoreItems = items.Count > pageSize;
    Guid? nextCursor = null;

    if (hasMoreItems)
    {
      items.RemoveAt(items.Count - 1);
      nextCursor = items.LastOrDefault()?.Id;
    }

    return new CursorResult<Campaign>(items, nextCursor, hasMoreItems);
  }

  public async Task<Campaign?> GetCampaignDetailsAsync(Guid id, Guid ownerId)
  {
    return await context.Campaigns.Include(c => c.GameSystem)
      .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
  }

  public async Task<CursorResult<Character>> BrowseCharactersAsync(int pageSize,
    Guid campaignId,
    Guid ownerId,
    Guid? cursor = null)
  {

    IQueryable<Character> query = context.Characters
      .Where(ch => ch.Campaign.OwnerId == ownerId && ch.CampaignId == campaignId)
      .OrderBy(ch => ch.Id);

    if (cursor is not null)
    {
      query = query.Where(ch => ch.Id > cursor);
    }

    List<Character> items = await query.Take(pageSize + 1).ToListAsync();

    bool hasMoreItems = items.Count > pageSize;
    Guid? nextCursor = null;

    if (hasMoreItems)
    {
      items.RemoveAt(items.Count - 1);
      nextCursor = items.LastOrDefault()?.Id;
    }

    return new CursorResult<Character>(items, nextCursor, hasMoreItems);
  }

  public async Task<Character?> GetCharacterByIdAsync(Guid campaignId,
    Guid characterId,
    Guid ownerId)
  {

    return await context.Characters
      .FirstOrDefaultAsync(ch => ch.Id == characterId &&
                            ch.CampaignId == campaignId &&
                            ch.Campaign.OwnerId == ownerId);
  }

  public void AddCampaign(Campaign campaign)
  {
    context.Campaigns.Add(campaign);
  }

  public void AddCharacter(Character character)
  {
    context.Characters.Add(character);
  }

  public async Task<Session?> GetCurrentSessionAsync(Guid id, Guid ownerId)
  {
    return await context.Campaigns.Where(c => c.Id == id && c.OwnerId == ownerId)
    .Select(c => c.CurrentSession)
    .FirstOrDefaultAsync();
  }
}