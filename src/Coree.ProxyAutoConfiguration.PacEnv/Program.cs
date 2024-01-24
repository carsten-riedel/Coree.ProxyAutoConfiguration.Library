using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Coree.ProxyAutoConfiguration.Library;
using NLog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace Coree.ProxyAutoConfiguration.PacEnv
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandApp<PacEnvCommand>();

            app.Configure(config =>
            {
                // ... any other configuration you might have ...

                // Customize the help text

                config.SetHelpProvider(new CustomHelpProvider(config.Settings));
            });

            return app.Run(args);
        }



public class CustomHelpProvider : IHelpProvider
    {
        private readonly IHelpProvider _defaultHelpProvider;

        public CustomHelpProvider(ICommandAppSettings settings)
        {
            // Initialize the default help provider with the provided settings
            _defaultHelpProvider = new HelpProvider(settings);
        }

            IEnumerable<IRenderable> IHelpProvider.Write(ICommandModel model, ICommandInfo? command)
            {
                var defaultHelpText = _defaultHelpProvider.Write(model, command);
                var customHelpText = new List<IRenderable>();

                foreach (var renderable in defaultHelpText)
                {
                    if (renderable is Markup markup)
                    {
                        //markup.Text.Replace("Coree.ProxyAutoConfiguration.PacEnv.dll", "pacenv");
                        // Replace the specific string in the markup
                        customHelpText.Add(renderable);
                    }
                    else
                    {
                        // If it's not a Markup, add it as is
                        customHelpText.Add(renderable);
                    }
                }

                return customHelpText;
            }
        }


    internal sealed class PacEnvCommand : Command<PacEnvCommand.Settings>
        {
            public enum Scope
            {
                user,
                process,
            }

            public enum HostPrefix
            {
                none,
                http,
                https,
            }

            public sealed class Settings : CommandSettings
            {
                [Description("Specifies the path to the .pac file. Accepts local file paths and URLs (http, https, ftp). Requires direct network access. For Windows, leaving this empty defaults to registry settings. On other operating systems, this argument is mandatory. e.g. https://yourproxy.com/proxy.pac, C:\\config\\proxy.pac, Path\\proxy.pac ")]
                [CommandArgument(0, "[ProxyAutoConfigurationLocation]")]
                public string? PacLocation { get; init; }

                [CommandOption("-s|--scope")]
                [Description("Sets the scope for the proxy environment variables. 'user' applies the settings to the current user profile, while 'process' applies them only to the current process. Default is 'process'.")]
                [DefaultValue(Scope.process)]
                public Scope Scope { get; init; }

                [CommandOption("-u|--urlprefix")]
                [Description("Defines the URL prefix for the output of FindProxyForUrl. Acceptable values: 'none', 'http', 'https'. Defaults to 'http'. The format applied is '[[urlprefix]]://[[host]]:[[port]]'.")]
                [DefaultValue(HostPrefix.http)]
                public HostPrefix HostPrefix { get; init; }

                [Description("Specifies the URL for which to find the appropriate proxy settings. This option allows you to determine the proxy configuration for a specific web resource. By default, it is set to 'https://example.com'.")]
                [DefaultValue("https://example.com")]
                [CommandOption("-f|--find")]
                public string? FindProxyForURL { get; init; }

                [Description("Specifies the index of the proxy entry to be used from the FindProxyForURL PAC script return value. Default is -1, which corresponds to the first proxy entry.")]
                [DefaultValue(-1)]
                [CommandOption("-p|--proxyindex")]
                public int ProxyIndex { get; init; }
            }

            public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
            {
                PacResolver pacResolver = new PacResolver();
                pacResolver.ParseInputUri(settings.PacLocation);

                switch (pacResolver.Result.State)
                {
                    case PacResolver.PacResolveState.Ok:
                        AnsiConsole.Markup($"[green]PAC file location resolved successfully.[/] -> {pacResolver.Result.Uri}");
                        break;

                    case PacResolver.PacResolveState.InvalidUri:
                        AnsiConsole.Markup("[red]Error: The provided PAC location is either not a valid URI, the file does not exist, or it cannot be accessed. In cases of malformed URIs, 'https://' is automatically prefixed. Please verify the location, ensuring it is correctly written and accessible.[/]");
                        break;

                    case PacResolver.PacResolveState.UriStringParamIsNull:
                        AnsiConsole.Markup("[yellow]Note: No PAC location provided. Attempting to retrieve from Windows registry...[/]");
                        break;

                    case PacResolver.PacResolveState.RegistryAccessErrorOrEmpty:
                        AnsiConsole.Markup("[Gray]Notice: PAC (Proxy Auto-Configuration) entry not found in the Windows registry, and 'ProxyAutoConfigurationLocation' is unspecified. No modifications will be made to HTTP_PROXY and HTTPS_PROXY environment variables. Assuming a direct connection and proceeding without proxy configuration.[/]");
                        break;

                    case PacResolver.PacResolveState.WrongPlatformNeedNotNullPacLocation:
                        AnsiConsole.Markup("[red]Error: No PAC location provided and automatic retrieval is not supported on this platform. Please specify a PAC location.[/]");
                        break;

                    default:
                        AnsiConsole.Markup("[red]Error: An unexpected error occurred during PAC location resolution.[/]");
                        break;
                }
                AnsiConsole.WriteLine();

                if (pacResolver.Result.State != PacResolver.PacResolveState.Ok)
                {
                    return -1;
                }

                UniversalContentReader.ContentResult contentResult = UniversalContentReader.FetchContentFromUri(pacResolver.Result.Uri);
                if (contentResult.Exception != null)
                {
                    AnsiConsole.Markup($"[red]Failed to read PAC script.[/]");
                    AnsiConsole.WriteException(contentResult.Exception, ExceptionFormats.Default);
                    return -2;
                }
                else
                {
                    AnsiConsole.Markup($"[Green]Pac content fetched successfully.[/]");
                }

                var FullJavaScript = contentResult.Content + Environment.NewLine + JintInvoke.MozillaPacFunctions;

                var FindProxyForURLResult = JintInvoke.CallFindProxyForURL(FullJavaScript, settings.FindProxyForURL!);

                // Handling different states of the PAC script execution result
                switch (FindProxyForURLResult.State)
                {
                    case JintScriptRunner.ExecutionState.Success:
                        // Green color for success
                        AnsiConsole.Markup("[green]Proxy script configuration value:[/] -> " + FindProxyForURLResult.ReturnValue);
                        break;

                    case JintScriptRunner.ExecutionState.FunctionNonInvokable:
                        // Red color for script error
                        AnsiConsole.Markup("[red]Error: There was an error in the PAC script.[/]");
                        break;

                    case JintScriptRunner.ExecutionState.ScriptMissing:
                        // Red color for no script found
                        AnsiConsole.Markup("[red]Error: No PAC script was found.[/]");
                        break;

                    case JintScriptRunner.ExecutionState.FunctionNotFound:
                        // Red color for function not found
                        AnsiConsole.Markup("[red]Error: The FindProxyForURL function was not found in the PAC script.[/]");
                        break;

                    case JintScriptRunner.ExecutionState.ScriptExecutionError:
                        // Red color for execution error
                        AnsiConsole.Markup("[red]Error: There was an execution error: " + FindProxyForURLResult.Exception?.Message + "[/]");
                        break;

                    default:
                        // Red color for unknown errors
                        AnsiConsole.Markup("[red]Error: An unknown error occurred.[/]");
                        break;
                }

                if (FindProxyForURLResult.State != JintScriptRunner.ExecutionState.Success)
                {
                    return -3;
                }
                AnsiConsole.WriteLine();

                List<FindProxyForURLResultParser.ProxyConfiguration> entries = FindProxyForURLResultParser.ParseProxyStrings(FindProxyForURLResult.ReturnValue);

                if (entries.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]Error: No entries found in the PAC script return value.[/]");
                    return -4;
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].Type == FindProxyForURLResultParser.ProxyType.Unknown)
                    {
                        AnsiConsole.Markup($"[yellow]Entry #{i} is invalid: Type - {entries[i].Type}, Value - {entries[i].ReturnValue}[/]");
                        AnsiConsole.WriteLine();
                    }
                    else
                    {
                        AnsiConsole.Markup($"[green]Entry #{i} is valid: Type - {entries[i].Type}, Value - {entries[i].ReturnValue}[/]");
                        AnsiConsole.WriteLine();
                    }
                }

                FindProxyForURLResultParser.ProxyConfiguration SelectedEntry;

                // Check if the default proxy entry should be used.
                if (settings.ProxyIndex == -1)
                {
                    SelectedEntry = entries.First();
                    AnsiConsole.MarkupLine($"[green]Default proxy selected: {SelectedEntry.ReturnValue}[/]");
                }
                else
                {
                    // Attempt to select a proxy entry based on the provided index.
                    try
                    {
                        SelectedEntry = entries[settings.ProxyIndex];
                        AnsiConsole.MarkupLine($"[green]Selected proxy entry: {SelectedEntry.ReturnValue}[/]");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        AnsiConsole.MarkupLine($"[red]Error: The specified proxy index {settings.ProxyIndex} is out of bounds. It must be between 0 and {entries.Count - 1}.[/]");
                        return -5;
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Unexpected error: {ex.Message}[/]");
                        return -5;
                    }
                }

                switch (settings.HostPrefix)
                {
                    case HostPrefix.none:
                        AnsiConsole.MarkupLine("[gray]Notice: No prefix added. Proxy URL: [/][blue]{SelectedEntry.ProxyUrl}[/]");
                        break;

                    case HostPrefix.http:
                        SelectedEntry.ReturnValue = "http://" + SelectedEntry.ReturnValue;
                        AnsiConsole.MarkupLine($"[gray]Notice: 'http://' prefix added. Proxy URL: [/][blue]{SelectedEntry.ReturnValue}[/]");
                        break;

                    case HostPrefix.https:
                        SelectedEntry.ReturnValue = "https://" + SelectedEntry.ReturnValue;
                        AnsiConsole.MarkupLine($"[gray]Notice: 'https://' prefix added. Proxy URL: [/][blue]{SelectedEntry.ReturnValue}[/]");
                        break;

                    default:
                        break;
                }

                switch (settings.Scope)
                {
                    case Scope.user:
                        System.Environment.SetEnvironmentVariable("HTTP_PROXY", SelectedEntry.ReturnValue, EnvironmentVariableTarget.User);
                        System.Environment.SetEnvironmentVariable("HTTPS_PROXY", SelectedEntry.ReturnValue, EnvironmentVariableTarget.User);
                        AnsiConsole.MarkupLine($"[green]Proxy settings applied to User scope. HTTP_PROXY and HTTPS_PROXY set to: [/][blue]{SelectedEntry.ReturnValue}[/]");
                        break;

                    case Scope.process:
                        System.Environment.SetEnvironmentVariable("HTTP_PROXY", SelectedEntry.ReturnValue, EnvironmentVariableTarget.Process);
                        System.Environment.SetEnvironmentVariable("HTTPS_PROXY", SelectedEntry.ReturnValue, EnvironmentVariableTarget.Process);
                        AnsiConsole.MarkupLine($"[green]Proxy settings applied to Process scope. HTTP_PROXY and HTTPS_PROXY set to: [/][blue]{SelectedEntry.ReturnValue}[/]");
                        break;
                }

                return 0;
            }

            public void Dump()
            {
                var vars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process)
            .Cast<DictionaryEntry>()
            .Select(entry => new KeyValuePair<string, string>(
                entry.Key?.ToString() ?? string.Empty,
                entry.Value?.ToString() ?? string.Empty))
            .ToList();

                //var vars = System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process).Cast<DictionaryEntry>().Cast<KeyValuePair<string, string>>();

                // Convert IDictionary to a List of KeyValuePairs and sort it by key
                var sortedVars = vars.OrderBy(entry => entry.Key).ToList();

                foreach (var var in sortedVars)
                {
                    string keyText = $"Key: {var.Key}".PadRight(20); // Adjust the padding as needed
                    string valueText = $"Value: {var.Value}".PadLeft(20); // Adjust the padding as needed

                    AnsiConsole.MarkupLine($"[blue]{keyText}[/], [green]{valueText}[/]");
                }
            }
        }
    }
}

//var proxyAutoConfiguration = new Library.ProxyAutoConfiguration();

/*
               [CommandOption("-p|--pattern")]
               public string? SearchPattern { get; init; }

               [CommandOption("-x|--hidden")]
               [DefaultValue(true)]
               public bool IncludeHidden { get; init; }
               */