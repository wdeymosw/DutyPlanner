namespace DutyPlanner.Infrastrustures.JsonFileStorage
{
    public interface IJsonFileStorage
    {
        bool Exists(string path);
        T Load<T>(string path);
        void Save<T>(string path, T data);
    }
}

