# Coree.ProxyAutoConfiguration.Library

**The project is under configuration and setup only atm, there is some code copy from proof of concepts.**

This library simplifies proxy configuration in .NET applications by automatically determining proxy settings. It supports two methods:

- **PAC URL Analysis**: Parses proxy settings from a specified PAC (Proxy Auto-Configuration) URL.
- **Windows Internet Options**: Integrates with and uses Windows Internet Options for proxy settings.

After determining the proxy, the library sets `HTTP_PROXY` and `HTTPS_PROXY` environment variables, enabling seamless proxy usage in projects that don't manage these settings natively. Ideal for ensuring consistent proxy configurations across various environments in .NET applications.

## Report placeholder

[LicenseReport solution](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/LicenseReport/license_sln.md)

[LicenseReport project](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/LicenseReport/license_prj.md)

[Nuget PackageList](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/NugetReport/PackageList.txt)

[Nuget PackageVulnerable](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/NugetReport/PackageVulnerable.txt)

[Code coverage report](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/ReportGeneratorOutput/SummaryGithub.md)

[Test result](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/MSTestResults/result.html)
Copy and link to docs folder

https://int.nugettest.org/
```
dotnet add package Coree.ProxyAutoConfiguration.Library --version 0.1.8782.10687-prerelease
```
