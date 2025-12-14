namespace Infrastructure.Infrastructure;

using Domain.Ports.Infrastructure;

public class DateTimeProvider : IDateTimeProvider {

  public DateTime GetCurrentDateTime() {
    return DateTime.UtcNow;
  }
}
