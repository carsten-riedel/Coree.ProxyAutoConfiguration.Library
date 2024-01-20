using System;
using System.Collections.Generic;
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
                
                [Description("Specifies the path to the .pac file. Accepts local file paths and URLs (http, https, ftp). Requires direct network access. For Windows, leaving this empty defaults to registry settings. On other operating systems, this argument is mandatory.")]
                [CommandArgument(0, "[ProxyAutoConfigurationLocation]")]
                public string? PacLocation { get; init; }

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
                var pacResolver = new Library.PacResolver();
                pacResolver.ParseInputUri(settings.PacLocation);

                switch (pacResolver.Result.State)
                {
                    case PacResolver.PacResolveState.Ok:
                        AnsiConsole.Markup($"[green]PAC file location resolved successfully.[/] -> {pacResolver.Result.Uri!.ToString()}");
                        break;

                    case PacResolver.PacResolveState.InvalidUri:
                        AnsiConsole.Markup("[red]Error: The provided PAC location is not a valid URI or local file path.[/]");
                        return -1;

                    case PacResolver.PacResolveState.UriStringParamIsNull:
                        AnsiConsole.Markup("[yellow]Note: No PAC location provided. Attempting to retrieve from Windows registry...[/]");
                        break;

                    case PacResolver.PacResolveState.RegistryAccessErrorOrEmpty:
                        AnsiConsole.Markup("[red]Error: Unable to retrieve PAC location from the Windows registry, or the registry entry is empty.[/]");
                        return -1;

                    case PacResolver.PacResolveState.WrongPlatformNeedNotNullPacLocation:
                        AnsiConsole.Markup("[red]Error: No PAC location provided and automatic retrieval is not supported on this platform. Please specify a PAC location.[/]");
                        return -1;

                    default:
                        AnsiConsole.Markup("[red]Error: An unexpected error occurred during PAC location resolution.[/]") ;
                        return -1;
                }
                AnsiConsole.WriteLine();


                UriSimpleReader uriSimpleReader = new UriSimpleReader();
                var pacScriptContent = uriSimpleReader.GetContent(pacResolver.Result.Uri);

                /*
                if (pacScriptContent.Content == null)
                {
                    string errorMessage = pacScriptContent.exception != null ? pacScriptContent.exception.Message : "Unknown error";
                    AnsiConsole.Markup($"[red]Failed to read PAC script: {errorMessage}.[/]");
                    return -1;
                }
                */


                // Assuming FullJavaScript is a combination of PAC script and necessary functions
                var FullJavaScript = pacScriptContent.Content + Environment.NewLine + JintInvoke.MozillaPacFunctions;

                // Overriding FullJavaScript with a test PAC script for testing purposes
                //FullJavaScript = JintInvoke.StubTest + Environment.NewLine + JintInvoke.MozillaPacFunctions;

                // Executing the PAC script to find the proxy configuration for a given URL
                var FindProxyForURLResult = JintInvoke.CallFindProxyForURL(FullJavaScript, settings.FindProxyForURL!);

                // Handling different states of the PAC script execution result
                switch (FindProxyForURLResult.State)
                {
                    case JintInvokeScriptFunction.JintInvokeState.Ok:
                        // Green color for success
                        AnsiConsole.Markup("[green]Proxy script configuration value:[/] -> " + FindProxyForURLResult.ReturnValue);
                        break;
                    case JintInvokeScriptFunction.JintInvokeState.ScriptError:
                        // Red color for script error
                        AnsiConsole.Markup("[red]Error: There was an error in the PAC script.[/]");
                        return -1;
                    case JintInvokeScriptFunction.JintInvokeState.NoScript:
                        // Red color for no script found
                        AnsiConsole.Markup("[red]Error: No PAC script was found.[/]");
                        return -1;
                    case JintInvokeScriptFunction.JintInvokeState.FunctionNotFound:
                        // Red color for function not found
                        AnsiConsole.Markup("[red]Error: The FindProxyForURL function was not found in the PAC script.[/]");
                        return -1;
                    case JintInvokeScriptFunction.JintInvokeState.ExecutionError:
                        // Red color for execution error
                        AnsiConsole.Markup("[red]Error: There was an execution error: " + FindProxyForURLResult.Exception?.Message + "[/]");
                        return -1;
                    default:
                        // Red color for unknown errors
                        AnsiConsole.Markup("[red]Error: An unknown error occurred.[/]");
                        return -1;
                }
                AnsiConsole.WriteLine();

                List<PacResultParser.ProxyConfiguration> entries = PacResultParser.ParseProxyStrings(FindProxyForURLResult.ReturnValue);

                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].Type == PacResultParser.ProxyType.Unknown)
                    {
                        AnsiConsole.Markup($"[yellow]Entry #{i} is invalid: Type - {entries[i].Type}, Value - {entries[i].ReturnValue}[/]");
                        AnsiConsole.WriteLine();
                    }
                    else
                    {
                        // Replace 'ok blah blah' with a more descriptive message if needed
                        AnsiConsole.Markup($"[green]Entry #{i} is valid: Type - {entries[i].Type}, Value - {entries[i].ReturnValue}[/]");
                        AnsiConsole.WriteLine();
                    }
                }

                if (!entries.Any(e => e.Type == PacResultParser.ProxyType.Proxy))
                {
                    AnsiConsole.MarkupLine("[yellow]Warning: No proxy entries found in the PAC script return value.[/]");
                    return -1;
                }


                PacResultParser.ProxyConfiguration FirstProxy = entries.First(e => e.Type == PacResultParser.ProxyType.Proxy);

                FirstProxy.ReturnValue = "http://" + FirstProxy.ReturnValue;

                AnsiConsole.MarkupLine($"[green]using {FirstProxy.ReturnValue} [/]");
                // Process the PAC script content if needed
                // ...

                System.Environment.SetEnvironmentVariable("HTTP_PROXY", FirstProxy.ReturnValue, EnvironmentVariableTarget.Process);
                System.Environment.SetEnvironmentVariable("HTTPS_PROXY", FirstProxy.ReturnValue, EnvironmentVariableTarget.Process);

                System.Collections.IDictionary vars =  System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);

                // Convert IDictionary to a List of KeyValuePairs and sort it by key
                var sortedVars = vars.Cast<System.Collections.DictionaryEntry>()
                                     .Select(entry => new KeyValuePair<string, string>(entry.Key.ToString(), entry.Value.ToString()))
                                     .OrderBy(entry => entry.Key)
                                     .ToList();

                foreach (var var in sortedVars)
                {
                    AnsiConsole.MarkupLine($"[blue]Key:[/] {var.Key}, [green]Value:[/] {var.Value}");
                }



                return 0;
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