using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMF.Expressions.Linq.Orleans.Model;
using NMF.Models;
using NMF.Models.Repository;
using NMF.Models.Tests.Railway;
using Orleans;
using TTC2015.TrainBenchmark;

namespace BenchmarkLibrary
{
    public class BenchmarkExecutor
    {
        public static async Task<List<BenchmarkRunResult>> ExecuteBenchmark(BenchmarkSettings settings, string rootPath)
        {
            var runs = new List<BenchmarkRunResult>();
            for (int i = 0; i < settings.Runs; i++)
            {
                Trace.TraceInformation("Starting run {0}", i);
                var run = new BenchmarkRunResult {Settings = settings};
                if (settings.RunType == ExecutionType.Orleans)
                    run.Runs = await ExecuteRunOrleans(settings, rootPath);
                else if (settings.RunType == ExecutionType.Compare)
                    run.Runs = await ExecuteRunOrleansAgainstIncremental(settings, rootPath);
                else
                    run.Runs = await ExecuteRun(settings, rootPath);
                runs.Add(run);
                Trace.TraceInformation("Finished run {0}", i);
            }

            return runs;
        }

        private static async Task<List<ExecutionInformation>> ExecuteRunOrleans(BenchmarkSettings settings, string rootFolder)
        {
            var executionList = new List<ExecutionInformation>();
            var fixedChangeSet = string.Equals(settings.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
            var expectedNumberOfActions = LoadExpectedResults(0, settings, rootFolder);
            var modelName = string.Format("railway-{0}.xmi", settings.Size);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            TrainRepairOrleans trainRepairOrleans = new IncrementalTrainRepairOrleans();
            var modelContainer = GrainClient.GrainFactory.GetGrain<IModelContainerGrain<Model>>(Guid.NewGuid());
            await modelContainer.LoadModelFromPath(modelName);
            await trainRepairOrleans.RepairTrains(modelContainer, settings, GrainClient.GrainFactory);
            var tid2 = await modelContainer.StartModelUpdate();
            await modelContainer.EndModelUpdate(tid2);

            stopwatch.Stop();
            executionList.Add(new ExecutionInformation()
            {
                Action = BenchmarkAction.Read,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Iteration = 0
            });

            // Check
            stopwatch.Restart();
            var actions = await trainRepairOrleans.Check();
            stopwatch.Stop();
            executionList.Add(new ExecutionInformation()
            {
                Action = BenchmarkAction.Check,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Iteration = 0
            });


            var orleansElements = actions.Select(x => x.Item1).OrderBy(s => s).ToList();
            if (orleansElements.Count != expectedNumberOfActions)
                executionList.Add(new ExecutionInformation() { Action = BenchmarkAction.ValidateFailed, ElapsedMilliseconds = 0, Iteration = 0 });

            var actionsSorted = (from pair in actions
                orderby pair.Item1
                select pair.Item2).ToList();

            for (int iter = 0; iter < settings.IterationCount; iter++)
            {
                // Repair
                if (fixedChangeSet)
                {
                    stopwatch.Restart();
                    var tid1 = await modelContainer.StartModelUpdate();
                    await trainRepairOrleans.RepairFixed(10, actionsSorted);
                    await modelContainer.EndModelUpdate(tid1);
                    stopwatch.Stop();
                }

                else
                {
                    stopwatch.Restart();
                    var tid = await modelContainer.StartModelUpdate();
                    await trainRepairOrleans.RepairProportional(10, actionsSorted);
                    await modelContainer.EndModelUpdate(tid);
                    stopwatch.Stop();
                }
                executionList.Add(new ExecutionInformation()
                {
                    Action = BenchmarkAction.Repair,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Iteration = iter
                });

                // ReCheck
                stopwatch.Restart();
                actions = await trainRepairOrleans.Check();
                stopwatch.Stop();
                executionList.Add(new ExecutionInformation()
                {
                    Action = BenchmarkAction.Recheck,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Iteration = iter + 1
                });

                orleansElements = actions.Select(x => x.Item1).OrderBy(s => s).ToList();
                if (orleansElements.Count != LoadExpectedResults(iter + 1, settings, rootFolder))
                    executionList.Add(new ExecutionInformation() { Action = BenchmarkAction.ValidateFailed, ElapsedMilliseconds = 0, Iteration = iter + 1 });


                actionsSorted = (from pair in actions
                    orderby pair.Item1
                    select pair.Item2).ToList();
            }

            await trainRepairOrleans.Reset();
            return executionList;
        }

        private static async Task<List<ExecutionInformation>> ExecuteRunOrleansAgainstIncremental(BenchmarkSettings settings, string rootFolder)
        {
            var executionList = new List<ExecutionInformation>();
            var fixedChangeSet = string.Equals(settings.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
            var expectedNumberOfActions = LoadExpectedResults(0, settings, rootFolder);
            var modelName = string.Format("railway-{0}.xmi", settings.Size);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var repository = new ModelRepository();
            var model = ModelLoader.Instance.LoadModel<Model>(modelName);
            var railwayContainer = model.RootElements.Single() as RailwayContainer;

            TrainRepair trainRepair = null;
            trainRepair = new IncrementalTrainRepair();
            trainRepair.RepairTrains(railwayContainer, settings.Query);

            stopwatch.Start();
            TrainRepairOrleans trainRepairOrleans = new IncrementalTrainRepairOrleans();
            var modelContainer = GrainClient.GrainFactory.GetGrain<IModelContainerGrain<Model>>(Guid.NewGuid());
            await modelContainer.LoadModelFromPath(modelName);
            await trainRepairOrleans.RepairTrains(modelContainer, settings, GrainClient.GrainFactory);
            var tid2 = await modelContainer.StartModelUpdate();
            await modelContainer.EndModelUpdate(tid2);
            stopwatch.Stop();

            executionList.Add(new ExecutionInformation()
            {
                Action = BenchmarkAction.Read,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Iteration = 0
            });

            // Check
            stopwatch.Restart();
            var actions = await trainRepairOrleans.Check();
            stopwatch.Stop();
            executionList.Add(new ExecutionInformation()
            {
                Action = BenchmarkAction.Check,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Iteration = 0
            });
            var localActions = trainRepair.Check();


            var orleansElements = actions.Select(x => x.Item1).OrderBy(s => s).ToList();
            var localElements = localActions.Select(x => x.Item1).OrderBy(s => s).ToList();
            if (orleansElements.Count != localElements.Count)
                executionList.Add(new ExecutionInformation() { Action = BenchmarkAction.ValidateFailed, ElapsedMilliseconds = 0, Iteration = 0 });

            var actionsSorted = (from pair in actions
                                 orderby pair.Item1
                                 select pair.Item2).ToList();

            var localActionsSorted = (from pair in localActions
                                 orderby pair.Item1
                                 select pair.Item2).ToList();

            for (int iter = 0; iter < settings.IterationCount; iter++)
            {
                // Repair
                if (fixedChangeSet)
                {
                    stopwatch.Restart();
                    var tid1 = await modelContainer.StartModelUpdate();
                    await trainRepairOrleans.RepairFixed(10, actionsSorted);
                    await modelContainer.EndModelUpdate(tid1);
                    stopwatch.Stop();

                    trainRepair.RepairFixed(10, localActionsSorted);
                }

                else
                {
                    stopwatch.Restart();
                    var tid = await modelContainer.StartModelUpdate();
                    await trainRepairOrleans.RepairProportional(10, actionsSorted);
                    await modelContainer.EndModelUpdate(tid);
                    stopwatch.Stop();

                    trainRepair.RepairProportional(10, localActionsSorted);
                }
                executionList.Add(new ExecutionInformation()
                {
                    Action = BenchmarkAction.Repair,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Iteration = iter
                });

                // ReCheck
                stopwatch.Restart();
                actions = await trainRepairOrleans.Check();
                stopwatch.Stop();
                executionList.Add(new ExecutionInformation()
                {
                    Action = BenchmarkAction.Recheck,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Iteration = iter + 1
                });
                localActions = trainRepair.Check();

                orleansElements = actions.Select(x => x.Item1).OrderBy(s => s).ToList();
                if (orleansElements.Count != localActions.Count())
                    executionList.Add(new ExecutionInformation() { Action = BenchmarkAction.ValidateFailed, ElapsedMilliseconds = 0, Iteration = iter + 1 });


                actionsSorted = (from pair in actions
                                 orderby pair.Item1
                                 select pair.Item2).ToList();

                localActionsSorted = (from pair in localActions
                                 orderby pair.Item1
                                 select pair.Item2).ToList();
            }
            await trainRepairOrleans.Reset();

            return executionList;
        }



        private static int LoadExpectedResults(int iteration, BenchmarkSettings settings, string rootFolder)
        {
            string fileName = String.Format("{0}-{1}.tsv", settings.ChangeSet == "fixed" ? "fixed" : "proportional", settings.Query);
            string[] lines = File.ReadAllLines((rootFolder + "/expected-results/" + fileName));
            var line = lines.Single(l => l.StartsWith(settings.Size.ToString() + '\t'));
            return Convert.ToInt32(line.Split('\t')[iteration + 1]);
        }

        public static Task<List<ExecutionInformation>> ExecuteRun(BenchmarkSettings settings, string rootFolder)
        {
            var executionList = new List<ExecutionInformation>();
            var fixedChangeSet = string.Equals(settings.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
            var expectedNumberOfActions = LoadExpectedResults(0, settings, rootFolder);
            var modelName = string.Format("railway-{0}.xmi", settings.Size);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var repository = new ModelRepository();
            var model = ModelLoader.Instance.LoadModel<Model>(modelName);
            var railwayContainer = model.RootElements.Single() as RailwayContainer;

            TrainRepair trainRepair = null;
            trainRepair = new IncrementalTrainRepair();
            trainRepair.RepairTrains(railwayContainer, settings.Query);
            stopwatch.Stop();
            executionList.Add(new ExecutionInformation()
            {
                Action = BenchmarkAction.Read,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Iteration = 0
            });

            // Check
            stopwatch.Restart();
            var actions = trainRepair.Check();
            stopwatch.Stop();

            executionList.Add(new ExecutionInformation()
            {
                Action = BenchmarkAction.Check,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Iteration = 0
            });

            var actionsSorted = (from pair in actions
                orderby pair.Item1
                select pair.Item2).ToList();

            if (actionsSorted.Count != expectedNumberOfActions)
                executionList.Add(new ExecutionInformation() { Action = BenchmarkAction.ValidateFailed, ElapsedMilliseconds = 0, Iteration = 0});

            for (int iter = 0; iter < settings.IterationCount; iter++)
            {
                // Repair
                if (fixedChangeSet)
                {
                    stopwatch.Restart();
                    trainRepair.RepairFixed(10, actionsSorted);
                    stopwatch.Stop();
                }
                else
                {
                    stopwatch.Restart();
                    trainRepair.RepairProportional(10, actionsSorted);
                    stopwatch.Stop();
                }
                executionList.Add(new ExecutionInformation()
                {
                    Action = BenchmarkAction.Repair,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Iteration = iter
                });

                // ReCheck
                stopwatch.Restart();
                actions = trainRepair.Check();
                stopwatch.Stop();
                executionList.Add(new ExecutionInformation()
                {
                    Action = BenchmarkAction.Recheck,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Iteration = iter + 1
                });

                actionsSorted = (from pair in actions
                    orderby pair.Item1
                    select pair.Item2).ToList();

                if (actionsSorted.Count != LoadExpectedResults(iter + 1, settings, rootFolder))
                    executionList.Add(new ExecutionInformation() { Action = BenchmarkAction.ValidateFailed, ElapsedMilliseconds = 0, Iteration = iter });
            }

            return Task.FromResult(executionList);
        }
    }
}