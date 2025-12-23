namespace RpgAI.Controllers.Dtos;

public record BrowseCampaignsRequest(
  int PageSize, 
  Guid OwnerId,
  string? SearchPhrase = null,
  Guid? Cursor = null);
