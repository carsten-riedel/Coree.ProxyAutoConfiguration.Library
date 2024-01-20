using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Coree.ProxyAutoConfiguration.Library;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Coree.ProxyAutoConfiguration.PacEnv
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandApp<PacEnvCommand>();
            return app.Run(args);
        }

        internal sealed class PacEnvCommand : Command<PacEnvCommand.Settings>
        {
            public sealed class Settings : CommandSettings
            {
                [Description("Path to search. Defaults to current directory.")]
                [CommandArgument(0, "[searchPath]")]
                public string? SearchPath { get; init; }

                [CommandOption("-p|--pattern")]
                public string? SearchPattern { get; init; }

                [CommandOption("-h|--hidden")]
                [DefaultValue(true)]
                public bool IncludeHidden { get; init; }
            }

            public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
            {
                var proxyAutoConfiguration = new Library.ProxyAutoConfiguration();
                
                return 0;
            }
        }
    }
}