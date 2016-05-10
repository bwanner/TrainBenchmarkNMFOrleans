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
        public static async Task<List<BenchmarkRunResult>> ExecuteBenchmark(BenchmarkSettings settings, string rootPath, string modelRootPath)
        {
            var runs = new List<BenchmarkRunResult>();
            for (int i = 0; i < settings.Runs; i++)
            {
                var run = new BenchmarkRunResult();
                if (settings.RunType == ExecutionType.Orleans)
                    run.Runs = await ExecuteRunOrleans(settings, rootPath, modelRootPath);
                else
                    run.Runs = await ExecuteRun(settings, rootPath, modelRootPath);
                runs.Add(run);
            }

            return runs;
        }

        private static async Task<List<ExecutionInformation>> ExecuteRunOrleans(BenchmarkSettings settings, string rootFolder, string modelRootFolder)
        {
            var executionList = new List<ExecutionInformation>();
            var fixedChangeSet = string.Equals(settings.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
            var expectedNumberOfActions = LoadExpectedResults(0, settings, rootFolder);
            var modelPath = string.Format("{0}railway-{1}.xmi", modelRootFolder, settings.Size);
            modelPath = modelPath.Replace("ClientRole", "SiloRole");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            TrainRepairOrleans trainRepairOrleans = new IncrementalTrainRepairOrleans();
            var modelContainer = GrainClient.GrainFactory.GetGrain<IModelContainerGrain<Model>>(Guid.NewGuid());
            await modelContainer.LoadModelFromPath(modelPath);
            await trainRepairOrleans.RepairTrains(modelContainer, settings.Query, GrainClient.GrainFactory);

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
                throw new InvalidOperationException();

            var actionsSorted = (from pair in actions
                orderby pair.Item1
                select pair.Item2).ToList();

            for (int iter = 0; iter < settings.IterationCount; iter++)
            {
                // Repair
                if (fixedChangeSet)
                {
                    stopwatch.Restart();
                    await trainRepairOrleans.RepairFixed(10, actionsSorted);
                    stopwatch.Stop();
                }

                stopwatch.Restart();
                await trainRepairOrleans.RepairProportional(10, actionsSorted);
                stopwatch.Stop();
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
                    throw new InvalidOperationException();


                actionsSorted = (from pair in actions
                    orderby pair.Item1
                    select pair.Item2).ToList();
            }

            return executionList;
        }

        private static int LoadExpectedResults(int iteration, BenchmarkSettings settings, string rootFolder)
        {
            string fileName = String.Format("{0}-{1}.tsv", settings.ChangeSet == "fixed" ? "fixed" : "proportional", settings.Query);
            string[] lines = File.ReadAllLines((rootFolder + "/expected-results/" + fileName));
            var line = lines.Single(l => l.StartsWith(settings.Size.ToString() + '\t'));
            return Convert.ToInt32(line.Split('\t')[iteration + 1]);
        }

        public static Task<List<ExecutionInformation>> ExecuteRun(BenchmarkSettings settings, string rootFolder, string modelRootFolder)
        {
            var executionList = new List<ExecutionInformation>();
            var fixedChangeSet = string.Equals(settings.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
            var expectedNumberOfActions = LoadExpectedResults(0, settings, rootFolder);
            var modelPath = string.Format("{0}railway-{1}.xmi", modelRootFolder, settings.Size);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var repository = new ModelRepository();
            var train = repository.Resolve(new Uri(new FileInfo(modelPath).FullName));
            var railwayContainer = train.Model.RootElements.Single() as RailwayContainer;

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
                throw new InvalidOperationException();

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
                    throw new InvalidOperationException();
            }

            return Task.FromResult(executionList);
        }
    }
}