using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Tetrifact.Tests
{
    public class JsonHelper
    {

        /// <summary>
        /// Common method for convert JsonResult from API endjoing to dynamic object
        /// </summary>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(ActionResult actionResult)
        {
            if (actionResult == null)
                throw new Exception("actionResult cannot be null if dynamic conversion is needed");

            string jrawJson;
            if (actionResult is NotFoundObjectResult)
            {
                NotFoundObjectResult notFound = actionResult as NotFoundObjectResult;
                jrawJson = JsonConvert.SerializeObject(notFound.Value);
            }
            else if (actionResult is JsonResult)
            {
                JsonResult jsonResult = actionResult as JsonResult;
                jrawJson = JsonConvert.SerializeObject(jsonResult.Value);
            }
            else 
                throw new Exception($"actionResult {actionResult} is not expected here, likely internal error");

            return JsonConvert.DeserializeObject(jrawJson);
        }

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
