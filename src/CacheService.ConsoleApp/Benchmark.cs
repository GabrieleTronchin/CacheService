namespace CacheService.ConsoleApp;

public class Benchmark
{
    public async Task Execute()
    {
        var cacheBenchmark = new CacheServiceBenchmark();

        await cacheBenchmark.AddSmallObject();

        await cacheBenchmark.GetSmallObject();

        await cacheBenchmark.AddBigObject();

        await cacheBenchmark.GetBigObject();
    }
}
