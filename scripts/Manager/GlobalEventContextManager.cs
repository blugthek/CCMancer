using System.Collections.Generic;

public class GlobalEventContextManager
{
    private Dictionary<string, object> _contextDictionary = new Dictionary<string, object>();

    // Register a context with a specific type
    public void RegisterContext<T>(string contextName, T context)
    {
        if (_contextDictionary.ContainsKey(contextName))
        {
            _contextDictionary[contextName] = context;
        }
        else
        {
            _contextDictionary.Add(contextName, context);
        }
    }

    // Retrieve a context with a specific type
    public T GetContext<T>(string contextName)
    {
        if (_contextDictionary.ContainsKey(contextName))
        {
            return (T)_contextDictionary[contextName];
        }
        else
        {
            throw new KeyNotFoundException($"Context '{contextName}' not found.");
        }
    }

    // Try to retrieve a context without throwing an exception
    public bool TryGetContext<T>(string contextName, out T? context)
    {
        if (_contextDictionary.ContainsKey(contextName))
        {
            context = (T)_contextDictionary[contextName];
            return true;
        }
        else
        {
            context = default(T);
            return false;
        }
    }

}