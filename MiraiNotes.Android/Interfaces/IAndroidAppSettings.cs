namespace MiraiNotes.Android.Interfaces
{
    public interface IAndroidAppSettings
    {
        void SetString(string key, string value);

        string GetString(string key);

        void SetInt(string key, int value);

        int GetInt(string key);

        void SetBoolean(string key, bool value);

        bool GetBoolean(string key);
    }
}