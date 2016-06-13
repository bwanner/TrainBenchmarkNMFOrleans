using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkLibrary;
using NMF.Models;
using NMF.Expressions;
using NMF.Expressions.Linq;
using NMF.Expressions.Linq.Orleans;
using NMF.Expressions.Linq.Orleans.Model;
using NMF.Models.Tests.Railway;
using NMF.Utilities;
using Orleans;
using Orleans.Streams;
using Orleans.Streams.Linq;

namespace TTC2015.TrainBenchmark
{
    public abstract class TrainRepairOrleans
    {
        public QueryPattern Pattern { get; set; }
        public QueryPattern InjectPattern { get; set; }
        public Random Random { get; set; }

        protected IModelContainerGrain<Model> ModelContainerGrain { get; private set; }

        public async Task RepairTrains(IModelContainerGrain<Model> modelContainerGrain, BenchmarkSettings settings, IGrainFactory grainFactory)
        {
            string task = settings.Query;
            int[] scatterFactors = settings.ScatterFactors;
            ModelContainerGrain = modelContainerGrain;
            var factory = new IncrementalNmfModelStreamProcessorAggregateFactory(grainFactory, modelContainerGrain);
            Random = new Random(0);

            if (task == "PosLength")
            {
                if (settings.QueryVariant == 0)
                {
                    await Fix(
                        modelPattern:
                            await
                                modelContainerGrain.SimpleSelectMany(
                                    model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<ISegment>(), factory,
                                    GetScatterFactor(scatterFactors, 0))
                                    .Where(seg => seg.Length < 0, GetScatterFactor(scatterFactors, 1))
                                    .ToNmfModelConsumer(),
                        action: seg => modelContainerGrain.ExecuteSync((container, elementUri) =>
                        {
                            var localSegment = (ISegment) container.Resolve((Uri) elementUri);
                            localSegment.Length = -localSegment.Length + 1;
                        }, seg.RelativeUri),
                        sortKey: seg => string.Format("<segment : {0:0000}>", seg.Id.GetValueOrDefault()));
                }
                else if (settings.QueryVariant == 1)
                {
                    await Fix(
                        modelPattern:
                            await
                                modelContainerGrain.ProcessLocal(models =>
                                {
                                    return models.SelectMany(
                                        model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<ISegment>())
                                        .Where(seg => seg.Length < 0);
                                }
                                    , factory, GetScatterFactor(scatterFactors, 0))
                                    .ToNmfModelConsumer(),
                        action: seg => modelContainerGrain.ExecuteSync((container, elementUri) =>
                        {
                            var localSegment = (ISegment) container.Resolve((Uri) elementUri);
                            localSegment.Length = -localSegment.Length + 1;
                        }, seg.RelativeUri, true),
                        sortKey: seg => string.Format("<segment : {0:0000}>", seg.Id.GetValueOrDefault()));
                }

                else if (settings.QueryVariant == 2)
                {
                    await Fix(
                        modelPattern:
                            await
                                modelContainerGrain.ProcessLocal(models =>
                                {
                                    return models.SelectMany(
                                        model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<ISegment>())
                                        .Where(seg => seg.Length < 0);
                                }
                                    , factory, GetScatterFactor(scatterFactors, 0))
                                    .ToNmfModelConsumer(),
                        action: seg => modelContainerGrain.ExecuteSync((container, elementUri) =>
                        {
                            var localSegment = (ISegment) container.Resolve((Uri) elementUri);
                            localSegment.Length = -localSegment.Length + 1;
                        }, seg.RelativeUri, false),
                        sortKey: seg => string.Format("<segment : {0:0000}>", seg.Id.GetValueOrDefault()));
                }

                //await Fix(
                //    modelPattern:
                //        await
                //            modelContainerGrain.SimpleSelectMany(
                //                model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<ISegment>(), factory,
                //                GetScatterFactor(scatterFactors, 0))
                //                .ProcessLocal(segments => segments.Where(seg => seg.Length <= 0))
                //                .ToNmfModelConsumer(),
                //    action: seg => modelContainerGrain.ExecuteSync((container, elementUri) =>
                //    {
                //        var localSegment = (ISegment) container.Resolve((Uri) elementUri);
                //        localSegment.Length = -localSegment.Length + 1;
                //    }, seg.RelativeUri),
                //    sortKey: seg => string.Format("<segment : {0:0000}>", seg.Id.GetValueOrDefault()));
            }
            if (task == "SwitchSensor")
            {
                // SwitchSensor
                var query =
                    await
                        modelContainerGrain.SimpleSelectMany(
                            model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<ISwitch>(), factory,
                            GetScatterFactor(scatterFactors, 0))
                            .Where(sw => sw.Sensor == null, GetScatterFactor(scatterFactors, 1))
                            .ToNmfModelConsumer();

                await Fix(modelPattern: query,
                    action: sw => modelContainerGrain.ExecuteSync((container, elementUri) =>
                    {
                        var swi = (ISwitch) container.Resolve((Uri) elementUri);
                        swi.Sensor = new Sensor();
                    }, sw.RelativeUri, true),
                    sortKey: sw => string.Format("<sw : {0:0000}>", sw.Id.GetValueOrDefault()));
                ;
            }
            if (task == "SwitchSet")
            {
                TransactionalStreamModelConsumer<ISwitchPosition, Model> query = null;
                if (settings.QueryVariant == 0)
                {
                    query = await
                        modelContainerGrain.SimpleSelectMany(
                            model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<IRoute>(), factory,
                            GetScatterFactor(scatterFactors, 0))
                            .Where(route => route.Entry != null && route.Entry.Signal == Signal.GO, GetScatterFactor(scatterFactors, 1))
                            .SimpleSelectMany(route => route.Follows.OfType<ISwitchPosition>(), GetScatterFactor(scatterFactors, 2))
                            .Where(swp => swp.Switch.CurrentPosition != swp.Position, GetScatterFactor(scatterFactors, 3))
                            .ToNmfModelConsumer();
                }
                else if (settings.QueryVariant == 1)
                {
                    query = await modelContainerGrain.ProcessLocal(models =>
                    {
                        return models.SelectMany(
                            model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<IRoute>())
                            .Where(route => route.Entry != null && route.Entry.Signal == Signal.GO);
                    }
                        , factory, GetScatterFactor(scatterFactors, 0))
                        .ProcessLocal(routes =>
                        {
                            return routes.SelectMany(route => route.Follows.OfType<ISwitchPosition>())
                                .Where(swp => swp.Switch.CurrentPosition != swp.Position);
                        }).ToNmfModelConsumer();
                }
                await
                    Fix(
                        modelPattern: query,
                        action: swp => modelContainerGrain.ExecuteSync((model, elementUri) =>
                        {
                            var localSwp = (ISwitchPosition) model.Resolve((Uri) elementUri);
                            localSwp.Switch.CurrentPosition = localSwp.Position;
                        }, swp.RelativeUri),
                        sortKey:
                            swP =>
                                string.Format("<semaphore : {0:0000}, route : {1:0000}, swP : {2:0000}, sw : {3:0000}>",
                                    swP.Route.Entry.Id.GetValueOrDefault(),
                                    swP.Route.Id.GetValueOrDefault(), swP.Id.GetValueOrDefault(), swP.Switch.Id.GetValueOrDefault())
                        );
            }
            if (task == "RouteSensor")
            {
                TransactionalStreamModelConsumer<ModelElementTuple<IRoute, ISwitchPosition>, Model> query = null;
                if (settings.QueryVariant == 0)
                {
                    query =
                        await
                            modelContainerGrain.SimpleSelectMany(
                                model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<IRoute>(), factory,
                                GetScatterFactor(scatterFactors, 0))
                                .SelectMany(route => route.Follows.OfType<ISwitchPosition>(),
                                    (route, position) => new ModelElementTuple<IRoute, ISwitchPosition>(route, position),
                                    GetScatterFactor(scatterFactors, 1))
                                .Where(tuple => tuple.Item2.Switch.Sensor != null && !tuple.Item1.DefinedBy.Contains(tuple.Item2.Switch.Sensor),
                                    GetScatterFactor(scatterFactors, 2))
                                .ToNmfModelConsumer();
                }

                else if (settings.QueryVariant == 1)
                {
                    query = await modelContainerGrain.SimpleSelectMany(
                        model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<IRoute>(), factory,
                        GetScatterFactor(scatterFactors, 0))
                        .ProcessLocal(routes =>
                            from route in routes
                            from swP in route.Follows.OfType<ISwitchPosition>()
                            where swP.Switch.Sensor != null && !route.DefinedBy.Contains(swP.Switch.Sensor)
                            select new ModelElementTuple<IRoute, ISwitchPosition>(route, swP))
                        .ToNmfModelConsumer();
                }

                await Fix(modelPattern: query,
                    action: async match => await modelContainerGrain.ExecuteSync((model, mat) =>
                    {
                        var localMatch = (Tuple<Uri, Uri>) mat;
                        var localRoute = (IRoute) model.Resolve(localMatch.Item1);
                        var localSwitchPosition = (ISwitchPosition) model.Resolve(localMatch.Item2);
                        var localSensor = localSwitchPosition.Switch.Sensor;
                        localRoute.DefinedBy.Add(localSensor);
                    }, new Tuple<Uri, Uri>(match.Item1.RelativeUri, match.Item2.RelativeUri), true),
                    // Forward action here since removal of model items changes identifiers
                    sortKey: match => string.Format("<route : {0:0000}, sensor : {1:0000}, swP : {2:0000}, sw : {3:0000}>",
                        match.Item1.Id.GetValueOrDefault(),
                        match.Item2.Switch.Sensor.Id.GetValueOrDefault(),
                        match.Item2.Id.GetValueOrDefault(),
                        match.Item2.Switch.Id.GetValueOrDefault())
                    );

                // RouteSensor
                //Fix(pattern: from route in routes
                //             from swP in route.Follows.OfType<SwitchPosition>()
                //             where swP.Switch.Sensor != null && !route.DefinedBy.Contains(swP.Switch.Sensor)
                //             select new { Route = route, Sensor = swP.Switch.Sensor, SwitchPos = swP },
                //     action: match => match.Route.DefinedBy.Add(match.Sensor),
                //     sortKey: match => string.Format("<route : {0:0000}, sensor : {1:0000}, swP : {2:0000}, sw : {3:0000}>",
                //         match.Route.Id.GetValueOrDefault(),
                //         match.Sensor.Id.GetValueOrDefault(),
                //         match.SwitchPos.Id.GetValueOrDefault(),
                //         match.SwitchPos.Switch.Id.GetValueOrDefault()));
            }
            if (task == "SemaphoreNeighbor")
            {
                TransactionalStreamModelConsumer<ModelElementTuple<IRoute, IRoute, ITrackElement, ITrackElement>, Model> query = null;
                if (settings.QueryVariant == 0)
                {
                    query = await modelContainerGrain.SelectMany(
                        model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<IRoute>(),
                        (model, route) => new ModelElementTuple<Model, IRoute>(model, route), factory, GetScatterFactor(scatterFactors, 0))
                        .SelectMany(tuple => tuple.Item1.RootElements.Single().As<RailwayContainer>().Descendants().OfType<IRoute>(),
                            (tuple, route) => new ModelElementTuple<IRoute, IRoute>(tuple.Item2, route), GetScatterFactor(scatterFactors, 1))
                        .Where(tuple => tuple.Item1 != tuple.Item2 && tuple.Item2.Entry != tuple.Item1.Exit, GetScatterFactor(scatterFactors, 2))
                        .SelectMany(tuple => tuple.Item1.DefinedBy,
                            (tuple, sensor) => new ModelElementTuple<IRoute, IRoute, ISensor>(tuple.Item1, tuple.Item2, sensor),
                            GetScatterFactor(scatterFactors, 3))
                        .SelectMany(tuple => tuple.Item3.Elements,
                            (tuple, element) =>
                                new ModelElementTuple<IRoute, IRoute, ISensor, ITrackElement>(tuple.Item1, tuple.Item2, tuple.Item3, element),
                            GetScatterFactor(scatterFactors, 4))
                        .SelectMany(tuple => tuple.Item4.ConnectsTo,
                            (tuple, element) =>
                                new ModelElementTuple<IRoute, IRoute, ISensor, ITrackElement, ITrackElement>(tuple.Item1, tuple.Item2, tuple.Item3,
                                    tuple.Item4, element), GetScatterFactor(scatterFactors, 5))
                        .Where(tuple => tuple.Item5.Sensor == null || tuple.Item2.DefinedBy.Contains(tuple.Item5.Sensor),
                            GetScatterFactor(scatterFactors, 6))
                        .Select(
                            tuple =>
                                new ModelElementTuple<IRoute, IRoute, ITrackElement, ITrackElement>(tuple.Item1, tuple.Item2, tuple.Item4, tuple.Item5))
                        .ToNmfModelConsumer();
                }
                else if (settings.QueryVariant == 1)
                {
                    query = await modelContainerGrain.SelectMany(
                        model => model.RootElements.Single().As<RailwayContainer>().Descendants().OfType<IRoute>(),
                        (model, route) => new ModelElementTuple<Model, IRoute>(model, route), factory, GetScatterFactor(scatterFactors, 0))
                        .ProcessLocal(
                            enumerable =>
                            {
                                var routePairs = enumerable.SelectMany(
                                    tuple => tuple.Item1.RootElements.Single().As<RailwayContainer>().Descendants().OfType<IRoute>(),
                                    (tuple, route) => new {r1 = tuple.Item2, r2 = route});
                                var res = from tuple in routePairs
                                    where tuple.r1 != tuple.r2 && tuple.r2.Entry != tuple.r1.Exit
                                    from sensor1 in tuple.r1.DefinedBy
                                    from te1 in sensor1.Elements
                                    from te2 in te1.ConnectsTo
                                    where te2.Sensor == null || tuple.r2.DefinedBy.Contains(te2.Sensor)
                                    select new ModelElementTuple<IRoute, IRoute, ITrackElement, ITrackElement>(tuple.r1, tuple.r2, te1, te2);

                                return res;
                            }
                        )
                        .ToNmfModelConsumer();
                }

                await Fix(modelPattern: query,
                    action: async match => await modelContainerGrain.ExecuteSync((model, mat) =>
                    {
                        var localMatch = (Tuple<Uri, Uri>) mat;
                        var localRoute1 = (IRoute) model.Resolve(localMatch.Item1);
                        var localRoute2 = (IRoute) model.Resolve(localMatch.Item2);

                        localRoute2.Entry = localRoute1.Exit;
                    }, new Tuple<Uri, Uri>(match.Item1.RelativeUri, match.Item2.RelativeUri)),
                    sortKey:
                        match =>
                            string.Format(
                                "<semaphore : {0:0000}, route1 : {1:0000}, route2 : {2:0000}, sensor1 : {3:0000}, sensor2 : {4:0000}, te1 : {5:0000}, te2 : {6:0000}>",
                                match.Item1.Exit.Id.GetValueOrDefault(),
                                match.Item1.Id.GetValueOrDefault(),
                                match.Item2.Id.GetValueOrDefault(),
                                match.Item3.Sensor.Id.GetValueOrDefault(),
                                match.Item4.Sensor != null
                                    ? match.Item4.Sensor.Id.GetValueOrDefault()
                                    : 0,
                                match.Item3.Id.GetValueOrDefault(),
                                match.Item4.Id.GetValueOrDefault()));

                //            // SemaphoreNeighbor
                //Fix(pattern: from route1 in routes
                //                         from route2 in routes
                //	         where route1 != route2 && route2.Entry != route1.Exit
                //                         from sensor1 in route1.DefinedBy
                //                         from te1 in sensor1.Elements
                //                         from te2 in te1.ConnectsTo
                //                         where te2.Sensor == null || route2.DefinedBy.Contains(te2.Sensor)
                //	select new { Route1 = route1, Route2 = route2, Te1 = te1, Te2 = te2 },
                //	action: match => match.Route2.Entry = match.Route1.Exit,
                //	sortKey: match => string.Format("<semaphore : {0:0000}, route1 : {1:0000}, route2 : {2:0000}, sensor1 : {3:0000}, sensor2 : {4:0000}, te1 : {5:0000}, te2 : {6:0000}>",
                //		match.Route1.Exit.Id.GetValueOrDefault(),
                //		match.Route1.Id.GetValueOrDefault(),
                //                    match.Route2.Id.GetValueOrDefault(),
                //                    match.Te1.Sensor.Id.GetValueOrDefault(),
                //		match.Te2.Sensor != null ?
                //			match.Te2.Sensor.Id.GetValueOrDefault() : 0,
                //                    match.Te1.Id.GetValueOrDefault(),
                //			match.Te2.Id.GetValueOrDefault()));
            }
        }

