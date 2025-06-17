namespace Core.Feature.Save
{
    public interface ISaveFeature
    {
        void Clear(SaveFileVariant variant);
        bool IsKeyPresent(string key, SaveFileVariant variant = SaveFileVariant.General);

        T Load<T>(string key, T defaultValue, SaveFileVariant variant = SaveFileVariant.General);
        void Save<T>(T value, string key, SaveFileVariant variant = SaveFileVariant.General);

        void DeleteKey(string key, SaveFileVariant variant);
    }
}