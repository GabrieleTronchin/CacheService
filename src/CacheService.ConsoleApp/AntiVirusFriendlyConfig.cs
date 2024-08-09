using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace CacheService.ConsoleApp;

public class AntiVirusFriendlyConfig : ManualConfig
{
    public AntiVirusFriendlyConfig()
    {
        AddJob(Job.MediumRun
            .WithWarmupCount(1)
            .WithIterationCount(1)
            .WithToolchain(InProcessNoEmitToolchain.Instance));
    }
}