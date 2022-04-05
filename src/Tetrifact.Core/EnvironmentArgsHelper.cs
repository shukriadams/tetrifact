using System;

namespace Tetrifact.Core
{
    public class EnvironmentArgsHelper
    {
        public static bool GetAsBool(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (value == null)
                value = string.Empty;

            value = value.ToLower();

            return value == "true" || value == "1";
        }

        public static int GetAsInt(string name, int defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(name);
            int val;
            if (!int.TryParse(Environment.GetEnvironmentVariable(name), out val)){
                Console.WriteLine($"Environment variable {value} expected to be integer, but could not be parsed, falling back to default {defaultValue}");
                return defaultValue;
            }

            return val;
        }
    }
}
