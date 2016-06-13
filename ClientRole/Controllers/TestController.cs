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
using NMF.Expressions.Linq.Orleans.Model;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Streams.Endpoints;
using Orleans.Streams.Linq;
using Orleans.Streams.Messages;
using TestGrains;

//using WebRole.Models;

namespace WebRole.Controllers
{
    public class TestController : ApiController
    {
        // GET: api/Test
        public async Task<JsonResult<List<BenchmarkRunResult>>> Post([FromBody] BenchmarkSettings settings)
        {
            BenchmarkSetup.SetupModelLoader(RoleEnvironment.GetLocalResource("ModelStorage").RootPath);
            List<BenchmarkRunResult> results;

            results = await BenchmarkExecutor.ExecuteBenchmark(settings, HostingEnvironment.MapPath("~/"));

            return Json(results);
        }

        private static List<int> CreateShuffled(int start, int count)
        {
            return Enumerable.Range(0, count).OrderBy(_ => Guid.NewGuid()).ToList();
        }

        [HttpGet]
        public async Task<string> Tc3([FromUri] int[] sizes = null, int runs = 50, int multiplex = 18,
            int flushSize = 512, int option = 2)
        {
            sizes = (sizes == null || sizes.Length == 0) ? DefaultSizes : sizes;
            var sb = new StringBuilder();
            sb.AppendLine("NumberOfItems,FlushSize,Multiplex,Testcase,Run,ArriveStartMs,ArriveEndMs,DurationMs");
            var factory = new DefaultStreamProcessorAggregateFactory(GrainClient.GrainFactory);

            for (int curRun = 0; curRun < runs; curRun++)
            {
                if (multiplex > 0)
                {
                    var provider = new StreamMessageSenderComposite<int>(GrainClient.GetStreamProvider("CollectionStreamProvider"), multiplex)
                    {
                        FlushQueueSize = flushSize
                    };
                    StreamProcessorChain<int, int, DefaultStreamProcessorAggregateFactory> query = null;
                    if (option == 0)
                        query = await provider.Select(_ => _, factory).Where(i => i%2 == 0 && TestUtil.BusyWaiting(Int32.MaxValue));
                    else if (option == 1)
                        query = await provider.Select(_ => _, factory).Where(i => i%2 == 0);
                    else if (option == 2)
                        query = await provider.Select(_ => _, factory).Where(i => i%2 == 0 && TestUtil.BusyWaiting500k());
                    else if (option == 3)
                        query = await provider.Select(_ => _, factory).Where(i => i%2 == 0 && TestUtil.BusyWaiting2Mio());
                    else if (option == 4)
                        query = await provider.Select(_ => _, factory).Where(i => i%2 == 0 && TestUtil.BusyWaiting4Mio());
                    var consumer = new TransactionalStreamConsumer(GrainClient.GetStreamProvider("CollectionStreamProvider"));

                    List<long> times = new List<long>();
                    int itemCount = 0;

                    consumer.MessageDispatcher.Register<ItemMessage<int>>((msg) =>
                    {
                        times.Add(DateTime.Now.Ticks);
                        itemCount += msg.Items.Count;
                        return TaskDone.Done;
                    });
                    await consumer.SetInput(await query.GetOutputStreams());

                    foreach (int size in sizes)
                    {
                        times = new List<long>();
                        var l = CreateShuffled(0, size);
                        var tid = Guid.NewGuid();
                        long startTicks = DateTime.Now.Ticks;
                        await provider.StartTransaction(tid);

                        foreach (var item in l)
                        {
                            provider.EnqueueMessage(new ItemMessage<int>(item.SingleValueToList()));
                        }
                        await provider.EndTransaction(tid);
                        long endTicks = DateTime.Now.Ticks;

                        if (itemCount != l.Count(i => i%2 == 0))
                            throw new ArgumentException();

                        itemCount = 0;
                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", l.Count, flushSize, multiplex, 1, curRun,
                            (times.Min() - startTicks)/TimeSpan.TicksPerMillisecond, (times.Max() - startTicks)/TimeSpan.TicksPerMillisecond,
                            (endTicks - startTicks)/TimeSpan.TicksPerMillisecond));
                    }
                    await query.TearDown();
                }

