namespace WiSave.Shared.EventStore.Projections;

public interface IProjection
{
    void Apply(object @event);
}

public interface IProjection<in TEvent>: IProjection where TEvent : class
{
    void Apply(TEvent @event);

    void IProjection.Apply(object @event)
    {
        if (@event is TEvent typedEvent)
            Apply(typedEvent);
    }
}

public interface ITrackLastProcessedPosition
{
    public ulong LastProcessedPosition { get; set; }
}

public interface IVersionedProjection: IProjection, ITrackLastProcessedPosition;