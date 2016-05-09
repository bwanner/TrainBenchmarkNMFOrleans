using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMF.Expressions.Linq.Orleans.Model;
using NMF.Models;
using Orleans;
using TTC2015.TrainBenchmark;

namespace BenchmarkLibrary
{
    public class BenchmarkExecutor
    {
        public static async Task ExecuteBenchmark(BenchmarkSettings settings, string rootPath)
        {
            for (int i = 0; i < settings.Runs; i++)
            {
                await ExecuteRunOrleans(i, settings, rootPath);
            }
        }

        private static async Task ExecuteRunOrleans(int i, BenchmarkSettings settings, string rootFolder)
        {
            var fixedChangeSet = string.Equals(settings.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
            var expectedNumberOfActions = LoadExpectedResults(0, settings, rootFolder);
            var modelPath = string.Format("{0}/railway-models/railway-{1}.xmi", rootFolder, settings.Size);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            TrainRepairOrleans trainRepairOrleans = new IncrementalTrainRepairOrleans();
            var modelContainer = GrainClient.GrainFactory.GetGrain<IModelContainerGrain<Model>>(Guid.NewGuid());
            await modelContainer.LoadModelFromPath(modelPath);
            await trainRepairOrleans.RepairTrains(modelContainer, settings.Query, GrainClient.GrainFactory);

            stopwatch.Stop();
            //Emit("read", i, 0, null);

            // Check
            stopwatch.Restart();
            var actions = await trainRepairOrleans.Check();
            stopwatch.Stop();
            //Emit("check", i, 0, actions.Count());

            var orleansElements = actions.Select(x => x.Item1).OrderBy(s => s).ToList();
            if(orleansElements.Count != expectedNumberOfActions)
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
                //Emit("repair", i, iter, null);

                // ReCheck
                stopwatch.Restart();
                actions = await trainRepairOrleans.Check();
                stopwatch.Stop();
                //Emit("recheck", i, iter, actions.Count());

                orleansElements = actions.Select(x => x.Item1).OrderBy(s => s).ToList();
                if (orleansElements.Count != LoadExpectedResults(iter + 1, settings, rootFolder))
                    throw new InvalidOperationException();


                actionsSorted = (from pair in actions
                                 orderby pair.Item1
                                 select pair.Item2).ToList();
            }
        }

        private static int LoadExpectedResults(int iteration, BenchmarkSettings settings, string rootFolder)
        {
            string fileName = String.Format("{0}-{1}.tsv", settings.ChangeSet == "fixed" ? "fixed" : "proportional", settings.Query);
            string[] lines = File.ReadAllLines((rootFolder + "/expected-results/" + fileName));
            var line = lines.Single(l => l.StartsWith(settings.Size.ToString() + '\t'));
            return Convert.ToInt32(line.Split('\t')[iteration + 1]);
        }
    }
}