                else
                {
                    foreach(var size in sizes)
                    {
                        var l = Enumerable.Range(0, size).ToList();
                        long startTicks = DateTime.Now.Ticks;
                        var result = l.Where(f => f % 2 == 0 && TestUtil.BusyWaiting500k()).ToList();
                        long endTicks = DateTime.Now.Ticks;
                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", l.Count, flushSize, multiplex, 1, curRun, 0, 0,
    (endTicks - startTicks) / TimeSpan.TicksPerMillisecond));
                    }
                }


            }

            return sb.ToString();

        }

        private static readonly int[] DefaultQuerySizes = {1, 2, 4, 8, 16, 32};

        [HttpGet]
        public async Task<string> Tc2(int size = 32768, [FromUri] int[] querySizes = null, int runs = 50, int multiplex = 1,
            int flushSize = 512, int payload = 512)
        {
            querySizes = (querySizes == null || querySizes.Length == 0) ? DefaultQuerySizes : querySizes;
            var sb = new StringBuilder();
            sb.AppendLine("NumberOfItems,QuerySize,FlushSize,Multiplex,Testcase,Run,ArriveStartMs,ArriveEndMs,DurationMs");
            var factory = new DefaultStreamProcessorAggregateFactory(GrainClient.GrainFactory);

            for (int curRun = 0; curRun < runs; curRun++)
            {
                foreach (int querySize in querySizes)
                {
                    if (multiplex > 0)
                    {
                        var provider = new StreamMessageSenderComposite<DummyIntWithPayload>(GrainClient.GetStreamProvider("CollectionStreamProvider"), multiplex)
                        {
                            FlushQueueSize = flushSize
                        };
                        var query = await provider.Select(_ => _, factory);
                        for (int i = 0; i < querySize; i++)
                        {
                            query = await query.Where(f => f.Value % 2 == 0);
                        }
                        var consumer = new TransactionalStreamConsumer(GrainClient.GetStreamProvider("CollectionStreamProvider"));

                        List<long> times = new List<long>();
                        int itemCount = 0;

                        consumer.MessageDispatcher.Register<ItemMessage<DummyIntWithPayload>>((msg) =>
                        {
                            times.Add(DateTime.Now.Ticks);
                            itemCount += msg.Items.Count;
                            return TaskDone.Done;
                        });
                        await consumer.SetInput(await query.GetOutputStreams());


                        times = new List<long>();
                        var l = CreateShuffled(0, size).Select(x => new DummyIntWithPayload(x, new byte[payload])).ToList();
                        var tid = Guid.NewGuid();
                        long startTicks = DateTime.Now.Ticks;
                        await provider.StartTransaction(tid);

                        foreach (var item in l)
                        {
                            provider.EnqueueMessage(new ItemMessage<DummyIntWithPayload>(item.SingleValueToList()));
                        }
                        await provider.EndTransaction(tid);
                        long endTicks = DateTime.Now.Ticks;

                        if (itemCount != l.Count(i => i.Value % 2 == 0))
                            throw new ArgumentException();

                        itemCount = 0;
                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", l.Count, querySize, flushSize, multiplex, 2, curRun,
                            (times.Min() - startTicks)/TimeSpan.TicksPerMillisecond, (times.Max() - startTicks)/TimeSpan.TicksPerMillisecond,
                            (endTicks - startTicks)/TimeSpan.TicksPerMillisecond));
                    }
                    else
                    {
                        var l = Enumerable.Range(0, size).ToList();
                        long startTicks = DateTime.Now.Ticks;
                        var result = l.Select(_ => _);
                        for (int i = 0; i < querySize; i++)
                        {
                            result = l.Where(f => f%2 == 0);
                        }
                        var resultList = result.ToList();
                        long endTicks = DateTime.Now.Ticks;

                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", l.Count, querySize, flushSize, multiplex, 2, curRun, 0, 0, 
                            (endTicks - startTicks) / TimeSpan.TicksPerMillisecond));

                    }
                }
            }

            return sb.ToString();
        }

        private static readonly int[] DefaultSizes = {32, 128, 512, 2048, 8192, 32768, 131072, 524288, 2097152};

        [HttpGet]
        public async Task<string> Tc1([FromUri] int[] sizes = null, int flushSize = 512, int multiplex = 1, int runs = 50)
        {
            sizes = (sizes == null || sizes.Length == 0) ? DefaultSizes : sizes;
            var factory = new DefaultStreamProcessorAggregateFactory(GrainClient.GrainFactory);
            var sb = new StringBuilder();
            sb.AppendLine("NumberOfItems,FlushSize,Multiplex,Testcase,Run,ArriveStartMs,ArriveEndMs,DurationMs");

            if (multiplex > 0)
            {
                for (int curRun = 0; curRun < runs; curRun++)
                {
                    var sender = new StreamMessageSenderComposite<int>(GrainClient.GetStreamProvider("CollectionStreamProvider"), multiplex)
                    {
                        FlushQueueSize = flushSize
                    };
                    var query = await sender.Select(_ => _, factory).Where(i => i%2 == 0);
                    var consumer = new TransactionalStreamConsumer(GrainClient.GetStreamProvider("CollectionStreamProvider"));

                    List<long> times = new List<long>();
                    int itemCount = 0;

                    consumer.MessageDispatcher.Register<ItemMessage<int>>((msg) =>
                    {
                        times.Add(DateTime.Now.Ticks);
                        itemCount += msg.Items.Count;
                        return TaskDone.Done;
                    });
                    await consumer.SetInput(await query.GetOutputStreams());

                    foreach (int size in sizes)
                    {
                        times = new List<long>();
                        var l = CreateShuffled(0, size);
                        var tid = Guid.NewGuid();
                        var resultItemCount = l.Count(i => i%2 == 0);


                        long startTicks = DateTime.Now.Ticks;
                        await sender.StartTransaction(tid);

                        foreach (var item in l)
                        {
                            sender.EnqueueMessage(new ItemMessage<int>(item.SingleValueToList()));
                        }
                        await sender.EndTransaction(tid);
                        long endTicks = DateTime.Now.Ticks;

                        if (itemCount != resultItemCount)
                            throw new ArgumentException(string.Format("Expected: {0} - Received: {1}", resultItemCount, itemCount));

                        itemCount = 0;
                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", l.Count, flushSize, multiplex, 1, curRun,
                            (times.Min() - startTicks)/TimeSpan.TicksPerMillisecond, (times.Max() - startTicks)/TimeSpan.TicksPerMillisecond,
                            (endTicks - startTicks)/TimeSpan.TicksPerMillisecond));
                    }
                }
            }
            else
            {
                for (int curRun = 0; curRun < runs; curRun++)
                {
                    foreach (int size in sizes)
                    {
                        var l = Enumerable.Range(0, size).ToList();
                        long startTicks = DateTime.Now.Ticks;
                        var result = l.Select(_ => _).Where(i => i%2 == 0).ToList();
                        long endTicks = DateTime.Now.Ticks;

                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", l.Count, flushSize, multiplex, 1, curRun,
                            0, 0,
                            (endTicks - startTicks) / TimeSpan.TicksPerMillisecond));
                    }
                }
            }


            return sb.ToString();
        }

        [HttpGet]
        public async Task<string> Tc4([FromUri] int[] sizes = null, int flushSize = 512, int multiplex = 1, int runs = 50)
        {
            sizes = (sizes == null || sizes.Length == 0) ? DefaultSizes : sizes;
            var factory = new DefaultStreamProcessorAggregateFactory(GrainClient.GrainFactory);
            var sb = new StringBuilder();
            sb.AppendLine("NumberOfItems,FlushSize,Multiplex,Testcase,Run,ArriveStartMs,ArriveEndMs,DurationMs");

            if (multiplex > 0)
            {
                for (int curRun = 0; curRun < runs; curRun++)
                {
                    var provider = new StreamMessageSenderComposite<int>(GrainClient.GetStreamProvider("CollectionStreamProvider"), 3)
                    {
                        FlushQueueSize = flushSize
                    };
                    var query = await provider.Select(_ => _, factory).SimpleSelectMany(i => Enumerable.Range(0,20)).Where(i => i % 2 == 0);
                    var consumer = new TransactionalStreamConsumer(GrainClient.GetStreamProvider("CollectionStreamProvider"));

                    List<long> times = new List<long>();
                    int itemCount = 0;

                    consumer.MessageDispatcher.Register<ItemMessage<int>>((msg) =>
                    {
                        times.Add(DateTime.Now.Ticks);
                        itemCount += msg.Items.Count;
                        return TaskDone.Done;
                    });
                    await consumer.SetInput(await query.GetOutputStreams());

                    foreach (int size in sizes)
                    {
                        times = new List<long>();
                        var l = CreateShuffled(0, size);
                        var tid = Guid.NewGuid();
                        var resultItemCount = l.Count * Enumerable.Range(0,20).Count(i => i % 2 == 0);


                        long startTicks = DateTime.Now.Ticks;
                        await provider.StartTransaction(tid);

                        foreach (var item in l)
                        {
                            provider.EnqueueMessage(new ItemMessage<int>(item.SingleValueToList()));
                        }
                        await provider.EndTransaction(tid);
                        long endTicks = DateTime.Now.Ticks;

                        if (itemCount != resultItemCount)
                            throw new ArgumentException(string.Format("Expected: {0} - Received: {1}", resultItemCount, itemCount));

                        itemCount = 0;
                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", l.Count, flushSize, multiplex, 1, curRun,
                            (times.Min() - startTicks) / TimeSpan.TicksPerMillisecond, (times.Max() - startTicks) / TimeSpan.TicksPerMillisecond,
                            (endTicks - startTicks) / TimeSpan.TicksPerMillisecond));
                    }
                }
            }
            else
            {
                for (int curRun = 0; curRun < runs; curRun++)
                {
                    foreach (int size in sizes)
                    {
                        var l = Enumerable.Range(0, size).ToList();
                        long startTicks = DateTime.Now.Ticks;
                        var result = l.Select(_ => _).SelectMany(i => Enumerable.Range(0, 20)).Where(i => i % 2 == 0).ToList();
                        long endTicks = DateTime.Now.Ticks;

                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", l.Count, flushSize, multiplex, 1, curRun,
                            0, 0,
                            (endTicks - startTicks) / TimeSpan.TicksPerMillisecond));
                    }
                }
            }


            return sb.ToString();
        }



        // PUT: api/Benchmark/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Benchmark/5
        public void Delete(int id)
        {
        }
    }
}