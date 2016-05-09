using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Results;
using BenchmarkLibrary;
using ClientRole;
using NMF.Expressions.Linq.Orleans.Model;
using Orleans.Streams;

//using WebRole.Models;

namespace WebRole.Controllers
{
    public class BenchmarkController : ApiController
    {
        // GET: api/Benchmark
        public async Task<IEnumerable<string>> Get()
        {
            //var path = HostingEnvironment.MapPath("~/ClientConfiguration.xml");
            //var config = ClientConfiguration.LoadFromFile(path);

            //if (!AzureClient.IsInitialized)
            //{
            //    AzureClient.Initialize(path);
            //    GrainClient.SetResponseTimeout(TimeSpan.FromSeconds(30));
            //}


            var friend = GrainClient.GrainFactory.GetGrain<IModelContainerGrain<NMF.Models.Model>>(Guid.NewGuid());

            var settings = new BenchmarkSettings()
            {
                ChangeSet = "10",
                Description = "Test",
                IterationCount = 10,
                Query = "PosLength",
                Runs = 1,
                RunType = ExecutionType.Orleans,
                Size = 2,
                ValidateAgainstTable = true
            };

            await BenchmarkExecutor.ExecuteBenchmark(settings, HostingEnvironment.MapPath("~/"));

            return "foobar".SingleValueToList();

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
