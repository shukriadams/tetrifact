using System;
using System.IO;
using System.Threading;

namespace Tetrifact.Tests
{
    public class TestSetupHelper
    {
        /// <summary>
        /// Sets up test directories for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string SetupDirectories(object context) 
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, context.GetType().FullName);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            Thread.Sleep(200);// race condition fix

            return testFolder;
        }
    }
}
