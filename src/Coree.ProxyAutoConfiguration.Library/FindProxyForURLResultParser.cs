using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coree.ProxyAutoConfiguration.Library
{
    public class FindProxyForURLResultParser
    {
        public class ProxyConfiguration
        {
            public string? ReturnValue { get; set; }
            public ProxyType Type { get; set; }
        }

        public enum ProxyType
        {
            Direct,
            Proxy,
            Socks,
            Http,
            Https,
            Socks4,
            Socks5,
            Unknown
        }

        private static ProxyConfiguration CreateProxyConfiguration(string entry, ProxyType type)
        {
            return new ProxyConfiguration
            {
                ReturnValue = entry.StartsWith("DIRECT", StringComparison.OrdinalIgnoreCase) ? "DIRECT" : entry.Substring(entry.IndexOf(' ') + 1).Trim(),
                Type = type
            };
        }

        public static List<ProxyConfiguration> ParseProxyStrings(string? pacResult)
        {
            var results = new List<ProxyConfiguration>();

            if (pacResult == null)
            {
                return results;
            }

            string[] entries = pacResult.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
                var trimmedEntry = entry.Trim();
                if (trimmedEntry.StartsWith("PROXY", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateProxyConfiguration(trimmedEntry, ProxyType.Proxy));
                }
                else if (trimmedEntry.StartsWith("SOCKS4", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateProxyConfiguration(trimmedEntry, ProxyType.Socks4));
                }
                else if (trimmedEntry.StartsWith("SOCKS5", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateProxyConfiguration(trimmedEntry, ProxyType.Socks5));
                }
                else if (trimmedEntry.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateProxyConfiguration(trimmedEntry, ProxyType.Http));
                }
                else if (trimmedEntry.StartsWith("HTTPS", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateProxyConfiguration(trimmedEntry, ProxyType.Https));
                }
                else if (trimmedEntry.Equals("DIRECT", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateProxyConfiguration(trimmedEntry, ProxyType.Direct));
                }
                else
                {
                    results.Add(new ProxyConfiguration { ReturnValue = trimmedEntry, Type = ProxyType.Unknown });
                }
            }

            return results;
        }
    }
}