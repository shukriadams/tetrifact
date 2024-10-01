using System;
using System.IO.Abstractions;
using YamlDotNet.Serialization;

namespace Tetrifact.Core
{
    public class DefaultSettingsProvider : ISettingsProvider
    {
        #region FIELDS

        private static Settings _settings;

        private readonly IFileSystem _fileSystem;

        #endregion

        #region CTORS

        public DefaultSettingsProvider(IFileSystem fileSystem) 
        {
            _fileSystem = fileSystem;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// load settints from app's default expected config location
        /// </summary>
        /// <returns></returns>
        public Settings Get()
        { 
            return this._Get();
        }

        /// <summary>
        /// Load app using provided YML config
        /// </summary>
        /// <returns></returns>
        public Settings Get(string ymlConfig)
        {
            return this._Get(ymlConfig);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ymlTextContent"></param>
        /// <returns></returns>
        private Settings _Get(string ymlTextContent = null)
        {
            if (_settings == null)
            {
                _settings = new Settings();

                // try to get YML config location from env var, all other settings will be loaded from this file. Note that this value can be overwritten by a different settings path 
                // the YML config file, but that value will never be used.
                _settings.SettingsPath = TryGetSetting("TETRIFACT_SETTINGS_PATH", _settings.SettingsPath);

                // load YML config from filesystem if it has not been directly passed in 
                if (ymlTextContent == null)
                {
                    if (_fileSystem.File.Exists(_settings.SettingsPath))
                    {
                        try
                        {
                            ymlTextContent = _fileSystem.File.ReadAllText(_settings.SettingsPath);
                        }
                        catch(Exception ex)
                        { 
                            throw new Exception($"Failed to load YML config from path {_settings.SettingsPath} : {ex}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No YML config provided for settings, and YML config found on disk. Falling back to defaults for everything");
                    }

                    if (!string.IsNullOrEmpty(ymlTextContent))
                    {
                        IDeserializer deserializer = YmlHelper.GetDeserializer();
                        _settings = deserializer.Deserialize<Settings>(ymlTextContent);
                    }
                }
            }

            return _settings;
        }

        private string TryGetSetting(string settingsName, string defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);

            if (settingsRawVariable == null)
                return defaultValue;

            return settingsRawVariable;
        }

        #endregion
    }
}
