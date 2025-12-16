namespace Common.Caching
{
    /// <summary>
    /// Implementación de ICacheService que no hace cache (para testing/debugging)
    /// </summary>
    public class NoCacheService : ICacheService
    {
        public Task<T> GetAsync<T>(string key)
        {
            // Siempre retorna default (como si no hubiera cache)
            return Task.FromResult(default(T)!);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            // No hace nada (no guarda en cache)
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            // No hace nada (no hay nada que remover)
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            // Siempre retorna false (como si no existiera)
            return Task.FromResult(false);
        }
    }
}