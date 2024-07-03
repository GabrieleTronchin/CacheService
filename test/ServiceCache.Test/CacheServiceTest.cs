using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ServiceCache.Test
{
    [TestClass]
    public class CacheServiceTest
    {
        private readonly Mock<IDistributedCache> _distributedCache = new Mock<IDistributedCache>();
        private readonly Mock<IOptions<CacheOptions>> _option = new Mock<IOptions<CacheOptions>>();

        private ICacheService GetObjectToTest()
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            ILogger<CacheService> logger = loggerFactory.CreateLogger<CacheService>();


            return new CacheService(logger, _option.Object, _distributedCache.Object);
        }

        [TestMethod]
        [DataRow("TestKey")]
        public void CreateAndSet(string key)
        {
            _distributedCache.Reset();
            _option.Reset();

            _option.Setup(x => x.Value).Returns(new CacheOptions() { SlidingExpirationToNowInMinutes = 1 });

            var objectToTest = GetObjectToTest();

            objectToTest.CreateAndSet(key, "A");

            _distributedCache.Verify(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),It.IsAny<CancellationToken>()), Times.Once);
        }



        [TestMethod]
        [DataRow("TestKey")]
        public async Task CreateAndSetAsync(string key)
        {
            _distributedCache.Reset();
            _option.Reset();

            _option.Setup(x => x.Value).Returns(new CacheOptions() { SlidingExpirationToNowInMinutes = 1 });

            var objectToTest = GetObjectToTest();

            await objectToTest.CreateAndSetAsync(key, () => Task.FromResult("A"));

            _distributedCache.Verify(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [DataRow("TestKey")]
        public async Task RemoveAsync(string key)
        {
            _distributedCache.Reset();
            _option.Reset();

            _option.Setup(x => x.Value).Returns(new CacheOptions() { SlidingExpirationToNowInMinutes = 1 });

            var objectToTest = GetObjectToTest();

            await objectToTest.RemoveAsync(key);

            _distributedCache.Verify(x => x.Remove(key), Times.Once);

        }


        [TestMethod]
        [DataRow("TestKey")]
        public async Task GetOrDefault(string key)
        {
            _distributedCache.Reset();
            _option.Reset();

            _option.Setup(x => x.Value).Returns(new CacheOptions() { SlidingExpirationToNowInMinutes = 1 });

            var objectToTest = GetObjectToTest();

            await objectToTest.GetOrDefault(key, () => Task.FromResult("A"));


            _distributedCache.Verify(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);

        }

    }
}