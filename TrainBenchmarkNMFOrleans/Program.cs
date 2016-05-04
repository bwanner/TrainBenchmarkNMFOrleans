using NMF.Models.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectionHost;
using NMF.Expressions;
using NMF.Expressions.Linq.Orleans.Model;
using NMF.Models;
using NMF.Models.Tests.Railway;
using NMF.Utilities;
using Orleans;

namespace TTC2015.TrainBenchmark
{
    class Program
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static string tool;
        private static Configuration configuration;

        static void Main(string[] args)
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = new string[0],
            });

            Orleans.GrainClient.Initialize("ClientConfiguration.xml");

            // TODO: once the previous call returns, the silo is up and running.
            //       This is the place your custom logic, for example calling client logic
            //       or initializing an HTTP front end for accepting incoming requests.


            Task.Run(async () => { await AsyncMain(args); }).Wait();


            Console.WriteLine("Orleans Silo is running.\nPress Enter to terminate...");
            Console.ReadLine();

            hostDomain.DoCallBack(ShutdownSilo);
        }

        static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper(args);

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        static void ShutdownSilo()
        {
            if (hostWrapper != null)
            {
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }

        private static OrleansHostWrapper hostWrapper;

        private static readonly Func<string, Model> ModelLoadingFunc = (path) =>
        {
            var repository = new ModelRepository();
            var train = repository.Resolve(new Uri(new FileInfo(path).FullName));
            return train.Model;
        };

        private static async Task AsyncMain(string[] args)
        {
            GrainClient.Initialize();
            try
            {
                configuration = new Configuration();
                if (!CommandLine.Parser.Default.ParseArguments(args, configuration))
                {
                    Console.Error.WriteLine("Wrong parameter arguments!");
                    Console.Error.WriteLine(CommandLine.Text.HelpText.AutoBuild(configuration));
                }

                for (int i = 0; i < configuration.Runs; i++)
                {
                    //ExecuteRun(i);
                    await ExecuteRunOrleans(i);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = 1;
            }
        }

        private static async Task ExecuteRunOrleans(int i, bool validate = true)
        {
            TrainRepair trainRepair = null;
            Model railwayModel = null;
            var fixedChangeSet = string.Equals(configuration.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
            if (validate)
            {
                var repository = new ModelRepository();
                var train = repository.Resolve(new Uri(new FileInfo(configuration.Target).FullName));
                railwayModel = train.Model;
            }
            if (!configuration.Batch)
            {
                trainRepair = new BatchTrainRepair();
                tool = "NMFOrleans(Incremental)";
            }
            else
            {
                throw new NotImplementedException();
            }

            if (validate)
            {
                trainRepair.RepairTrains(railwayModel.RootElements.Single().As<RailwayContainer>(), configuration.Query);
            }


            stopwatch.Start();

            TrainRepairOrleans trainRepairOrleans = new IncrementalTrainRepairOrleans();
            var modelContainer = GrainClient.GrainFactory.GetGrain<IModelContainerGrain<Model>>(Guid.NewGuid());
            await modelContainer.LoadModelFromPath(configuration.Target);
            await trainRepairOrleans.RepairTrains(modelContainer, configuration.Query, GrainClient.GrainFactory);

            stopwatch.Stop();
            Emit("read", i, 0, null);

            // Check
            stopwatch.Restart();
            var actions = await trainRepairOrleans.Check();
            stopwatch.Stop();
            Emit("check", i, 0, actions.Count());
            IEnumerable<Tuple<string, Action>> localActions = new List<Tuple<string, Action>>();
            if (validate)
            {
                localActions = trainRepair.Check();
                var orleansElements = actions.Select(x => x.Item1).OrderBy(s => s).ToList();
                var localElements = localActions.Select(x => x.Item1).OrderBy(s => s).ToList();

                if (orleansElements.Count != localElements.Count)
                    throw new ArgumentException();
                for (int f = 0; f < orleansElements.Count; f++)
                {
                    if (orleansElements[f] != localElements[f])
                    {
                        throw new ArgumentException();
                    }
                }
            }

            var actionsSorted = (from pair in actions
                orderby pair.Item1
                select pair.Item2).ToList();

            var localActionsSorted = (from pair in localActions
                orderby pair.Item1
                select pair.Item2).ToList();

            for (int iter = 0; iter < configuration.IterationCount; iter++)
            {
                // Repair
                if (fixedChangeSet)
                {
                    stopwatch.Restart();
                    await trainRepairOrleans.RepairFixed(10, actionsSorted);
                    stopwatch.Stop();
                    if (validate)
                        trainRepair.RepairFixed(10, localActionsSorted);
                }

                stopwatch.Restart();
                await trainRepairOrleans.RepairProportional(10, actionsSorted);
                stopwatch.Stop();
                if (validate)
                    trainRepair.RepairProportional(10, localActionsSorted);
                Emit("repair", i, iter, null);

                //containerXml = await modelContainer.ModelToString(container => container);
                //localXml = railwayModel.ToXmlString();
                //if (!containerXml.Equals(localXml))
                //{
                //    throw new ArgumentException();
                //}

                // ReCheck
                stopwatch.Restart();
                actions = await trainRepairOrleans.Check();
                stopwatch.Stop();
                Emit("recheck", i, iter, actions.Count());

                if (validate)
                {
                    localActions = trainRepair.Check();
                    var orleansElements = actions.Select(x => x.Item1).OrderBy(s => s).ToList();
                    var localElements = localActions.Select(x => x.Item1).OrderBy(s => s).ToList();

                    if (orleansElements.Count != localElements.Count)
                        throw new ArgumentException();
                    for (int f = 0; f < orleansElements.Count; f++)
                    {
                        if (orleansElements[f] != localElements[f])
                        {
                            throw new ArgumentException();
                        }
                    }
                }

                actionsSorted = (from pair in actions
                    orderby pair.Item1
                    select pair.Item2).ToList();

                localActionsSorted = (from pair in localActions
                    orderby pair.Item1
                    select pair.Item2).ToList();
            }
        }

        //private static void ExecuteRun(int i)
        //{
        //    var fixedChangeSet = string.Equals(configuration.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
        //    stopwatch.Start();
        //    var repository = new ModelRepository();
        //    var train = repository.Resolve(new Uri(new FileInfo(configuration.Target).FullName));
        //    var railwayContainer = train.Model.RootElements.Single() as RailwayContainer;

        //    TrainRepair trainRepair = null;
        //    TrainRepairOrleans trainRepairOrleans = null;
        //    if (!configuration.Batch)
        //    {
        //        trainRepair = new IncrementalTrainRepair();
        //        trainRepairOrleans = new IncrementalTrainRepairOrleans();
        //        tool = "NMF(Incremental)";
        //    }
        //    else
        //    {
        //        trainRepair = new BatchTrainRepair();
        //        tool = "NMF(Batch)";
        //    }
        //    trainRepair.RepairTrains(railwayContainer, configuration.Query);
        //    stopwatch.Stop();
        //    Emit("read", i, 0, null);

        //    // Check
        //    stopwatch.Restart();
        //    var actions = trainRepair.Check();
        //    stopwatch.Stop();
        //    Emit("check", i, 0, actions.Count());

        //    if (!configuration.Inject)
        //    {
        //        var actionsSorted = (from pair in actions
        //            orderby pair.Item1
        //            select pair.Item2).ToList();

        //        for (int iter = 0; iter < configuration.IterationCount; iter++)
        //        {
        //            // Repair
        //            if (fixedChangeSet)
        //            {
        //                stopwatch.Restart();
        //                trainRepair.RepairFixed(10, actionsSorted);
        //                stopwatch.Stop();
        //            }
        //            else
        //            {
        //                stopwatch.Restart();
        //                trainRepair.RepairProportional(10, actionsSorted);
        //                stopwatch.Stop();
        //            }
        //            Emit("repair", i, iter, null);

        //            // ReCheck
        //            stopwatch.Restart();
        //            actions = trainRepair.Check();
        //            stopwatch.Stop();
        //            Emit("recheck", i, iter, actions.Count());

        //            actionsSorted = (from pair in actions
        //                orderby pair.Item1
        //                select pair.Item2).ToList();
        //        }
        //    }
        //    else
        //    {
        //        var actionsSorted = (from pair in trainRepair.Inject()
        //            orderby pair.Item1
        //            select pair.Item2).ToList();

        //        for (int iter = 0; iter < configuration.IterationCount; iter++)
        //        {
        //            // Repair
        //            if (fixedChangeSet)
        //            {
        //                stopwatch.Restart();
        //                trainRepair.RepairFixed(10, actionsSorted);
        //                stopwatch.Stop();
        //            }
        //            else
        //            {
        //                stopwatch.Restart();
        //                trainRepair.RepairProportional(10, actionsSorted);
        //                stopwatch.Stop();
        //            }
        //            Emit("inject", i, iter, null);

        //            // ReCheck
        //            stopwatch.Restart();
        //            actions = trainRepair.Check();
        //            stopwatch.Stop();
        //            Emit("recheck", i, iter, actions.Count());

        //            actionsSorted = (from pair in trainRepair.Inject()
        //                orderby pair.Item1
        //                select pair.Item2).ToList();
        //        }
        //    }
        //}

        private static void Emit(string phase, int runIdx, int iteration, int? elements)
        {
            const string format = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}";

            Console.Out.WriteLine(format, configuration.ChangeSet, runIdx, tool, configuration.Size, configuration.Query, phase, iteration, "time",
                stopwatch.ElapsedTicks*100);
            Console.Out.WriteLine(format, configuration.ChangeSet, runIdx, tool, configuration.Size, configuration.Query, phase, iteration, "memory",
                Environment.WorkingSet);
            if (elements != null)
            {
                Console.Out.WriteLine(format, configuration.ChangeSet, runIdx, tool, configuration.Size, configuration.Query, phase, iteration, "rss",
                    elements.Value);
            }
        }
    }
}