using System.Reflection;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ServiceCache;

public class CacheService : ICacheService
{
    private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly CacheOptions _options;

    public CacheService(
        ILogger<CacheService> logger,
        IOptions<CacheOptions> options,
        IDistributedCache cache
    )
    {
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> create,
        int expirationMinutes = 0
    )
    {
        var bytesResult = await GetAsync(key);

        if (bytesResult?.Length > 0)
        {
            using StreamReader reader = new(new MemoryStream(bytesResult));
            using JsonTextReader jsonReader = new(reader);
            JsonSerializer ser = new();
            ser.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            ser.TypeNameHandling = TypeNameHandling.All;
            ser.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

            var result = ser.Deserialize<T>(jsonReader);
            if (result != null)
            {
                return result;
            }
        }

        return await this.CreateAndSetAsync<T>(key, create, expirationMinutes);
    }

    public async Task<T> GetOrCreateParallelAsync<T>(
        string key,
        Func<Task<T>> create,
        int expirationMinutes = 0
    )
    {
        var bytesResult = await GetAsync(key);

        if (bytesResult?.Length > 0)
        {
            using StreamReader reader = new(new MemoryStream(bytesResult));
            using JsonTextReader jsonReader = new(reader);
            JsonSerializer ser = new();
            ser.TypeNameHandling = TypeNameHandling.All;
            ser.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

            var result = ser.Deserialize<T>(jsonReader);
            if (result != null)
            {
                return result;
            }
        }

        return await this.CreateAndSetParalelAsync<T>(key, create, expirationMinutes);
    }


    public async Task<T?> GetOrDefault<T>(string key)
    {
        var bytesResult = await GetAsync(key);

        if (bytesResult?.Length > 0)
        {
            using StreamReader reader = new(new MemoryStream(bytesResult));
            using JsonTextReader jsonReader = new(reader);
            JsonSerializer ser = new();
            ser.TypeNameHandling = TypeNameHandling.All;
            ser.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

            var result = ser.Deserialize<T>(jsonReader);
            if (result != null)
            {
                return result;
            }
        }

        return default;
    }

    public async Task<T> GetOrDefault<T>(string key, T defaultVal)
    {
        var bytesResult = await GetAsync(key);

        if (bytesResult?.Length > 0)
        {
            using StreamReader reader = new(new MemoryStream(bytesResult));
            using JsonTextReader jsonReader = new(reader);
            JsonSerializer ser = new();
            ser.TypeNameHandling = TypeNameHandling.All;
            ser.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

            var result = ser.Deserialize<T>(jsonReader);
            if (result != null)
            {
                return result;
            }
        }

        return defaultVal;
    }

    public async Task CreateAndSet<T>(string key, T thing, int expirationMinutes = 0)
        where T : class
    {
        try
        {
            await RemoveAsync(key);

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializerSettings.TypeNameHandling = TypeNameHandling.All;
            serializerSettings.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

            var json = JsonConvert.SerializeObject(thing, serializerSettings);

            await SetAsync(
                key,
                Encoding.ASCII.GetBytes(json),
                GetCacheExpirationOptions(expirationMinutes)
            );

            _logger.LogTrace(
                string.Format(
                    "{0} - Item with key {1} added to cache.",
                    nameof(CreateAndSetAsync),
                    key.ToString()
                )
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                string.Format("An error occurred at {0}.", nameof(CreateAndSetAsync))
            );
            throw;
        }
    }

    public async Task<T> CreateAndSetAsync<T>(
        string key,
        Func<Task<T>> createAsync,
        int expirationMinutes = 0
    )
    {
        T thing;
        try
        {
            await Remove(key);
            thing = await createAsync();

            JsonSerializerSettings serializerSettings =
                new()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                };

            var json = JsonConvert.SerializeObject(thing, serializerSettings);

            await SetAsync(
                key,
                Encoding.ASCII.GetBytes(json),
                GetCacheExpirationOptions(expirationMinutes)
            );

            _logger.LogTrace(
                string.Format(
                    "{0} - Item with key {1} added to cache.",
                    nameof(CreateAndSetAsync),
                    key
                )
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"An error occurred at {nameof(CreateAndSetAsync)}.");
            throw;
        }

        return thing;
    }

    private DistributedCacheEntryOptions GetCacheExpirationOptions(int expirationMinutes = 0)
    {
        if (expirationMinutes <= 0)
            expirationMinutes = _options.SlidingExpirationToNowInMinutes;

        return new DistributedCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromMinutes(expirationMinutes)
        };
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await Remove(key);

            _logger.LogTrace(
                string.Format("{0} - Item with key {1} removed to cache.", nameof(RemoveAsync), key)
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"An error occurred at {nameof(RemoveAsync)}.");
            throw;
        }
    }

    private async Task<byte[]?> GetAsync(string key)
    {
        await Locker.WaitAsync();
        try
        {
            var bytesResult = await _cache.GetAsync(key);
            return bytesResult;
        }
        finally
        {
            Locker.Release();
        }
    }

    private async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        await Locker.WaitAsync();
        try
        {
            await _cache.SetAsync(key, value, options);
        }
        finally
        {
            Locker.Release();
        }
    }

    private async Task Remove(string key)
    {
        await Locker.WaitAsync();
        try
        {
            _cache.Remove(key);
        }
        finally
        {
            Locker.Release();
        }
    }
}