        private int GetScatterFactor(int[] scatterFactors, int index)
        {
            if (scatterFactors.Length > index)
                return scatterFactors[index];

            return 1;
        }

        protected void CompareMatches<T>(IEnumerableExpression<T> pattern, IEnumerable<T> cmpResult, Func<T, string> sortKey, Func<T, T, bool> cmpFunc)
        {
            var matchesOne = (from match in pattern
                select new Tuple<string, T>(sortKey(match), match)).ToList();

            var matchesTwo = (from match in cmpResult
                select new Tuple<string, T>(sortKey(match), match)).ToList();


            if (matchesOne.Count != matchesTwo.Count)
            {
                throw new ArgumentException("Invalid result");
            }
            for (int i = 0; i < matchesOne.Count; i++)
            {
                if (!cmpFunc(matchesOne[i].Item2, matchesTwo[i].Item2))
                {
                    throw new ArgumentException("Invalid result");
                }
            }
        }

        protected abstract Task Fix<T, TModel>(TransactionalStreamModelConsumer<T, TModel> modelPattern, Func<T, Task> action, Func<T, string> sortKey)
            where TModel : IResolvableModel;

        public async Task<IEnumerable<Tuple<string, Func<Task>>>> Check()
        {
            return await Pattern.GetAvailableActions();
        }

