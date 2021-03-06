﻿using _7DTDManager.Commands;
using _7DTDManager.Config;
using _7DTDManager.Interfaces;
using _7DTDManager.Localize;
using _7DTDManager.Objects;
using _7DTDManager.Players;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _7DTDManager
{
    class Program
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        public static string VERSION = "V1.5";
        public static string HELLO = String.Format("This Server runs 7DTDManager Version {0}. See /help for available commands.", VERSION);

        public static String ApplicationDirectory
        {
            get { return _ApplicationDirectory ?? (_ApplicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); }
        } private static String _ApplicationDirectory;

        public static Manager Server;
        public static Configuration Config
        {
            get { return _Config; }
            set {
                if (_Config != null)
                    _Config.Deinit();
                _Config = value;
                _Config.Init();
            }
        } private static Configuration _Config;

        public static IPlayer ServerPlayer = new ServerPlayer { Name = "Server" };

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture; 
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            ConfigureLogging();
           

            Config = Configuration.Load();
                        
            Config.UpdateDefaults();

            if (Config.IsNewConfiguration)
            {
                Config.Save();
                Console.WriteLine("Configuration not found. A default-config has been created for you. Please change to your needs and restart the application.");
                return;
            }

            MessageLocalizer.Init();
            MessageLocalizer.InitTranslation("english-default");
            MessageLocalizer.SaveTranslation();

            Server = new Manager();
            
            PlayersManager.Init();
            
            ExtensionManager.LoadExtensions(Server);

            PlayersManager.Load();

            CommandManager.Init();
            PositionManager.Init();

            
            PlayersManager.Instance.RegisterPlayers();

            Server.Connect();
            
            while (1 == 1)
            {
                    string cline = Console.ReadLine();
                    if (cline == "exit")
                    {
                        Server.AllPlayers.Save();
                        LogManager.Flush();
                        return;
                    }
                    else
                    {
                        
                       
                        if (cline.StartsWith("!"))
                        {
                            Program.Server.Execute(cline.Substring(1));
                            continue;
                        }
                         
                        _7DTDManager.LineHandlers.lineServerCommand c = new _7DTDManager.LineHandlers.lineServerCommand();
                        c.ProcessLine(Program.Server, "INF GMSG: Server: /" + cline);
                        //bool res = cmd.Execute(Server, ServerPlayer, largs);
                    }                
            }
        }

        private static void ConfigureLogging()
        {
            bool bBuiltIn = false;
            string logPath = Path.Combine(ApplicationDirectory, "logs");

            Directory.CreateDirectory(logPath);

           
            try { File.Delete(Path.Combine(logPath, "7dtdmanager.log.20")); }
            catch { }
            for (int i = 19; i >= 0; i--)
            {
                try { File.Move(Path.Combine(logPath, "7dtdmanager.log." + i.ToString()), Path.Combine(logPath, "7dtdmanager.log." + (i + 1).ToString())); }
                catch { }

            }
            try { File.Move(Path.Combine(logPath, "7dtdmanager.log"), Path.Combine(logPath, "7dtdmanager.log.0")); }
            catch { }

            if (!File.Exists(Path.Combine(ApplicationDirectory, "nlog.config")))
            {
                LoggingConfiguration config = new LoggingConfiguration();


                FileTarget fileTarget = new FileTarget();

                config.AddTarget("file", fileTarget);
                fileTarget.FileName = "${basedir}/logs/7dtdmanager.log";
                fileTarget.Encoding = Encoding.UTF8;
                fileTarget.Layout = "${longdate}|${level:uppercase=true}|${threadid}|${logger}|${message}";

               // AsyncTargetWrapper asyncFileTarget = new AsyncTargetWrapper(fileTarget, 5000, AsyncTargetWrapperOverflowAction.Discard);

                NLogViewerTarget nlogTarget = new NLogViewerTarget();
                nlogTarget.Address = "udp://127.0.0.1:9999";
#if DEBUG
                // To nicely view the nlogtarget i prefer Log4View (free) http://www.log4view.de/log4view/
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget));
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, nlogTarget));
#else
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, new ConsoleTarget()));
#endif
                // config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, nlogTarget));
                //config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, nlogTarget));
                LogManager.Configuration = config;
                bBuiltIn = true;
            }
            LogManager.EnableLogging();
            logger = LogManager.GetCurrentClassLogger();
            logger.Info(">>> START 7dtdmanager Version {0}", Assembly.GetEntryAssembly().FullName);
            logger.Info("Using {0} configuration", (bBuiltIn ? "Builtin" : "Configfile"));
        }
    }
}
