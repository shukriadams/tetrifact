using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                cmd.StartInfo.FileName = "sh";
                cmd.StartInfo.Arguments = $"-c \"{command}\"";
            }
            else
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = $"/k {command}";
            }

            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            List<string> stdOut = new List<string>();
            List<string> stdErr = new List<string>();
            int timeout = 50000;


            //cmd.Start();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    cmd.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            outputWaitHandle.Set();
                        else
                            stdOut.Add(e.Data);
                    };

                    cmd.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                            stdErr.Add(e.Data);
                    };

                    cmd.Start();
                    cmd.BeginOutputReadLine();
                    cmd.BeginErrorReadLine();

                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || (cmd.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout)))
                        return new ShellResult(cmd.ExitCode, stdOut, stdErr);
                    else
                        throw new Exception("Time out");
                }
            }
            else
            {
                cmd.Start();
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();


                while (!cmd.StandardOutput.EndOfStream)
                {
                    string line = cmd.StandardOutput.ReadLine();
                    stdOut.Add(line);
                }

                while (!cmd.StandardError.EndOfStream)
                {
                    string line = cmd.StandardError.ReadLine();
                    stdErr.Add(line);
                }

                return new ShellResult(cmd.ExitCode, stdOut, stdErr);
            }
        }
    }
}
