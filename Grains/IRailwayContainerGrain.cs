using Orleans.Collections;
using TTC2015.TrainBenchmark.Railway;

namespace Grains
{
    public interface IRailwayContainerGrain : IElementEnumeratorNode<IRailwayContainer>, IElementExecutor<IRailwayContainer>
    {
         
    }
}