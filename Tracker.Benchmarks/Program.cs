using BenchmarkDotNet.Running;
using Tracker.Benchmarks;

//BenchmarkRunner.Run<HashersBenchamrk>();
//return;

//BenchmarkRunner.Run<ReferenceEqualVsManuallStringCompare>();
//return;

Console.WriteLine(new ETagComparerBenchmark().Compare_Equal_PartialGenerate_BuildETagV2());

BenchmarkRunner.Run<ETagComparerBenchmark>();
