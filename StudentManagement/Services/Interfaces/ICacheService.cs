namespace StudentManagementSystem.Services.Interfaces
{
    public interface ICacheService
    {
        T? GetData<T>(string key);
        Task<T?> GetDataAsync<T>(string key);
        bool SetData<T>(string key, T value, DateTimeOffset expirationTime);
        Task<bool> SetDataAsync<T>(string key, T value, DateTimeOffset expirationTime);
        object RemoveData(string key);
        Task<object> RemoveDataAsync(string key);
        Task RemoveByPatternAsync(string v);
    }
}
