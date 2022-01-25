using Newtonsoft.Json.Linq;
using System.IO;

namespace Tetrifact.Tests
{
    public class JsonHelper
    {
        /// <summary>
        /// Writes a value to the root level of a JSON file
        /// </summary>
        /// <param name="jsonFilePath"></param>
        /// <param name="entry"></param>
        /// <param name="value"></param>
        public static void WriteValuetoRoot(string jsonFilePath, string entry, JToken value)
        {
            JObject json = JObject.Parse(File.ReadAllText(jsonFilePath));
            json[entry] = value;
            File.WriteAllText(jsonFilePath, json.ToString());
        }
    }
}
