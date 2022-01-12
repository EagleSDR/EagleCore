using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.IO.FileSystem;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.Radio;
using EagleWeb.Common.IO.Sockets;
using EagleWeb.Core.Auth;
using EagleWeb.Core.NetObjects;
using EagleWeb.Core.Plugins;
using EagleWeb.Core.Radio;
using EagleWeb.Core.Web;
using EagleWeb.Core.Web.FileSystem;
using EagleWeb.Core.Web.Services;
using EagleWeb.Core.Web.Sockets;
using EagleWeb.Core.Web.WS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace EagleWeb.Core
{
    class EagleContext : IEagleContext, IEagleLogger, IEagleObjectManagerLink
    {
        public EagleContext(string workingPathname)
        {
            //Set
            this.workingPathname = workingPathname;
            Log(EagleLogLevel.INFO, "BOOT", "Using config directory: " + workingPathname);

            //Make core components
            auth = new EagleAuthManager(this, workingPathname + "accounts.json");
            sessions = new EagleSessionManager(auth, workingPathname + "sessions.json");
            objectManager = new EagleNetObjectManager(this);
            sockManager = new EagleSocketManager(this);

            //Make others
            pluginManager = new EaglePluginManager(this, new DirectoryInfo(workingPathname).CreateSubdirectory("plugins"));
            fileManager = new WebFsManager(this, new DirectoryInfo(workingPathname).CreateSubdirectory("home"));
            radio = new EagleRadio(this);

            //Set the control component for web clients
            objectManager.SetControlObject(new EagleControlObject(this));

            //Set up HTTP server
            http = new EagleWebServer(this, 45555);
            http.RegisterService("/api/info", new EagleInfoService(this));
            http.RegisterService("/api/login", new LoginService(this));
            http.RegisterService("/api/asset", new EagleAssetService(pluginManager));

            http.RegisterService("/ws/sock", sockManager);
            http.RegisterService("/ws/rpc", objectManager);

            http.RegisterService("/", new EagleFileService(@"C:\Users\Roman\source\repos\EagleWeb-JS\EagleWebCore\dist\index.html", "text/html"));
            http.RegisterService("/main.js", new EagleFileService(@"C:\Users\Roman\source\repos\EagleWeb-JS\EagleWebCore\dist\main.js", "text/javascript"));
        }

        private readonly string workingPathname;
        private readonly EagleAuthManager auth;
        private readonly EagleSessionManager sessions;
        private readonly EagleWebServer http;
        private readonly EagleNetObjectManager objectManager;
        private readonly EagleSocketManager sockManager;

        private readonly EaglePluginManager pluginManager;
        private readonly WebFsManager fileManager;
        private readonly EagleRadio radio;

        private readonly List<EagleLoadedPlugin> plugins = new List<EagleLoadedPlugin>();

        public int BufferSize => EagleRadio.BUFFER_SIZE;
        public DirectoryInfo Root => new DirectoryInfo(workingPathname);
        public EagleAuthManager Auth => auth;
        public EagleSessionManager Sessions => sessions;
        public EagleSocketManager Sockets => sockManager;
        public EaglePluginManager PluginManager => pluginManager;
        public WebFsManager FileManager => fileManager;
        public IEagleRadio Radio => radio;
        public IEagleObjectManager ObjectManager => objectManager;

        public void Init()
        {
            //Initialize all plugins
            pluginManager.Init();
        }

        public void Run()
        {
            //Run HTTP
            http.RunAsync().GetAwaiter().GetResult();
        }

        public void Log(EagleLogLevel level, string topic, string message)
        {
            //Decide console color from level
            switch (level)
            {
                case EagleLogLevel.DEBUG: Console.ForegroundColor = ConsoleColor.DarkGray; Console.BackgroundColor = ConsoleColor.Black; break;
                case EagleLogLevel.INFO: Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Black; break;
                case EagleLogLevel.WARN: Console.ForegroundColor = ConsoleColor.Yellow; Console.BackgroundColor = ConsoleColor.Black; break;
                case EagleLogLevel.ERROR: Console.ForegroundColor = ConsoleColor.Red; Console.BackgroundColor = ConsoleColor.Black; break;
                case EagleLogLevel.FATAL: Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Red; break;
            }

            //Log
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] [{Thread.CurrentThread.ManagedThreadId}] [{topic}] {message}");
        }

        /* API */

        public IEagleSocketServer RegisterSocketServer(string friendlyName, IEagleSocketHandler handler)
        {
            return sockManager.RegisterServer(friendlyName, handler);
        }

        public WebFsFileStream ResolveFileToken(string token)
        {
            return FileManager.ResolveFileTokenImpl(token);
        }
    }
}

