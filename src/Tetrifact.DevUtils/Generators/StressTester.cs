using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Tetrifact.Dev;

namespace Tetrifact.DevUtils
{
    public class StressTester
    {
        Process _serverProcess;

        const string url = "http://127.0.0.1:3000";
        string workingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stressTests");

        public void Curl(string command, string workingDirectory) 
        {
            ProcessStartInfo serverStartInfo = new ProcessStartInfo("curl");
            if (workingDirectory != null)
                serverStartInfo.WorkingDirectory = workingDirectory;
            serverStartInfo.Arguments = command;

            Process process = new Process();
            process.StartInfo = serverStartInfo;
            process.Start();
        }

        public void Start() 
        {
            int threads = 10;

            if (Directory.Exists(workingDirectory))
                Directory.Delete(workingDirectory, true);

            Thread.Sleep(100);
            Directory.CreateDirectory(workingDirectory);


            StartServer();

            Thread.Sleep(5000);
            //Curl($"-X DELETE {url}/v1/projects/stressTest", null);
            Curl($"-X POST {url}/v1/projects/stressTest", null);

            // start x nr of threads
            for (int i = 0; i < threads; i++) 
            {
                Thread thread = new Thread(Work);
                thread.Name = $"Worker {i}";
                thread.Start();
            }

            // thread can either add, delete or retrieve a package
        }

        private void StartServer() 
        {
            ProcessStartInfo serverStartInfo = new ProcessStartInfo("dotnet");
            serverStartInfo.WorkingDirectory = "../../../../";
            serverStartInfo.Arguments = $"run --project Tetrifact.Web";

            _serverProcess = new Process();
            _serverProcess.StartInfo = serverStartInfo;
            _serverProcess.EnableRaisingEvents = true;
            _serverProcess.Exited += Server_Exited;

            Thread thread = new Thread(new ThreadStart(delegate () {
                Console.WriteLine("Starting server");
                _serverProcess.Start();
            }));

            thread.Start();
        }

        private async void Server_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("server exited");
        }

        private void Create() 
        {
            Console.WriteLine(Thread.CurrentThread.Name + " creating");

            List<DummyFile> inFiles = new List<DummyFile>();
            for (int i = 0; i < 50; i++)
                inFiles.Add(new DummyFile
                {
                    Data = DataHelper.GetRandomData(1, 100),
                    Path = Guid.NewGuid().ToString()
                });

            string packageName = Guid.NewGuid().ToString();
            string filename = $"{packageName}.zip";

            // create package from files array, zipped up
            using (Stream zipStream = ArchiveHelper.ZipStreamFromFiles(inFiles)) 
            {
                using (Stream fileStream = File.Create(Path.Combine(workingDirectory, filename)))
                {
                    zipStream.Seek(0, SeekOrigin.Begin);
                    zipStream.CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            

            ProcessStartInfo startInfo = new ProcessStartInfo("curl");
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.Arguments = $"-X POST -H \"Content-Type: multipart/form-data\" -F \"Files=@{filename}\" {url}/v1/packages/stressTest/{packageName}?isArchive=true ";
            startInfo.RedirectStandardInput = true;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            Thread thread = new Thread(new ThreadStart(delegate () {
                process.Start();
            }));

            thread.Start();

        }

        private void Retrieve()
        {
            Console.WriteLine(Thread.CurrentThread.Name + " retrieving");

        }

        private void Delete()
        {
            Console.WriteLine(Thread.CurrentThread.Name + " deleting");
        }

        private void Work() 
        {
            while (true) 
            {
                Random r = new Random();

                try
                {
                    int action = r.Next(0,2);
                    switch (action) 
                    {
                        case 0:
                            {
                                Create();
                                break;
                            }
                        case 1:
                            {
                                Retrieve();
                                break;
                            }
                        case 2:
                            {
                                Delete();
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Thread.Sleep(r.Next(500, 5000));
            }
        }
    }
}
