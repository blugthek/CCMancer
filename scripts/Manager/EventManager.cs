using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public delegate void EventCallback<T1, T2, T3>(T1 param1, T2 param2, T3 param3);

public class EventManager
{
    private GlobalEventContextManager _contextManager = new GlobalEventContextManager();
    private Dictionary<string, EventEntry> _eventDictionary = new Dictionary<string, EventEntry>();

    private readonly Dictionary<string, Delegate> _eventDelegate = new Dictionary<string, Delegate>();

    private Dictionary<string, List<Action<object>>> _eventListeners =
        new Dictionary<string, List<Action<object>>>();

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
            _eventDictionary[eventName].Callback += callback;
        }
    }

    public void AddEventListener(string eventName, Action<object> listener)
    {
        if (!_eventListeners.ContainsKey(eventName))
        {
            _eventListeners[eventName] = new List<Action<object>>();
        }

        _eventListeners[eventName].Add(listener);
    }

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

    public void TriggerEvent(string eventName, Action<object> callback, params object[] parameters)
    {
        if (!_eventDictionary.ContainsKey(eventName)) return;

        var fullParameters = new object[parameters.Length + 1];
        fullParameters[0] = callback;
        Array.Copy(parameters, 0, fullParameters, 1, parameters.Length);

        var eventEntry = _eventDictionary[eventName];

        if (eventEntry.Condition == null || eventEntry.Condition())
        {
            eventEntry.Callback.Invoke(fullParameters);
        }
    }

    public async Task TriggerEventAsync(string eventName, Action<object> callback, params object[] parameters)
    {
        if (_eventDictionary.ContainsKey(eventName))
        {
            var entry = _eventDictionary[eventName];
            if (entry.Condition == null || entry.Condition())
            {
                var fullParameters = new object[] { callback }.Concat(parameters).ToArray();

                await Task.Run(() => entry.Callback?.Invoke(fullParameters));
            }
        }
    }

    private readonly Dictionary<string, Task> _ongoingTasks = new Dictionary<string, Task>();
    private readonly object _taskLock = new object();

    public Task TriggerEventAsThread(string eventName, Action<object> callback, params object[] parameters)
    {
        if (_eventDictionary.ContainsKey(eventName))
        {
            lock (_taskLock)
            {
                if (_ongoingTasks.ContainsKey(eventName) && !_ongoingTasks[eventName].IsCompleted)
                {
                    return _ongoingTasks[eventName];
                }

                var newTask = Task.Run(() =>
                {
                    var entry = _eventDictionary[eventName];

                    if (entry.Condition == null || entry.Condition())
                    {
                        var fullParameters = new object[] { callback }.Concat(parameters).ToArray();

                        entry.Callback?.Invoke(fullParameters);
                    }
                });

                _ongoingTasks[eventName] = newTask;

                return newTask;
            }
        }

        return Task.CompletedTask; 
    }

    public void UnregisterEvent(string eventName, Action<object[]> callback)
    {
        if (!_eventDictionary.ContainsKey(eventName)) return;

        var eventEntry = _eventDictionary[eventName];

        eventEntry.Callback -= callback;
        _eventDictionary.Remove(eventName);

        if (_eventListeners.ContainsKey(eventName))
        {
            _eventListeners.Remove(eventName);
        }
    }

    // public void UseMiddleware(string eventName, Func<Action<object[]>, Action<object[]>, Action<object[]>> middleware)
    // {
    //     if (_eventDictionary.ContainsKey(eventName))
    //     {
    //         _eventDictionary[eventName].MiddlewarePipeline.Add(middleware);
    //     }
    // }

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
        }
        Task.Run(async () =>
        {
            await Task.Delay(timeSpan);
            await TriggerEventAsync(eventName, parameters);
        });
    }

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

    private class EventEntry
    {
        public Action<object[]> Callback;
        public Func<bool> Condition;
        public List<Func<Action<object[]>, Action<object[]>, Action<object[]>>> MiddlewarePipeline = new();
    }

    public void RegisterContext<T>(string contextName, T context)
    {
        _contextManager.RegisterContext(contextName, context);
    }

    public T GetContext<T>(string contextName)
    {
        return _contextManager.GetContext<T>(contextName);
    }

    internal void HandleInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.Pressed)
        {
            var keycode = eventKey.Keycode;
            var eventName = keycode.ToString();
            if (!_eventDictionary.ContainsKey(eventName))
            {
                return;
            }
            else
            {
            }
        }
    }
}
