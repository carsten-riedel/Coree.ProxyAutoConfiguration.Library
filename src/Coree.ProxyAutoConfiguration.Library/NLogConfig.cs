using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Coree.ProxyAutoConfiguration.Library
{
    public class NLogConfig
    {
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable CA1823 // Unused field _

        private static readonly LogFactory _ = NLog.LogManager.Setup(SetupBuilder =>
        {
            SetupBuilder.LoadConfiguration(LoadConfigurationBuilder =>
            {
                LoadConfigurationBuilder.Configuration = new LoggingConfiguration();
                LoadConfigurationBuilder.Configuration.AddRuleForAllLevels(new ConsoleTarget() { Name = "Console" });
                LoadConfigurationBuilder.Configuration.AddRuleForAllLevels(new FileTarget()
                {
                    Name = "File",
                    ConcurrentWrites = true,
                    MaxArchiveFiles = 24,
                    FileName = "${processname}.log",
                    KeepFileOpen = true,
                    ArchiveFileName = "archive\\${processname}.{#}.log",
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveOldFileOnStartup = true,
                    Layout = new JsonLayout
                    {
                        Attributes =
                        {
                            new JsonAttribute("time", "${longdate}"),
                            new JsonAttribute("level", "${level}"),
                            new JsonAttribute("hostname", "${hostname}"),
                            new JsonAttribute("environment-user", "${environment-user}"),
                            new JsonAttribute("message", "${message}"),
                            new JsonAttribute("properties", new JsonLayout { IncludeEventProperties = true, MaxRecursionLimit = 2}, encode: false),
                            new JsonAttribute("exception", new JsonLayout
                            {
                                Attributes =
                                {
                                    new JsonAttribute("type", "${exception:format=type}"),
                                    new JsonAttribute("message", "${exception:format=message}"),
                                    new JsonAttribute("stacktrace", "${exception:format=tostring}"),
                                }
                            },
                            encode: false) // don't escape layout
                        }
                    }
                });
            });
        });

#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore CA1823 // Unused field _

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();



#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
        [ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
        public static void Initialize()
        {
            logger.Info("NLog ModuleInitializer Initialized");
        }
    }
}