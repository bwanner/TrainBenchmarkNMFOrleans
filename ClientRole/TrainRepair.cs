using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMF.Models;
using NMF.Expressions;
using NMF.Expressions.Linq;
using NMF.Models.Tests.Railway;

namespace TTC2015.TrainBenchmark
{
    abstract class TrainRepair
    {
        public QueryPattern Pattern { get; set; }
        public QueryPattern InjectPattern { get; set; }
        public Random Random { get; set; }

        public void RepairTrains(RailwayContainer rc, string task)
        {
            Random = new Random(0);
			var routes = rc.Routes.Concat (rc.Invalids.OfType<Route> ());
            if (task == "ConnectedSegments")
            {
                Fix(pattern: from sensor in rc.Descendants().OfType<Sensor>()
                             from segment1 in sensor.Elements.OfType<Segment>()
                             from segment2 in segment1.ConnectsTo.OfType<Segment>()
                             from segment3 in segment2.ConnectsTo.OfType<Segment>()
                             from segment4 in segment3.ConnectsTo.OfType<Segment>()
                             from segment5 in segment4.ConnectsTo.OfType<Segment>()
                             from segment6 in segment5.ConnectsTo.OfType<Segment>()
                             where segment6.Sensor == sensor && segment1 != segment2 && segment1 != segment3 && segment1 != segment3 && segment1 != segment4 && segment1 != segment5 && segment1 != segment6
                                && segment2 != segment3 && segment2 != segment4 && segment2 != segment5 && segment2 != segment6 && segment3 != segment4 && segment3 != segment5 && segment3 != segment6
                                && segment4 != segment5 && segment4 != segment6 && segment5 != segment6
                             select new { Sensor = sensor, Segment1 = segment1, Segment2 = segment2, Segment3 = segment3, Segment4 = segment4, Segment5 = segment5, Segment6 = segment6 },
                     action: match =>
                     {
                         match.Segment1.ConnectsTo.Remove(match.Segment2);
                         match.Segment2.Sensor = null;
                         match.Segment1.ConnectsTo.Add(match.Segment3);
                     },
                     sortKey: match => string.Format("<sensor: {0:0000}, seg1: {1:0000}, seg2: {2:0000}, seg3: {3:0000}, seg4: {4:0000}, seg5: {5:0000}, seg6: {6:0000}>",
                         match.Sensor.Id.GetValueOrDefault(),
                         match.Segment1.Id.GetValueOrDefault(),
                         match.Segment2.Id.GetValueOrDefault(),
                         match.Segment3.Id.GetValueOrDefault(),
                         match.Segment4.Id.GetValueOrDefault(),
                         match.Segment5.Id.GetValueOrDefault(),
                         match.Segment6.Id.GetValueOrDefault()));

                Inject(pattern: from sensor in rc.Descendants().OfType<Sensor>()
                                from segment1 in sensor.Elements.OfType<Segment>()
                                from segment3 in segment1.ConnectsTo.OfType<Segment>()
                                where segment1 != segment3
                                select new { Sensor = sensor, Segment1 = segment1, Segment3 = segment3 },
                       action: match =>
                       {
                           var newSegment = new Segment();
                           newSegment.Length = 1;
                           newSegment.Sensor = match.Segment1.Sensor;
                           newSegment.ConnectsTo.Add(match.Segment3);
                           match.Segment1.ConnectsTo.Remove(match.Segment3);
                           match.Segment1.ConnectsTo.Add(newSegment);
                       },
                       sortKey: match => string.Format("<sensor: {0:0000}, seg1: {1:0000}, seg3: {2:0000}>", match.Sensor.Id.GetValueOrDefault(), match.Segment1.Id.GetValueOrDefault(),
                           match.Segment3.Id.GetValueOrDefault()));
            }            
            if (task == "PosLength")
            {
                // PosLength
                Fix(pattern: rc.Descendants().OfType<Segment>().Where(seg => seg.Length < 0),
                    action: segment => segment.Length = -segment.Length + 1,
                    sortKey: seg => string.Format("<segment : {0:0000}>", seg.Id.GetValueOrDefault()));

                Inject(pattern: rc.Descendants().OfType<Segment>().Where(seg => seg.Length >= 0),
                    action: segment => segment.Length = 0,
                    sortKey: seg => string.Format("<segment : {0:0000}>", seg.Id.GetValueOrDefault()));
            }
            if (task == "SwitchSensor")
            {
                //CompareMatches(rc.Descendants().OfType<Switch>().Where(sw => sw.Sensor == null), serializedContainer);
                // sslSwitchSensor
                Fix(pattern: rc.Descendants().OfType<Switch>().Where(sw => sw.Sensor == null),
                    action: sw => sw.Sensor = new Sensor(),
                    sortKey: sw => string.Format("<sw : {0:0000}>", sw.Id.GetValueOrDefault()));

                Inject(pattern: rc.Descendants().OfType<Switch>().Where(sw => sw.Sensor != null),
                    action: sw => sw.Sensor = null,
                    sortKey: sw => string.Format("<sw : {0:0000}>", sw.Id.GetValueOrDefault()));
            }
            if (task == "SwitchSet")
            {
                // SwitchSet
                Fix(pattern: from route in routes
                             where route.Entry != null && route.Entry.Signal == Signal.GO
                             from swP in route.Follows.OfType<SwitchPosition>()
                             where swP.Switch.CurrentPosition != swP.Position
                             select swP,
                     action: swP => swP.Switch.CurrentPosition = swP.Position,
                     sortKey: swP => string.Format("<semaphore : {0:0000}, route : {1:0000}, swP : {2:0000}, sw : {3:0000}>", swP.Route.Entry.Id.GetValueOrDefault(),
                         swP.Route.Id.GetValueOrDefault(), swP.Id.GetValueOrDefault(), swP.Switch.Id.GetValueOrDefault()));

                Inject(pattern: rc.Descendants().OfType<Switch>(),
                    action: sw => sw.CurrentPosition = ((Position)(((int)sw.CurrentPosition + 1) % 4)),
                    sortKey: sw => string.Format("<switch: {0:0000}>", sw.Id.GetValueOrDefault()));
            }
            if (task == "RouteSensor")
            {
                // RouteSensor
                Fix(pattern: from route in routes
                             from swP in route.Follows.OfType<SwitchPosition>()
                             where swP.Switch.Sensor != null && !route.DefinedBy.Contains(swP.Switch.Sensor)
                             select new { Route = route, Sensor = swP.Switch.Sensor, SwitchPos = swP },
                     action: match => match.Route.DefinedBy.Add(match.Sensor),
                     sortKey: match => string.Format("<route : {0:0000}, sensor : {1:0000}, swP : {2:0000}, sw : {3:0000}>",
                         match.Route.Id.GetValueOrDefault(),
                         match.Sensor.Id.GetValueOrDefault(),
                         match.SwitchPos.Id.GetValueOrDefault(),
                         match.SwitchPos.Switch.Id.GetValueOrDefault()));

                Inject(pattern: rc.Descendants().OfType<Switch>(),
                    action: sw => sw.Sensor = null,
                    sortKey: sw => string.Format("<switch: {0:0000}>", sw.Id.GetValueOrDefault()));
            }
            if (task == "SemaphoreNeighbor")
            {
                // SemaphoreNeighbor
				Fix(pattern: from route1 in routes
                             from route2 in routes
					         where route1 != route2 && route2.Entry != route1.Exit
                             from sensor1 in route1.DefinedBy
                             from te1 in sensor1.Elements
                             from te2 in te1.ConnectsTo
                             where te2.Sensor == null || route2.DefinedBy.Contains(te2.Sensor)
					select new { Route1 = route1, Route2 = route2, Te1 = te1, Te2 = te2 },
					action: match => match.Route2.Entry = match.Route1.Exit,
					sortKey: match => string.Format("<semaphore : {0:0000}, route1 : {1:0000}, route2 : {2:0000}, sensor1 : {3:0000}, sensor2 : {4:0000}, te1 : {5:0000}, te2 : {6:0000}>",
						match.Route1.Exit.Id.GetValueOrDefault(),
						match.Route1.Id.GetValueOrDefault(),
                        match.Route2.Id.GetValueOrDefault(),
                        match.Te1.Sensor.Id.GetValueOrDefault(),
						match.Te2.Sensor != null ?
							match.Te2.Sensor.Id.GetValueOrDefault() : 0,
                        match.Te1.Id.GetValueOrDefault(),
							match.Te2.Id.GetValueOrDefault()));

                Inject(pattern: from route in routes
                                where route.Entry != null
                                select route,
                       action: route => route.Entry = null,
                       sortKey: route => string.Format("<semaphore: {0:0000}, route: {1:0000}>", route.Entry.Id.GetValueOrDefault(), route.Id.GetValueOrDefault()));
            }
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

        protected abstract void Fix<T>(IEnumerableExpression<T> pattern, Action<T> action, Func<T, string> sortKey);

        protected abstract void Inject<T>(IEnumerableExpression<T> pattern, Action<T> action, Func<T, string> sortKey);

		public IEnumerable<Tuple<string, Action>> Check()
        {
			return Pattern.GetAvailableActions();
        }

        public IEnumerable<Tuple<string, Action>> Inject()
        {
            return InjectPattern.GetAvailableActions();
        }

		public void RepairFixed(int count, List<Action> actions)
        {
			for (int i = 0; i < count && i < actions.Count; i++)
            {
				int index = Random.NextInt(actions.Count);
                actions[index]();
				actions.RemoveAt(index);
            }
        }

		public void RepairProportional(int percentage, List<Action> actions)
        {
            RepairFixed(Pattern.NumberOfInvalidElements * percentage / 100, actions);
        }

        public void Reset()
        {
            Pattern.Clear();
        }

        public abstract class QueryPattern
        {
            public abstract IEnumerable<Tuple<string, Action>> GetAvailableActions();

            public abstract int NumberOfInvalidElements { get; }

            public virtual void Clear() { }
        }
    }

