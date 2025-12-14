namespace Domain.Ports.Infrastructure;

public interface IDateTimeProvider {
  DateTime GetCurrentDateTime();
}