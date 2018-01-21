using System;
using System.Runtime.Caching;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal static class DeviceFacadeMemoryCacheExtensions
    {
        public static void AddEventToCache(
            [NotNull] this MemoryCache cache,
            [NotNull] ICycleProcessingContextParams contextParams,
            TimeSpan lifeTime)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (contextParams == null) throw new ArgumentNullException(nameof(contextParams));

            cache.Add(new CacheItem(contextParams.UniqObjectId.ToString(), contextParams), new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow + lifeTime
            });
        }

        public static bool ContainsEvent(
            [NotNull] this MemoryCache cache,
            [NotNull] ICycleProcessingContextParams contextParams)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (contextParams == null) throw new ArgumentNullException(nameof(contextParams));

            return cache.Contains(contextParams.UniqObjectId.ToString());
        }
    }
}