namespace Domain.Exceptions;

public class SceneMessageLimitException(string message = "Scene message limit reached. Start a new scene to save this message.") : Exception(message) {
  
}
