namespace CacheService.ConsoleApp;


public class Benchmark
{

    public async Task Execute()
    {

        var cacheBanchmark = new CacheServiceBenchmark();

        await cacheBanchmark.AddSmallObject();

        await cacheBanchmark.GetSmallObject();

        await cacheBanchmark.AddBigObject();

        await cacheBanchmark.GetBigObject();
    }


}
