namespace Infrastructure.Infrastructure;

using Domain.Ports.Infrastructure;

public class GuidGenerator : IGuidGenerator {

  public Guid GeneradeGuid() {
    return Guid.CreateVersion7();
  }
}
