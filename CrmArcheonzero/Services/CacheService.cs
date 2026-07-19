using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CrmArcheonzero.Services
{
    public class CacheService<T> where T : class
    {
        private readonly ConcurrentDictionary<string, CacheItem<T>> _cache = new();
        private readonly int _expirationMinutes;
        private readonly Timer _cleanupTimer;

        public CacheService(int expirationMinutes = 5)
        {
            _expirationMinutes = expirationMinutes;
            _cleanupTimer = new Timer(Cleanup, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public T GetOrAdd(string key, Func<T> factory)
        {
            if (_cache.TryGetValue(key, out var cacheItem) && !cacheItem.IsExpired)
                return cacheItem.Value;

            var value = factory();
            _cache[key] = new CacheItem<T>(value, _expirationMinutes);
            return value;
        }

        public void Invalidate(string key) => _cache.TryRemove(key, out _);
        public void Clear() => _cache.Clear();

        private void Cleanup(object? state)
        {
            var keysToRemove = new List<string>();
            foreach (var kvp in _cache)
                if (kvp.Value.IsExpired)
                    keysToRemove.Add(kvp.Key);

            foreach (var key in keysToRemove)
                _cache.TryRemove(key, out _);
        }
    }

    public class CacheItem<T>
    {
        public T Value { get; }
        public DateTime ExpiresAt { get; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public CacheItem(T value, int expirationMinutes)
        {
            Value = value;
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        }
    }
}