    class IncrementalTrainRepair : TrainRepair
    {
        private class QueryPattern<T> : QueryPattern
        {
            public INotifyEnumerable<T> Source { get; set; }
            public Func<T, string> SortKey { get; set; }
            public Action<T> Action { get; set; }

            public override IEnumerable<Tuple<string,Action>> GetAvailableActions()
            {
                return from match in Source.AsEnumerable()
					select new Tuple<string,Action>(SortKey(match), () => Action(match));
            }

            public override int NumberOfInvalidElements
            {
                get { return Source.AsEnumerable().Count(); }
            }

            public override void Clear()
            {
                Source.Detach();
            }
        }

        protected override void Fix<T>(IEnumerableExpression<T> pattern, Action<T> action, Func<T, string> sortKey)
        {
            Pattern = new QueryPattern<T>() { Source = pattern.AsNotifiable(), Action = action, SortKey = sortKey };
        }

        protected override void Inject<T>(IEnumerableExpression<T> pattern, Action<T> action, Func<T, string> sortKey)
        {
            InjectPattern = new BatchTrainRepair.QueryPattern<T>() { Source = pattern, Action = action, SortKey = sortKey };
        }
    }

    class BatchTrainRepair : TrainRepair
    {
        public class QueryPattern<T> : QueryPattern
        {
            public IEnumerable<T> Source { get; set; }
            public Func<T, string> SortKey { get; set; }
            public Action<T> Action { get; set; }

            public override IEnumerable<Tuple<string, Action>> GetAvailableActions()
            {
                return (from match in Source
					select new Tuple<string, Action>(SortKey(match), () => Action(match))).ToList();
            }

            public override int NumberOfInvalidElements
            {
                get { return Source.Count(); }
            }
        }

        protected override void Fix<T>(IEnumerableExpression<T> pattern, Action<T> action, Func<T, string> sortKey)
        {
            Pattern = new QueryPattern<T>() { Source = pattern, Action = action, SortKey = sortKey };
        }

        protected override void Inject<T>(IEnumerableExpression<T> pattern, Action<T> action, Func<T, string> sortKey)
        {
            InjectPattern = new QueryPattern<T>() { Source = pattern, Action = action, SortKey = sortKey };
        }
    }


}
