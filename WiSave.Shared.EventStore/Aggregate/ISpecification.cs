namespace WiSave.Shared.EventStore.Aggregate;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
    void Check(T candidate);
}