using System.Threading.Tasks;
using Orleans;
using Orleans.Collections;
using OR = TTC2015.TrainBenchmark.Orleans.Railway;
using SR = TTC2015.TrainBenchmark.Railway;

namespace Grains
{
    public interface IRailwayContainerGrain : IGrainWithGuidKey, IElementEnumeratorNode<OR.IRailwayContainer>, IElementExecutor<OR.IRailwayContainer>
    {

        Task InitializeRailwayContainer();

    }
}