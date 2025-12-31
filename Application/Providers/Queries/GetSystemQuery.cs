namespace Application.Providers.Queries;

public record GetSystemQuery(Guid Id, Guid? OwnerId = null);
