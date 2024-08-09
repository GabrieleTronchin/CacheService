namespace CacheService.ConsoleApp;

public class Benchmark
{
    public async Task Execute()
    {
        CacheServiceBenchmark cacheBenchmark = new();

        await cacheBenchmark.AddSmallObject();

        await cacheBenchmark.GetSmallObject();

        await cacheBenchmark.AddBigObject();

        await cacheBenchmark.GetBigObject();
    }
}
