namespace TTC2015.TrainBenchmark.Railway
{
    public interface ISerializableModelElement<TTransferable>
    {
        TTransferable ToSerializableModelElement();
    }

    public interface IIncrementalizableModelElement<TIncrementalizable>
    {
        TIncrementalizable ToIncrementalModelElement();
    }
}