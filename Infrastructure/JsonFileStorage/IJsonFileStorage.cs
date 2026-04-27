namespace DutyPlanner.Infrastructure.JsonFileStorage
{
    public interface IJsonFileStorage
    {
        bool Exists(string path);
        T Load<T>(string path);
        void Save<T>(string path, T data);

        Task<T> LoadAsync<T>(string path);
        Task SaveAsync<T>(string path, T data);
    }
}
