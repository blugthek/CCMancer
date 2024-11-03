using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public delegate void EventCallback<T1, T2, T3>(T1 param1, T2 param2, T3 param3);

public class EventManager
{
    private GlobalEventContextManager _contextManager = new GlobalEventContextManager();

    // Dictionary to hold event names and their corresponding event handlers
    // private Dictionary<string, Action<Action<object>, object[]>> _eventDictionary =
    //     new Dictionary<string, Action<Action<object>, object[]>>();
    private Dictionary<string, EventEntry> _eventDictionary = new Dictionary<string, EventEntry>();

    private readonly Dictionary<string, Delegate> _eventDelegate = new Dictionary<string, Delegate>();

    private Dictionary<string, List<Action<object>>> _eventListeners =
        new Dictionary<string, List<Action<object>>>();

    // Register an event with a handler
    // Register an event (can be regular or conditional)
    public void RegisterEvent(string eventName, Action<object[]> callback, Func<bool> condition = null)
    {
        if (!_eventDictionary.ContainsKey(eventName))
        {
            _eventDictionary[eventName] = new EventEntry
            {
                Callback = callback,
                Condition = condition
            };
        }
        else
        {
            // Combine callbacks if the event already exists
            _eventDictionary[eventName].Callback += callback;
        }
    }

    // Register an event listener
    public void AddEventListener(string eventName, Action<object> listener)
    {
        if (!_eventListeners.ContainsKey(eventName))
        {
            _eventListeners[eventName] = new List<Action<object>>();
        }

        _eventListeners[eventName].Add(listener);
    }

    // Register an event with a handler
    public void RegisterDelegateEvent<T1, T2, T3>(string eventName, EventCallback<T1, T2, T3> eventHandler)
    {
        if (!_eventDelegate.ContainsKey(eventName))
        {
            _eventDelegate.Add(eventName, eventHandler);
        }
        else
        {
            _eventDelegate[eventName] = Delegate.Combine(_eventDelegate[eventName], eventHandler);
        }
    }

    public void TriggerDelegateEvent<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
    {
        if (!_eventDelegate.ContainsKey(eventName)) return;
        var callback = _eventDelegate[eventName] as EventCallback<T1, T2, T3>;
        callback?.Invoke(param1, param2, param3);
    }

    // Trigger the event with parameters and a callback
    // Trigger an event manually
    public void TriggerEvent(string eventName, Action<object> callback, params object[] parameters)
    {
        if (!_eventDictionary.ContainsKey(eventName)) return;

        // Prepend the callback to the parameters array
        var fullParameters = new object[parameters.Length + 1];
        fullParameters[0] = callback;
        Array.Copy(parameters, 0, fullParameters, 1, parameters.Length);

        var eventEntry = _eventDictionary[eventName];

        // Check condition, if present
        if (eventEntry.Condition == null || eventEntry.Condition())
        {
            eventEntry.Callback.Invoke(fullParameters);
        }
    }


    // Trigger the event with parameters and a callback and wait until it is done before executing the rest of the code
    public async Task TriggerEventAsync(string eventName, Action<object> callback, params object[] parameters)
    {
        if (_eventDictionary.ContainsKey(eventName))
        {
            var entry = _eventDictionary[eventName];
            if (entry.Condition == null || entry.Condition())
            {
                // Combine the callback with the parameters array
                var fullParameters = new object[] { callback }.Concat(parameters).ToArray();

                await Task.Run(() => entry.Callback?.Invoke(fullParameters));
                // Task.Yield();
            }
        }
    }
    // Trigger an event asynchronously
    // public async Task TriggerEventAsync(string eventName, object[] callback, params object[] parameters)
    // {
    //     if (_eventDictionary.ContainsKey(eventName))
    //     {
    //         var entry = _eventDictionary[eventName];
    //         if (entry.Condition == null || entry.Condition())
    //         {
    //             await Task.Run(() => InvokeMiddlewarePipeline(eventName, entry.Callback, parameters));
    //         }
    //     }
    // }

    // Trigger the event with parameters as a background Task
    // Trigger an event as a thread

    private readonly Dictionary<string, Task> _ongoingTasks = new Dictionary<string, Task>();
    private readonly object _taskLock = new object();

