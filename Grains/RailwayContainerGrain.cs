using System.Threading.Tasks;
using Orleans.Collections;
using OR = TTC2015.TrainBenchmark.Orleans.Railway;
using SR = TTC2015.TrainBenchmark.Railway; 

namespace Grains
{
    public class RailwayContainerGrain : ContainerNodeGrain<OR.IRailwayContainer>, IRailwayContainerGrain
    {
        public Task InitializeRailwayContainer()
        {
            throw new System.NotImplementedException();
        }
    }
}