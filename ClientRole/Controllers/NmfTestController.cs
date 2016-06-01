using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Results;
using BenchmarkLibrary;
using ClientRole;
using Microsoft.WindowsAzure.ServiceRuntime;
using NMF.Expressions.Linq.Orleans;
using NMF.Expressions.Linq.Orleans.Model;
using NMF.Models;
using NMF.Models.Tests.Railway;
using NMF.Utilities;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Linq;
using Orleans.Streams.Messages;
using Orleans.Streams.Stateful.Messages;

//using WebRole.Models;

namespace WebRole.Controllers
{
    public class NmfTestcontroller : ApiController
    {
        // GET: api/Test
        public async Task<JsonResult<List<BenchmarkRunResult>>> Post([FromBody] BenchmarkSettings settings)
        {
            BenchmarkSetup.SetupModelLoader(RoleEnvironment.GetLocalResource("ModelStorage").RootPath);
            List<BenchmarkRunResult> results;

            results = await BenchmarkExecutor.ExecuteBenchmark(settings, HostingEnvironment.MapPath("~/"));

            return Json(results);
        }

        public async Task<string> Get(int flushSize = 512, int modelSize = 32, int multiplex = 1)
        {
            var modelGrain = GrainClient.GrainFactory.GetGrain<IModelContainerGrain<Model>>(Guid.NewGuid());
            await modelGrain.LoadModelFromPath(string.Format("railway-{0}.xmi", modelSize));

            var factory = new IncrementalNmfModelStreamProcessorAggregateFactory(GrainClient.GrainFactory, modelGrain);
            var query = await modelGrain.SimpleSelectMany(
                model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<ISegment>(), factory, multiplex)
                .Where(seg => seg.Length < 0, 1);

            var consumer = new TransactionalStreamConsumer(GrainClient.GetStreamProvider("CollectionStreamProvider"));
            List<long> times = new List<long>();

            consumer.MessageDispatcher.Register<RemoteItemAddMessage<ISegment>>((msg) =>
            {
                times.Add(DateTime.Now.Ticks);
                return TaskDone.Done;
            });
            await consumer.SetInput(await query.GetOutputStreams());

            var sb = new StringBuilder();

            for (int i = 1; i <= 10; i++)
            {
                long startTicks = DateTime.Now.Ticks;
                await modelGrain.EnumerateToSubscribers();
                long endTicks = DateTime.Now.Ticks;

                sb.AppendLine(string.Format("Number of items: {0}", times.Count));
                sb.AppendLine(string.Format("Ticks last: {0}, First result arrived: {1}, Last result arrived: {2}", (endTicks - startTicks) / TimeSpan.TicksPerMillisecond,
                    (times.Min() - startTicks) / TimeSpan.TicksPerMillisecond, (times.Max() - startTicks) / TimeSpan.TicksPerMillisecond));
                times.Clear();
            }
       

            return sb.ToString();
        }

        // POST: api/Benchmark
        //public JsonResult<List<BenchmarkResult>> Post([FromBody] BenchmarkSettings settings)
        //{
        //    var path = HostingEnvironment.MapPath("~/ClientConfiguration.xml");
        //    var config = ClientConfiguration.LoadFromFile(path);

        //    if (!AzureClient.IsInitialized)
        //    {
        //        AzureClient.Initialize(path);
        //        GrainClient.SetResponseTimeout(TimeSpan.FromSeconds(1800));
        //    }

        //    var treeSettings = new RandomTreeParams(settings.SeedValue, settings.TreeDepth,
        //        settings.ChildrenPerNode, TimeSpan.FromMilliseconds(settings.ExecutionTimePerNode),
        //        settings.NodeDataBytes, settings.MaxNodesPerContainer, settings.NumberRandomConnections, settings.MaxConcurrentContainers);

        //    var resultChecker = new ResultComparator();
        //    //

        //    if (settings.TreeTypes == null)
        //    {
        //        settings.TreeTypes = new List<TreeType>()
        //        {
        //            TreeType.RandomTreeGrainFromClient,
        //            TreeType.RandomTreeContainerGrainSync,
        //            TreeType.RandomTreeContainerGrainAsync,
        //            TreeType.RandomTreeDefault,
        //            TreeType.RandomTreeGrainFromGrain
        //        };
        //    }

        //    var results = new List<BenchmarkResult>();

        //    if (settings.TreeTypes.Contains(TreeType.RandomTreeDefault))
        //    {
        //        var resultSync = BenchmarkExecutor.Run(() =>
        //        {
        //            var tree = new RandomTree(treeSettings);
        //            tree.Construct();
        //            return Task.FromResult((IBenchmarkTree)tree);
        //        }, treeSettings, TreeType.RandomTreeDefault, resultChecker);
        //        results.Add(resultSync);
        //    }


        //    if (settings.TreeTypes.Contains(TreeType.RandomTreeGrainFromGrain))
        //    {
        //        IRandomTreeGrain randomTree = null;

        //        var result = BenchmarkExecutor.Run(async () =>
        //        {
        //            randomTree = GrainClient.GrainFactory.GetGrain<IRandomTreeGrain>(0);
        //            await randomTree.Construct(treeSettings);
        //            return randomTree;
        //        }, treeSettings, TreeType.RandomTreeGrainFromGrain, resultChecker);
        //        results.Add(result);
        //    }


        //    if (settings.TreeTypes.Contains(TreeType.RandomTreeGrainFromClient))
        //    {
        //        var result = BenchmarkExecutor.Run(() =>
        //        {
        //            var t = new RandomTreeOrleans(treeSettings);
        //            t.Construct();
        //            return Task.FromResult((IBenchmarkTree)t);
        //        }, treeSettings, TreeType.RandomTreeGrainFromClient, resultChecker);
        //        results.Add(result);
        //    }


        //    if (settings.TreeTypes.Contains(TreeType.RandomTreeContainerGrainSync))
        //    {

        //        IRandomContainerTreeGrain randomTree1 = null;
        //        var result = BenchmarkExecutor.Run(async () =>
        //        {
        //            randomTree1 = GrainClient.GrainFactory.GetGrain<IRandomContainerTreeGrain>(0);
        //            await randomTree1.Construct(treeSettings);
        //            return randomTree1;
        //        }, treeSettings, TreeType.RandomTreeContainerGrainSync, resultChecker);
        //        results.Add(result);
        //    }


        //    if (settings.TreeTypes.Contains(TreeType.RandomTreeContainerGrainAsync))
        //    {
        //        IRandomContainerTreeGrain randomTree1 = null;
        //        var result = BenchmarkExecutor.Run(async () =>
        //        {
        //            randomTree1 = GrainClient.GrainFactory.GetGrain<IRandomContainerTreeGrain>(0);
        //            await randomTree1.Construct(treeSettings);
        //            return randomTree1;
        //        }, treeSettings, TreeType.RandomTreeContainerGrainAsync, resultChecker, true);
        //        result.AsyncContainerQueries = true;
        //        results.Add(result);
        //    }



        //    return Json(results);
        //}

        // PUT: api/Benchmark/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Benchmark/5
        public void Delete(int id)
        {
        }
    }
}