    public Task TriggerEventAsThread(string eventName, Action<object> callback, params object[] parameters)
    {
        if (_eventDictionary.ContainsKey(eventName))
        {
            lock (_taskLock)
            {
                // Check if an ongoing task exists for this event
                if (_ongoingTasks.ContainsKey(eventName) && !_ongoingTasks[eventName].IsCompleted)
                {
                    return _ongoingTasks[eventName]; // Return the ongoing task
                }

                // Create a new task to handle the event
                var newTask = Task.Run(() =>
                {
                    var entry = _eventDictionary[eventName];

                    if (entry.Condition == null || entry.Condition())
                    {
                        // Combine the callback with the parameters array
                        var fullParameters = new object[] { callback }.Concat(parameters).ToArray();

                        // Call the event's callback with the parameters
                        entry.Callback?.Invoke(fullParameters);
                        // callback?.Invoke("Done");
                    }
                });

                // Store the task in the dictionary
                _ongoingTasks[eventName] = newTask;

                return newTask;
            }
        }

        return Task.CompletedTask; // Return a completed task if the event doesn't exist
    }
    // Trigger an event in a separate thread
    // public Task TriggerEventAsThread(string eventName, Action<object> callback, params object[] parameters)
    // {
    //     if (_eventDictionary.ContainsKey(eventName))
    //     {
    //         lock (_taskLock)
    //         {
    //             if (_ongoingTasks.ContainsKey(eventName) && !_ongoingTasks[eventName].IsCompleted)
    //             {
    //                 return _ongoingTasks[eventName];
    //             }
    //
    //             var newTask = Task.Run(() =>
    //             {
    //                 var entry = _eventDictionary[eventName];
    //                 if (entry.Condition == null || entry.Condition())
    //                 {
    //                     var fullParameters = new object[] { callback }.Concat(parameters).ToArray();
    //                     InvokeMiddlewarePipeline(eventName, entry.Callback, fullParameters);
    //                 }
    //             });
    //
    //             _ongoingTasks[eventName] = newTask;
    //
    //             return newTask;
    //         }
    //     }
    //
    //     return Task.CompletedTask;
    // }


    // Unregister an event handler
    public void UnregisterEvent(string eventName, Action<object[]> callback)
    {
        if (!_eventDictionary.ContainsKey(eventName)) return;

        // Get the event entry
        var eventEntry = _eventDictionary[eventName];

        // Unsubscribe the callback
        eventEntry.Callback -= callback;
        _eventDictionary.Remove(eventName);

        // Remove listeners associated with the event if needed
        if (_eventListeners.ContainsKey(eventName))
        {
            _eventListeners.Remove(eventName);
        }
    }


    // Register middleware for an event
    // public void UseMiddleware(string eventName, Func<Action<object[]>, Action<object[]>, Action<object[]>> middleware)
    // {
    //     if (_eventDictionary.ContainsKey(eventName))
    //     {
    //         _eventDictionary[eventName].MiddlewarePipeline.Add(middleware);
    //     }
    // }

    // Invoke middleware pipeline
    // private void InvokeMiddlewarePipeline(string eventName, Action<object[]> finalHandler, object[] parameters)
    // {
    //     var entry = _eventDictionary[eventName];
    //     var pipeline = entry.MiddlewarePipeline;
    //     Action<object[]> next = finalHandler;
    //
    //     for (int i = pipeline.Count - 1; i >= 0; i--)
    //     {
    //         Func<Action<object[]>, Action<object[]>, Action<object[]>> currentMiddleware = pipeline[i];
    //         var currentNext = next;
    //         next = (paramsArray) => currentMiddleware(paramsArray, currentNext, finalHandler);
    //     }
    //
    //     next(parameters);
    // }

    // Periodically check all conditional events and trigger them if conditions are met
    public void UpdateConditionalEvents()
    {
        foreach (var kvp in _eventDictionary)
        {
            var entry = kvp.Value;
            if (entry.Condition != null && entry.Condition())
            {
                entry.Callback?.Invoke(null);
            }
        }
    }

    // Schedule an event to be triggered after a delay
    public void ScheduleEvent<T>(string eventName, T delay, Action<object> parameters)
    {
        TimeSpan timeSpan;

        switch (delay)
        {
            case int intDelay:
                timeSpan = TimeSpan.FromMilliseconds(intDelay);
                break;
            case float floatDelay:
                timeSpan = TimeSpan.FromMilliseconds((int)floatDelay);
                break;
            case double doubleDelay:
                timeSpan = TimeSpan.FromMilliseconds((int)doubleDelay);
                break;
            case TimeSpan timeSpanDelay:
                timeSpan = timeSpanDelay;
                break;
            default:
                throw new ArgumentException("Unsupported delay type");
                // return;
        }

        // Task.Delay(timeSpan).ContinueWith(_ => TriggerEventAsync(eventName, parameters));
        // Task.Yield();
        Task.Run(async () =>
        {
            await Task.Delay(timeSpan);
            await TriggerEventAsync(eventName, parameters);
        });
    }

    // Schedule an event to be triggered at a specific time
    public async Task ScheduleEventAt(string eventName, DateTime triggerTime, Action<object> parameters)
    {
        var delay = triggerTime - DateTime.Now;
        if (delay.TotalMilliseconds <= 0)
        {
            await TriggerEventAsync(eventName, parameters);
        }
        else
        {
            ScheduleEvent(eventName, delay, parameters);
        }
    }

    // Class to hold event data
    private class EventEntry
    {
        public Action<object[]> Callback;
        public Func<bool> Condition;
        public List<Func<Action<object[]>, Action<object[]>, Action<object[]>>> MiddlewarePipeline = new();
    }

    // Register context through EventManager
    public void RegisterContext<T>(string contextName, T context)
    {
        _contextManager.RegisterContext(contextName, context);
    }

    // Get context through EventManager
    public T GetContext<T>(string contextName)
    {
        return _contextManager.GetContext<T>(contextName);
    }
}
