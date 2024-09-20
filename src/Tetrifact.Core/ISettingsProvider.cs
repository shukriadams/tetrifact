namespace Tetrifact.Core
{
    public interface ISettingsProvider
    {
        Settings Get();

        /// <summary>
        /// Load app using provided YML config
        /// </summary>
        /// <returns></returns>
        Settings Get(string ymlConfig);
    }
}
