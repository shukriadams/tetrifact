using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tetrifact.Core
{
    public class Shell
    {
        /// <summary>
        /// Runs a shell command SYNCHRONOUSLY, returns a tuple with stdout and stderr.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static (IEnumerable<string>, IEnumerable<string>) Run(string command)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "sh";
            cmd.StartInfo.Arguments = $"-c \"{command}\"";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            List<string> stdOut = new List<string>();
            List<string> stdErr = new List<string>();

            while (!cmd.StandardOutput.EndOfStream)
            {
                string line = cmd.StandardOutput.ReadLine();
                stdOut.Add(line);
                Console.WriteLine(line);
            }

            while (!cmd.StandardError.EndOfStream)
            {
                string line = cmd.StandardError.ReadLine();
                stdErr.Add(line);
                Console.WriteLine(line);
            }

            return (stdOut, stdErr);
        }
    }
}