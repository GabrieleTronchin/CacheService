using BenchmarkDotNet.Running;
using CacheService.ConsoleApp;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

Benchmark benchmark = new();

await benchmark.Execute();

Console.ReadLine();
