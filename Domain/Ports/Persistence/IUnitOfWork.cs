namespace Domain.Ports.Persistence;

public interface IUnitOfWork {
  Task SaveChangesAsync();
}
