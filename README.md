# Coree.ProxyAutoConfiguration.Library

**This library is currently a proof of concept and is not intended for immediate use.**

**At the moment, the project is primarily focused on configuration and setup, incorporating code from various proof of concepts.**
**Please note that significant changes and developments are expected as the project evolves.**

This library simplifies proxy configuration in .NET applications by automatically determining proxy settings. It supports two methods:

- **PAC URL Analysis**: Parses proxy settings from a specified PAC (Proxy Auto-Configuration) URL.
- **Windows Internet Options**: Integrates with and uses Windows Internet Options for proxy settings.

Once the proxy settings are determined, the library sets the `HTTP_PROXY` and `HTTPS_PROXY` environment variables exclusively for the current process. This approach ensures that there are no changes to the user or system-wide environment settings. This functionality enables seamless proxy integration in projects that do not automatically handle these settings, providing a consistent proxy configuration within the scope of the .NET application's process.

## Report placeholder

[LicenseReport solution](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/LicenseReport/license_sln.md)

[LicenseReport project](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/LicenseReport/license_prj.md)

[Nuget PackageList](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/NugetReport/PackageList.txt)

[Nuget PackageVulnerable](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/NugetReport/PackageVulnerable.txt)

[Code coverage report](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/ReportGeneratorOutput/SummaryGithub.md)

[Test result](https://github.com/carsten-riedel/Coree.ProxyAutoConfiguration.Library/blob/main/src/Coree.ProxyAutoConfiguration.Library.MSTest/MSTestResults/result.html)
Copy and link to docs folder


[Nuget testing](https://int.nugettest.org/)
```
REM Adding nuget test
dotnet nuget add source https://apiint.nugettest.org/v3/index.json -n nugettest.org
REM Add as library
dotnet add package Coree.ProxyAutoConfiguration.Library --version 0.1.8782.10687-prerelease
REM Add as dotnet global tool
dotnet tool install --global Coree.ProxyAutoConfiguration.PacEnv --version 0.1.8784.11408-prerelease
REM dotnet global tool update to latest version
dotnet tool update --global Coree.ProxyAutoConfiguration.PacEnv --prerelease
REM call the dotnet tool command
pacenv
```
