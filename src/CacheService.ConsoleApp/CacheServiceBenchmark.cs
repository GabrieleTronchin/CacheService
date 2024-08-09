using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceCache;

namespace CacheService.ConsoleApp;

public class CacheServiceBenchmark
{
    private const string FILE_NAME = "commedia.txt";
    private readonly string bigObject;
    private readonly string smallObjectKey;
    private readonly string bigObjectKey;

    [Params("redis", "garnet")]
    public string ConfigurationFile { get; set; } = "redis";

    private ICacheService GetCacheService()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{this.ConfigurationFile}.json", true, true)
            .Build();

        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddServiceCache(configuration)
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<ICacheService>();
    }

    public CacheServiceBenchmark()
    {
        bigObject = File.ReadAllText(FILE_NAME);
        smallObjectKey = Guid.NewGuid().ToString();
        bigObjectKey = Guid.NewGuid().ToString();
    }

    [Benchmark]
    public async Task AddSmallObject()
    {
        var cacheService = GetCacheService();

        await cacheService.CreateAndSet(smallObjectKey, "Test");
    }

    [Benchmark]
    public async Task GetSmallObject()
    {
        var cacheService = GetCacheService();

        await cacheService.GetOrDefaultAsync(smallObjectKey, "Test");
    }

    [Benchmark]
    public async Task AddBigObject()
    {
        var cacheService = GetCacheService();

        await cacheService.CreateAndSet(bigObjectKey, bigObject);
    }

    [Benchmark]
    public async Task GetBigObject()
    {
        var cacheService = GetCacheService();

        await cacheService.GetOrDefaultAsync(bigObjectKey, bigObject);
    }
}
