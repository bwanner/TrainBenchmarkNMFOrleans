using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BenchmarkLibrary
{
    public class BenchmarkSettings
    {
        public String Description { get; set; }
        public ExecutionType RunType { get; set; }
        public int Runs { get; set; }
        public int Size { get; set; }
        public string Query { get; set; }
        public string ChangeSet { get; set; } // "fixed" or "10"
        public int IterationCount { get; set; }
    }

    //var settings = new BenchmarkSettings()
    //{
    //    ChangeSet = "10",
    //    Description = "Test",
    //    IterationCount = 10,
    //    Query = "PosLength",
    //    Runs = 1,
    //    RunType = ExecutionType.Orleans,
    //    Size = 2
    //};


    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecutionType
    {
        Orleans,
        NmfIncremental
    }
}