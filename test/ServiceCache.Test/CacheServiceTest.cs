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
            try
            {
                objectToTest.CreateAndSet(key, "A");
            }
            catch (Exception)
            {
                Assert.Fail("Cannot create cache");
            }
            //_distributedCache.Verify(x => x.SetAsync(key, It.IsAny<byte[]>()), Times.Once);
        }
    }
}