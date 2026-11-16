using Microsoft.Extensions.Caching.Memory;

namespace ShoppingPlatformMVC.Services
{
    public class DemoCartStore(IMemoryCache cache)
    {
        public SemaphoreSlim Gate { get; } = new(1, 1);

        public Dictionary<int, int> GetCart(string id)
        {
            return cache.GetOrCreate($"demo-cart:{id}", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(2);
                return new Dictionary<int, int>();
            })!;
        }
    }
}