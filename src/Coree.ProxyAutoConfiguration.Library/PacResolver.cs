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
            GenericUriConverter uriResolver = new GenericUriConverter();
            uriResolver.ConvertInputToUri(PacLocation);

            if (uriResolver.ConversionResult.State == GenericUriConverter.ConversionState.Uninitialized)
            {
                string? registryUri = GetPacUrlFromRegistry();
                if (registryUri == null)
                {
                    return new PacResolutionResult { State = PacResolveState.RegistryAccessErrorOrEmpty };
                }
                uriResolver.ConvertInputToUri(registryUri);
                this.UsedPacLocation = registryUri; // Set the used PAC location from the registry
            }
            else
            {
                this.UsedPacLocation = PacLocation; // Set the used PAC location as the provided one
            }

            return new PacResolutionResult
            {
                Uri = uriResolver.ConversionResult.Uri,
                State = ConvertToPacResolveState(uriResolver.ConversionResult.State),
                Exception = uriResolver.ConversionResult.Exception
            };
        }

        private PacResolveState ConvertToPacResolveState(GenericUriConverter.ConversionState state)
        {
            switch (state)
            {
                case GenericUriConverter.ConversionState.ValidUri:
                    return PacResolveState.Ok;

                case GenericUriConverter.ConversionState.ConvertException:
                    return PacResolveState.InvalidUri;

                case GenericUriConverter.ConversionState.Uninitialized:
                    return PacResolveState.UriStringParamIsNull;

                default:
                    return PacResolveState.Unresolved;
            }
        }
    }
}