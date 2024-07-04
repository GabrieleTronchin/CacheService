using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceCache;

namespace CacheService.ConsoleApp;


public class CacheServiceBenchmark
{

    //[Params(1, 10, 100, 1000)]
    //public int NumberOfRow { get; set; }

    [Params("appsettings.memory.json", "appsettings.redis.json", "appsettings.garnet.json")]
    public string ConfigurationFile { get; set; }



    private ICacheService GetCacheService()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile(this.ConfigurationFile, true, true).Build();

        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddServiceCache(configuration)
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<ICacheService>();
    }

    [Benchmark]
    public async Task AddSmallObject()
    {
        var cacheService = GetCacheService();

        await cacheService.CreateAndSet("Test", "Test");
    }

    [Benchmark]
    public async Task GetSmallObject()
    {
        var cacheService = GetCacheService();

        await cacheService.GetOrDefaultAsync("Test", "Test");
    }


    [Benchmark]
    public async Task AddBigObject()
    {
        var cacheService = GetCacheService();

        await cacheService.CreateAndSet("Test2", "Test");
    }


    [Benchmark]
    public async Task GetBigObject()
    {
        var cacheService = GetCacheService();

        await cacheService.GetOrDefaultAsync("Test2", "Test");
    }

}

