using System;
using System.Diagnostics;
using System.Threading;

namespace Tetrifact.DevUtils.Generators
{
    public class StressTester
    {
        Process _serverProcess;

        public void Start() 
        {
            int threads = 10;

            StartServer();

            // start x nr of threads
            for (int i = 0; i < threads; i++) 
            {
                Thread thread = new Thread(Work);
                thread.Start();
            }

            // thread can either add, delete or retrieve a package
        }

        private void StartServer() 
        {
            ProcessStartInfo serverStartInfo = new ProcessStartInfo("exe abs path ");
            serverStartInfo.WorkingDirectory = "exe folder";
            serverStartInfo.Arguments = $"-my params";

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

        }

        private void Retrieve()
        {

        }

        private void Delete()
        {

        }

        private void Work() 
        {
            while (true) 
            {
                try
                {
                    Random r = new Random();
                    int action = r.Next(0,2);
                    switch (action) 
                    {
                        case 0:
                            {
                                // create
                                Create();
                                break;
                            }
                        case 1:
                            {
                                // retrieve
                                Retrieve();
                                break;
                            }
                        case 2:
                            {
                                // delete
                                Delete();
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
