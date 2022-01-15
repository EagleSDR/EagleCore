using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace EagleWeb.Launcher
{
    class EagleApplication
    {
        public EagleApplication(FileInfo executable, string args, string eagleDir)
        {
            this.executable = executable;
            this.args = args;
            this.eagleDir = eagleDir;
        }

        private FileInfo executable;
        private string args;
        private string eagleDir;
        private Process process;
        private EagleApplicationManager openEditor;

        public bool IsRunning => process != null && !process.HasExited;
        public bool IsLocked => openEditor != null;

        public FileInfo Executable => executable;
        public DirectoryInfo ExecutableDirectory => executable.Directory;
        public string EagleDirectory => eagleDir;

        public void Start()
        {
            //Validate
            EnsureUnlocked();

            //If it's already running, abort
            if (IsRunning)
                throw new Exception("Process is already running.");

            //Log
            Console.WriteLine("### Application started.");

            //Start
            process = Process.Start(new ProcessStartInfo
            {
                FileName = executable.FullName,
                Arguments = args,
                UseShellExecute = false
            });
        }

        public void Stop()
        {
            //If it's not running, do nothing
            if (!IsRunning)
                return;

            //Kill (we'll likely stop it more gracefully later)
            process.Kill();

            //Wait a sec
            Thread.Sleep(500);

            //Log
            Console.WriteLine("### Application stopped.");
        }

        public EagleApplicationManager Configure()
        {
            //Validate
            EnsureUnlocked();

            //Save state and stop
            bool running = IsRunning;
            Stop();

            //Create
            openEditor = new EagleApplicationManager(this, () =>
            {
                //Release lock
                openEditor = null;

                //Restart if it was running
                if (running)
                    Start();
            });

            return openEditor;
        }

        private void EnsureUnlocked()
        {
            if (IsLocked)
                throw new Exception("This application currently has an open editor. Make sure you're calling dispose on the editors.");
        }
    }
}
