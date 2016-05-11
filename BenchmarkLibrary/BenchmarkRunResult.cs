using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BenchmarkLibrary
{
    public class BenchmarkRunResult
    {
        public BenchmarkSettings Settings;
        public List<ExecutionInformation> Runs;
    }

    public class ExecutionInformation
    {
        public int Iteration;
        public BenchmarkAction Action;
        public long ElapsedMilliseconds;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BenchmarkAction
    {
        Read,
        Check,
        Repair,
        Recheck,
        ValidateFailed
    }
}