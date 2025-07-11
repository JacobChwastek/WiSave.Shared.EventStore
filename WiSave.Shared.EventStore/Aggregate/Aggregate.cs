using System.Reflection;
using Marten.Schema;

namespace WiSave.Shared.EventStore.Aggregate;

public abstract class Aggregate<TKey> : IAggregate where TKey : AggregateRootId
{
    private readonly Dictionary<Type, MethodInfo> _eventHandlers = new();
    private readonly Queue<object> _uncommittedEvents = new();

    public TKey Id { get; set; } = null!;

    [Identity]
    public Guid AggregateId
    {
        get => Id.Id;
        set { }
    }

    public int Version { get; protected set; }

    protected Aggregate()
    {
        RegisterEventHandlers();
    }

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = _uncommittedEvents.ToArray();
        _uncommittedEvents.Clear();
        return dequeuedEvents;
    }

    protected void RaiseEvent<TEvent>(TEvent @event) where TEvent : Event<TKey>
    {
        ApplyEvent(@event);
        Enqueue(@event);
        Version++;
    }

    protected void Enqueue(object @event)
    {
        _uncommittedEvents.Enqueue(@event);
    }

    private void ApplyEvent(object @event)
    {
        var eventType = @event.GetType();
        
        if (_eventHandlers.TryGetValue(eventType, out var handler))
        {
            handler.Invoke(this, [@event]);
        }
        else if (HasBaseEventHandler(eventType, out var baseHandler))
        {
            baseHandler!.Invoke(this, [@event]);
        }
    }

    private bool HasBaseEventHandler(Type eventType, out MethodInfo? handler)
    {
        handler = null;
        
        foreach (var baseType in GetBaseTypes(eventType))
        {
            if (_eventHandlers.TryGetValue(baseType, out handler))
                return true;
        }
        
        return false;
    }

    private IEnumerable<Type> GetBaseTypes(Type type)
    {
        var current = type.BaseType;
        while (current != null && current != typeof(object))
        {
            yield return current;
            current = current.BaseType;
        }
    }

    private void RegisterEventHandlers()
    {
        var aggregateType = GetType();
        var methods = aggregateType
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(m => m.Name == "Apply" && m.GetParameters().Length == 1);

        foreach (var method in methods)
        {
            var eventType = method.GetParameters()[0].ParameterType;
            _eventHandlers[eventType] = method;
        }
    }
    
    public void Apply(object @event)
    {
        ApplyEvent(@event);
        Version++;
    }
}