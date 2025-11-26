namespace Application.Exceptions;

public class NotFoundException(string message) : Exception(message) {
  
}

public class NotFoundException<T>(string keyName, object keyValue) : NotFoundException($"{typeof(T).Name} with {keyName}: {keyValue} does not exist.") {
  public NotFoundException(Guid id) : this("unique id", id){
        
  }
}
