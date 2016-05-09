using System;

namespace BenchmarkLibrary
{
    public class BenchmarkSettings
    {
        public String Description { get; set; }

        public bool ValidateAgainstTable { get; set; }
        public ExecutionType RunType { get; set; }
        public int Runs { get; set; }
        public int Size { get; set; }
        public string Query { get; set; }
        public string ChangeSet { get; set; } // "fixed" or "10"
        public int IterationCount { get; set; }
    }

    public enum ExecutionType
    {
        Orleans,
        NmfIncremental
    }
}