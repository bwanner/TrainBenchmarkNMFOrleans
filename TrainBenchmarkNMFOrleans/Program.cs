using NMF.Models.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMF.Expressions;
using TTC2015.TrainBenchmark.Railway;

namespace TTC2015.TrainBenchmark
{
    class Program
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static string tool;
        private static Configuration configuration;

        static void Main(string[] args)
        {
            try
            {
                configuration = new Configuration();
                if (!CommandLine.Parser.Default.ParseArguments(args, configuration))
                {
                    Console.Error.WriteLine("Wrong parameter arguments!");
                    Console.Error.WriteLine(CommandLine.Text.HelpText.AutoBuild(configuration));
                }

                var fixedChangeSet = string.Equals(configuration.ChangeSet, "fixed", StringComparison.InvariantCultureIgnoreCase);
                for (int i = 0; i < configuration.Runs; i++)
                {
                    stopwatch.Start();
                    var repository = new ModelRepository();
                    var train = repository.Resolve(new Uri(new FileInfo(configuration.Target).FullName));
                    var railwayContainer = train.Model.RootElements.Single() as RailwayContainer;


                    TrainRepair trainRepair = null;
                    if (!configuration.Batch)
                    {
                        trainRepair = new IncrementalTrainRepair();
                        tool = "NMF(Incremental)";
                    }
                    else
                    {
                        trainRepair = new BatchTrainRepair();
                        tool = "NMF(Batch)";
                    }
                    trainRepair.RepairTrains(railwayContainer, configuration.Query);
                    stopwatch.Stop();
                    Emit("read", i, 0, null);

                    // Check
                    stopwatch.Restart();
                    var actions = trainRepair.Check();
                    stopwatch.Stop();
                    Emit("check", i, 0, actions.Count());

                    if (!configuration.Inject)
                    {
                        var actionsSorted = (from pair in actions
                                             orderby pair.Item1
                                             select pair.Item2).ToList();

                        for (int iter = 0; iter < configuration.IterationCount; iter++)
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
                            Emit("repair", i, iter, null);

                            // ReCheck
                            stopwatch.Restart();
                            actions = trainRepair.Check();
                            stopwatch.Stop();
                            Emit("recheck", i, iter, actions.Count());

                            actionsSorted = (from pair in actions
                                             orderby pair.Item1
                                             select pair.Item2).ToList();
                        }
                    }
                    else
                    {
                        var actionsSorted = (from pair in trainRepair.Inject()
                                             orderby pair.Item1
                                             select pair.Item2).ToList();

                        for (int iter = 0; iter < configuration.IterationCount; iter++)
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
                            Emit("inject", i, iter, null);

                            // ReCheck
                            stopwatch.Restart();
                            actions = trainRepair.Check();
                            stopwatch.Stop();
                            Emit("recheck", i, iter, actions.Count());

                            actionsSorted = (from pair in trainRepair.Inject()
                                             orderby pair.Item1
                                             select pair.Item2).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = 1;
            }
        }

        private static void Emit(string phase, int runIdx, int iteration, int? elements)
        {
            const string format = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}";

            Console.Out.WriteLine(format, configuration.ChangeSet, runIdx, tool, configuration.Size, configuration.Query, phase, iteration, "time", stopwatch.ElapsedTicks * 100);
            Console.Out.WriteLine(format, configuration.ChangeSet, runIdx, tool, configuration.Size, configuration.Query, phase, iteration, "memory", Environment.WorkingSet);
            if (elements != null)
            {
                Console.Out.WriteLine(format, configuration.ChangeSet, runIdx, tool, configuration.Size, configuration.Query, phase, iteration, "rss", elements.Value);
            }
        }
    }
}
