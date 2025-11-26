namespace Domain.Ports.Infrastructure;

public interface IUnitOfWork {
  Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
