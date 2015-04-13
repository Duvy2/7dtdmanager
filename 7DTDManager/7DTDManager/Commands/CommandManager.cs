﻿
using _7DTDManager.Interfaces;
using _7DTDManager.Interfaces.Commands;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _7DTDManager.Commands
{
    public static class CommandManager
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        static Dictionary<string, ICommand> allCommands = new Dictionary<string, ICommand>();
        static List<string> loadedDLLs = new List<string>();

        public static IReadOnlyDictionary<string, ICommand> AllCommands
        {
            get { return allCommands as IReadOnlyDictionary<string, ICommand>; }
        }

        static CommandManager()
        {
            
        }

        public static void Init()
        {            
            allCommands["coins"] = new InfoCommand(String.Format("You will get {0} coin(s) per minute playtime (not idletime!) and {1} coin(s) per zombie slain.",Program.Config.CoinsPerMinute,Program.Config.CoinsPerZombiekill));
            allCommands["death"] = new InfoCommand(String.Format("You will loose {0} coin(s) if you die (not exterminated by another player!)",Program.Config.CoinLossPerDeath));
            allCommands["bounties"] = new InfoCommand(String.Format("When you eliminate another player a bounty will be set on your head"));

            RegisterCommandHandlers(System.Reflection.Assembly.GetExecutingAssembly());
            LoadCommands();
        }

        public static void LoadCommands()
        {
            logger.Info("Loading Commands ...");
            String path = System.IO.Path.Combine(Program.ApplicationDirectory, "cmds");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            foreach (var file in Directory.EnumerateFiles(path, "*.dll"))
            {
                try
                {
                    if (loadedDLLs.Contains(file))
                        continue;
                    Assembly x = Assembly.LoadFile(file);
                    if (!x.FullName.Contains("PublicKeyToken"))
                    {
                        logger.Warn("Assembly {0} has no string name. Loading skipped....", x.FullName);
                        x = null;
                        continue;
                    }
                    logger.Info("Loading Commands from {1}", x.FullName);
                    RegisterCommandHandlers(x);
                    loadedDLLs.Add(file);
                    
                }
                catch (Exception ex)
                {
                    logger.Error("Error loading {0}: {1}", file, ex.ToString());
                }
            }
        }

        public static void RegisterCommandHandlers(Assembly x)
        {
            foreach (var t in x.GetExportedTypes())
            {
                if (t.GetInterfaces().Contains(typeof(ICommand)))
                {
                    if (t.IsAbstract)
                        continue;
                    logger.Info("Loading Command {0} from {1}", t.FullName, x.GetName().Name);
                    ICommand ex = Activator.CreateInstance(t) as ICommand;

                    allCommands[ex.cmd] = ex;
                    if (!Program.Config.Commands.ContainsCommand(ex.cmd))
                    {
                        Program.Config.Commands.Add(new Config.CommandConfiguration(ex));
                    }
                    else
                        Program.Config.Commands.UpdateCommand(ex);
                    Program.Config.Save();
                }
            }
        }
        
    }
}
