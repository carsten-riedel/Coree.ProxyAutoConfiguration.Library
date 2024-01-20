using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Coree.ProxyAutoConfiguration.Library;

namespace Coree.ProxyAutoConfiguration.Library
{

    public class PacResolver
    {
        public string? PacLocation { get; private set; }
        public string? UsedPacLocation { get; private set; }
        public PacResolutionResult Result { get; private set; }

        public PacResolver()
        {
            this.Result = new PacResolutionResult { State = PacResolveState.Unresolved };
        }

        public void ParseInputUri(string? pacLocation)
        {
            this.PacLocation = pacLocation;
            this.Result = ResolvePacLocationAsUri();
        }

        public enum PacResolveState
        {
            Ok,
            InvalidUri,
            UriStringParamIsNull,
            RegistryAccessErrorOrEmpty,
            WrongPlatformNeedNotNullPacLocation,
            Unresolved
        }

        private string? GetPacUrlFromRegistry()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
                using (RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(registryKeyPath))
                {
                    if (registryKey != null)
                    {
                        object? autoConfigUrlValue = registryKey.GetValue("AutoConfigURL");
                        if (autoConfigUrlValue != null)
                        {
                            return autoConfigUrlValue.ToString();
                        }
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public class PacResolutionResult
        {
            public Uri? Uri { get; set; }
            public PacResolveState State { get; set; }
            public Exception? Exception { get; set; }
        }

        private PacResolutionResult ResolvePacLocationAsUri()
        {
            UriResolver uriResolver = new UriResolver();
            uriResolver.ParseInputUri(PacLocation);

            if (uriResolver.Result.State == UriResolver.ResolveState.UriStringParamIsNull)
            {
                string? registryUri = GetPacUrlFromRegistry();
                if (registryUri == null)
                {
                    return new PacResolutionResult { State = PacResolveState.RegistryAccessErrorOrEmpty };
                }
                uriResolver.ParseInputUri(registryUri);
                this.UsedPacLocation = registryUri; // Set the used PAC location from the registry
            }
            else
            {
                this.UsedPacLocation = PacLocation; // Set the used PAC location as the provided one
            }

            return new PacResolutionResult
            {
                Uri = uriResolver.Result.Uri,
                State = ConvertToPacResolveState(uriResolver.Result.State),
                Exception = uriResolver.Result.Exception
            };
        }
        private PacResolveState ConvertToPacResolveState(UriResolver.ResolveState state)
        {
            switch (state)
            {
                case UriResolver.ResolveState.Ok:
                    return PacResolveState.Ok;
                case UriResolver.ResolveState.InvalidUri:
                    return PacResolveState.InvalidUri;
                case UriResolver.ResolveState.UriStringParamIsNull:
                    return PacResolveState.UriStringParamIsNull;
                default:
                    return PacResolveState.Unresolved;
            }
        }
    }


    public class UriResolver
    {
        public string? InputUri { get; private set; }
        public UriResolutionResult Result { get; private set; }

        public UriResolver()
        {
            Result = new UriResolutionResult { State = ResolveState.UriStringParamIsNull };
        }

        public enum ResolveState
        {
            Ok,
            InvalidUri,
            UriStringParamIsNull,
        }

        public class UriResolutionResult
        {
            public Uri? Uri { get; set; }
            public ResolveState State { get; set; }
            public Exception? Exception { get; set; }
        }

        public void ParseInputUri(string? inputUri)
        {
            this.InputUri = inputUri;
            this.Result = Resolve();
        }

        private UriResolutionResult Resolve()
        {
            if (InputUri == null)
            {
                return new UriResolutionResult { State = ResolveState.UriStringParamIsNull };
            }
            else
            {
                try
                {
                    Uri resolvedUri;
                    if (Uri.IsWellFormedUriString(InputUri, UriKind.Absolute))
                    {
                        resolvedUri = new Uri(InputUri);
                    }
                    else
                    {
                        var absolutePath = Path.GetFullPath(InputUri);
                        resolvedUri = new Uri(absolutePath);
                    }

                    return new UriResolutionResult { State = ResolveState.Ok, Uri = resolvedUri };
                }
                catch (Exception ex)
                {
                    return new UriResolutionResult { State = ResolveState.InvalidUri, Exception = ex };
                }
            }
        }
    }



}