using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tetrifact.Core
{
    public class ShellResult
    {
        public int ExitCode {get;set;}
        public IEnumerable<string> StdOut {get; set; }
        public IEnumerable<string> StdErr {get; set; }
        
        public ShellResult(int exitCode, IEnumerable<string> stdOut, IEnumerable<string> stdErr)
        {
            this.ExitCode = exitCode;
            this.StdOut = stdOut;
            this.StdErr = stdErr;
        }
    }

    public class Shell
    {
        /// <summary>
        /// Runs a shell command SYNCHRONOUSLY, returns a tuple with exit code, stdout and stderr.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static ShellResult Run(string command, bool verbose=false)
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
                if (verbose)
                    Console.WriteLine(line);
            }

            while (!cmd.StandardError.EndOfStream)
            {
                string line = cmd.StandardError.ReadLine();
                stdErr.Add(line);
                if (verbose)
                    Console.WriteLine(line);
            }
            
            return new ShellResult(cmd.ExitCode, stdOut, stdErr);
        }
    }
}
