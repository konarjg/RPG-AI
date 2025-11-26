namespace Application.Exceptions;

public class AlreadyExistsException(string message) : Exception(message) {
  
}

public class AlreadyExistsException<T>(string keyName, object keyValue) : AlreadyExistsException($"{typeof(T).Name} already exists with {keyName}: {keyValue}.") {
  
} 
