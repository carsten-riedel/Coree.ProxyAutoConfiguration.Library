using Jint.Native;
using Jint.Runtime;
using Jint;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Coree.ProxyAutoConfiguration.Library
{
    public class ProxyAutoConfiguration
    {
        //See https://developer.mozilla.org/en-US/docs/Web/HTTP/Proxy_servers_and_tunneling/Proxy_Auto-Configuration_PAC_file
        //See https://github.com/mozilla/gecko-dev/blob/ae3df68e9ba2faeaf76445a7650e4e742eb7b4e7/netwerk/base/ascii_pac_utils.js
        private const string MozillaPacFunctions = @"
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

/* global dnsResolve */

function dnsDomainIs(host, domain) {
  return (
    host.length >= domain.length &&
    host.substring(host.length - domain.length) == domain
  );
}

function dnsDomainLevels(host) {
  return host.split(""."").length - 1;
}

function isValidIpAddress(ipchars) {
  var matches = /^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$/.exec(ipchars);
  if (matches == null) {
    return false;
  } else if (
    matches[1] > 255 ||
    matches[2] > 255 ||
    matches[3] > 255 ||
    matches[4] > 255
  ) {
    return false;
  }
  return true;
}

function convert_addr(ipchars) {
  var bytes = ipchars.split(""."");
  var result =
    ((bytes[0] & 0xff) << 24) |
    ((bytes[1] & 0xff) << 16) |
    ((bytes[2] & 0xff) << 8) |
    (bytes[3] & 0xff);
  return result;
}

function isInNet(ipaddr, pattern, maskstr) {
  if (!isValidIpAddress(pattern) || !isValidIpAddress(maskstr)) {
    return false;
  }
  if (!isValidIpAddress(ipaddr)) {
    ipaddr = dnsResolve(ipaddr);
    if (ipaddr == null) {
      return false;
    }
  }
  var host = convert_addr(ipaddr);
  var pat = convert_addr(pattern);
  var mask = convert_addr(maskstr);
  return (host & mask) == (pat & mask);
}

function isPlainHostName(host) {
  return host.search(""(\\.)|:"") == -1;
}

function isResolvable(host) {
  var ip = dnsResolve(host);
  return ip != null;
}

function localHostOrDomainIs(host, hostdom) {
  return host == hostdom || hostdom.lastIndexOf(host + ""."", 0) == 0;
}

function shExpMatch(url, pattern) {
  pattern = pattern.replace(/\./g, ""\\."");
  pattern = pattern.replace(/\*/g, "".*"");
  pattern = pattern.replace(/\?/g, ""."");
  var newRe = new RegExp(""^"" + pattern + ""$"");
  return newRe.test(url);
}

var wdays = { SUN: 0, MON: 1, TUE: 2, WED: 3, THU: 4, FRI: 5, SAT: 6 };
var months = {
  JAN: 0,
  FEB: 1,
  MAR: 2,
  APR: 3,
  MAY: 4,
  JUN: 5,
  JUL: 6,
  AUG: 7,
  SEP: 8,
  OCT: 9,
  NOV: 10,
  DEC: 11,
};

function weekdayRange() {
  function getDay(weekday) {
    if (weekday in wdays) {
      return wdays[weekday];
    }
    return -1;
  }
  var date = new Date();
  var argc = arguments.length;
  var wday;
  if (argc < 1) {
    return false;
  }
  if (arguments[argc - 1] == ""GMT"") {
    argc--;
    wday = date.getUTCDay();
  } else {
    wday = date.getDay();
  }
  var wd1 = getDay(arguments[0]);
  var wd2 = argc == 2 ? getDay(arguments[1]) : wd1;
  if (wd1 == -1 || wd2 == -1) {
    return false;
  }

  if (wd1 <= wd2) {
    return wd1 <= wday && wday <= wd2;
  }

  return wd2 >= wday || wday >= wd1;
}

function dateRange() {
  function getMonth(name) {
    if (name in months) {
      return months[name];
    }
    return -1;
  }
  var date = new Date();
  var argc = arguments.length;
  if (argc < 1) {
    return false;
  }
  var isGMT = arguments[argc - 1] == ""GMT"";

  if (isGMT) {
    argc--;
  }
  // function will work even without explict handling of this case
  if (argc == 1) {
    let tmp = parseInt(arguments[0]);
    if (isNaN(tmp)) {
      return (
        (isGMT ? date.getUTCMonth() : date.getMonth()) == getMonth(arguments[0])
      );
    } else if (tmp < 32) {
      return (isGMT ? date.getUTCDate() : date.getDate()) == tmp;
    }
    return (isGMT ? date.getUTCFullYear() : date.getFullYear()) == tmp;
  }
  var year = date.getFullYear();
  var date1, date2;
  date1 = new Date(year, 0, 1, 0, 0, 0);
  date2 = new Date(year, 11, 31, 23, 59, 59);
  var adjustMonth = false;
  for (let i = 0; i < argc >> 1; i++) {
    let tmp = parseInt(arguments[i]);
    if (isNaN(tmp)) {
      let mon = getMonth(arguments[i]);
      date1.setMonth(mon);
    } else if (tmp < 32) {
      adjustMonth = argc <= 2;
      date1.setDate(tmp);
    } else {
      date1.setFullYear(tmp);
    }
  }
  for (let i = argc >> 1; i < argc; i++) {
    let tmp = parseInt(arguments[i]);
    if (isNaN(tmp)) {
      let mon = getMonth(arguments[i]);
      date2.setMonth(mon);
    } else if (tmp < 32) {
      date2.setDate(tmp);
    } else {
      date2.setFullYear(tmp);
    }
  }
  if (adjustMonth) {
    date1.setMonth(date.getMonth());
    date2.setMonth(date.getMonth());
  }
  if (isGMT) {
    let tmp = date;
    tmp.setFullYear(date.getUTCFullYear());
    tmp.setMonth(date.getUTCMonth());
    tmp.setDate(date.getUTCDate());
    tmp.setHours(date.getUTCHours());
    tmp.setMinutes(date.getUTCMinutes());
    tmp.setSeconds(date.getUTCSeconds());
    date = tmp;
  }
  return date1 <= date2
    ? date1 <= date && date <= date2
    : date2 >= date || date >= date1;
}

function timeRange() {
  var argc = arguments.length;
  var date = new Date();
  var isGMT = false;
  if (argc < 1) {
    return false;
  }
  if (arguments[argc - 1] == ""GMT"") {
    isGMT = true;
    argc--;
  }

  var hour = isGMT ? date.getUTCHours() : date.getHours();
  var date1, date2;
  date1 = new Date();
  date2 = new Date();

  if (argc == 1) {
    return hour == arguments[0];
  } else if (argc == 2) {
    return arguments[0] <= hour && hour <= arguments[1];
  }
  switch (argc) {
    case 6:
      date1.setSeconds(arguments[2]);
      date2.setSeconds(arguments[5]);
    // falls through
    case 4:
      var middle = argc >> 1;
      date1.setHours(arguments[0]);
      date1.setMinutes(arguments[1]);
      date2.setHours(arguments[middle]);
      date2.setMinutes(arguments[middle + 1]);
      if (middle == 2) {
        date2.setSeconds(59);
      }
      break;

    default:
      throw new Error(""timeRange: bad number of arguments"");
  }

  if (isGMT) {
    date.setFullYear(date.getUTCFullYear());
    date.setMonth(date.getUTCMonth());
    date.setDate(date.getUTCDate());
    date.setHours(date.getUTCHours());
    date.setMinutes(date.getUTCMinutes());
    date.setSeconds(date.getUTCSeconds());
  }
  return date1 <= date2
    ? date1 <= date && date <= date2
    : date2 >= date || date >= date1;
}
";


        private const string SuggestedPacFunctions = @"
function isPlainHostName(host) {
    // Check if there are no dots in the hostname
    return (host.indexOf('.') === -1);
}

function shExpMatch(url, pattern) {
    pattern = pattern.replace(/\./g, '\\.').replace(/\*/g, '.*').replace(/\?/g, '.');
    var regex = new RegExp('^' + pattern + '$');
    return regex.test(url);
}

function isInNet(host, pattern, mask) {
    // Convert a dot-decimal IP address to a 32-bit integer
    function ipToLong(ip) {
        var components = ip.split(""."");
        var ipLong = 0;
        for (var i = 0; i < components.length; i++) {
            ipLong |= parseInt(components[i]) << (8 * (3 - i));
        }
        return ipLong >>> 0; // Unsigned right shift to ensure a positive number
    }

    // Convert host to IP if it's not already an IP
    var ip = host; // Assuming host is already an IP address
    // If you have a way to resolve hostnames to IPs, you should do it here

    var longIp = ipToLong(ip);
    var longPattern = ipToLong(pattern);
    var longMask = ipToLong(mask);

    return (longIp & longMask) === (longPattern & longMask);
}
";
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

        private string? ReadPacFileSync(string? url)
        {
            if (url != null)
            {
                using (HttpClient client = new())
                {
                    try
                    {
                        var response = client.GetAsync(url).Result;
                        response.EnsureSuccessStatusCode();
                        return response.Content.ReadAsStringAsync().Result;
                    }
                    catch
                    {
                        // In case of any failure, return null
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }

        private string? JintInvokeScriptFunctionFindProxyForURL(string? pacScript, string urlToTest)
        {

            if (pacScript == null)
            {
                return null;
            }
            Engine engine = new Engine();
            engine.Execute(pacScript);

            var findProxyForURL = engine.GetValue("FindProxyForURL");

            if (findProxyForURL.Type == Types.Object)
            {
                Uri uri = new Uri(urlToTest);
                string host = uri.Host;

                JsValue result = engine.Invoke(findProxyForURL, new JsValue[] { new JsValue(urlToTest), new JsValue(host) });

                if (result.Type == Types.String)
                {
                    return result.ToString();
                }
            }

            return null;
        }

        // Method to parse the PAC result and return a list of proxy URLs
        private List<string> ParseProxyStrings(string? pacResult)
        {
            if (pacResult == null)
            {
                return new List<string>();
            }

            var proxies = new List<string>();
            string[] entries = pacResult.Split(';');

            foreach (var entry in entries)
            {
                var trimmedEntry = entry.Trim();
                if (trimmedEntry.StartsWith("PROXY"))
                {
                    proxies.Add(trimmedEntry.Substring(6).Trim()); // Extract the proxy URL
                }
                else if (trimmedEntry.Equals("DIRECT", StringComparison.OrdinalIgnoreCase))
                {
                    proxies.Add("DIRECT");
                }
            }

            return proxies;
        }

        public string? AutoConfigURL { get; private set; }
        public string? PacFileContent { get; private set; }
        public string? FindProxyForURLResult { get; private set; }
        public string? FullJavaScript { get; private set; }

        public ProxyAutoConfiguration()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException();
            }
            AutoConfigURL = GetPacUrlFromRegistry();
            PacFileContent = ReadPacFileSync(AutoConfigURL);

            FullJavaScript = PacFileContent + Environment.NewLine + MozillaPacFunctions;

            FindProxyForURLResult = JintInvokeScriptFunctionFindProxyForURL(FullJavaScript, "https://www.example.com");

            List<string> proxyOrder = ParseProxyStrings(FindProxyForURLResult);

            foreach (var item in proxyOrder)
            {
                Debug.Print($@"{item}");
            }

            if (proxyOrder.Any())
            {
                Environment.SetEnvironmentVariable("HTTP_PROXY", proxyOrder.First(), EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("HTTPS_PROXY", proxyOrder.First(), EnvironmentVariableTarget.Process);
            }
        }

        public string? Resolve(string url)
        {

            FindProxyForURLResult = JintInvokeScriptFunctionFindProxyForURL(FullJavaScript, url);

            List<string> proxyOrder = ParseProxyStrings(FindProxyForURLResult);

            if (proxyOrder.Any())
            {
                return proxyOrder.First();
            }
            else
            {
                return null;
            }
        }

        public void Set(string url)
        {
            var firstprox = Resolve(url);
            if (firstprox != null)
            {
                Environment.SetEnvironmentVariable("HTTP_PROXY", firstprox, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("HTTPS_PROXY", firstprox, EnvironmentVariableTarget.Process);
            }
        }
    }
}