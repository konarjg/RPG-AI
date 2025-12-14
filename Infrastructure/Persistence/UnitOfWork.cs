namespace Infrastructure.Persistence;

using Domain.Ports.Persistence;

public class UnitOfWork(DatabaseContext context) : IUnitOfWork {

  public async Task SaveChangesAsync() {
    await context.SaveChangesAsync();
  }
}