        public async Task<IEnumerable<Tuple<string, Func<Task>>>> Inject()
        {
            return await InjectPattern.GetAvailableActions();
        }

        public async Task RepairFixed(int count, List<Func<Task>> actions)
        {
            List<Task> awaitedActions = new List<Task>(actions.Count);
            var tid = await ModelContainerGrain.StartModelUpdate();
            for (int i = 0; i < count && i < actions.Count; i++)
            {
                int index = Random.NextInt(actions.Count);
                awaitedActions.Add(actions[index]());
                actions.RemoveAt(index);
            }
            await Task.WhenAll(awaitedActions);
            await ModelContainerGrain.EndModelUpdate(tid);
        }

        public async Task RepairProportional(int percentage, List<Func<Task>> actions)
        {
            await RepairFixed(Pattern.NumberOfInvalidElements*percentage/100, actions);
        }

        public async Task Reset()
        {
            await Pattern.Clear();
        }

        public abstract class QueryPattern
        {
            public abstract Task<IEnumerable<Tuple<string, Func<Task>>>> GetAvailableActions();

            public abstract int NumberOfInvalidElements { get; }

            public virtual Task Clear()
            {
                return TaskDone.Done;
            }
        }
    }

    public class IncrementalTrainRepairOrleans : TrainRepairOrleans
    {
        private class QueryPattern<T, TModel> : QueryPattern where TModel : IResolvableModel
        {
            public IModelContainerGrain<Model> Source { get; set; }
            public TransactionalStreamModelConsumer<T, TModel> ResultConsumer { get; set; }
            public Func<T, string> SortKey { get; set; }
            public Func<T, Task> Action { get; set; }

            private bool _firstCheckCall = true;

            public override async Task<IEnumerable<Tuple<string, Func<Task>>>> GetAvailableActions()
            {
                if (_firstCheckCall)
                    await Source.EnumerateToSubscribers();

                _firstCheckCall = false;

                return ResultConsumer.Items.Select(item => new Tuple<string, Func<Task>>(SortKey(item), () => Action(item)));
            }

            public override int NumberOfInvalidElements => ResultConsumer.Items.Count;

            public override async Task Clear()
            {
                //await Source.TearDown();
                //Source.Detach();
            }
        }

        protected override Task Fix<T, TModel>(TransactionalStreamModelConsumer<T, TModel> resultConsumer, Func<T, Task> action,
            Func<T, string> sortKey)
        {
            Pattern = new QueryPattern<T, TModel>()
            {
                Source = ModelContainerGrain,
                ResultConsumer = resultConsumer,
                Action = action,
                SortKey = sortKey
            };
            return TaskDone.Done;
        }
    }
}