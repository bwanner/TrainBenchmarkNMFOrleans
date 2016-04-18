using NMF.Models;

namespace TTC2015.TrainBenchmark.Railway
{
    public interface ITrainBenchmarkModelElement<TSerializable> : IModelElement, ISerializableModelElement<TSerializable>
    {
         
    }
}