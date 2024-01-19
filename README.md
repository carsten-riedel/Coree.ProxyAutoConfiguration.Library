# Coree.ProxyAutoConfiguration.Library

**This library is currently a proof of concept and is not intended for immediate use.**

**At the moment, the project is primarily focused on configuration and setup, incorporating code from various proof of concepts.**
**Please note that significant changes and developments are expected as the project evolves.**

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


[Nuget testing](https://int.nugettest.org/)
```
dotnet nuget add source https://apiint.nugettest.org/v3/index.json -n nugettest.org
dotnet add package Coree.ProxyAutoConfiguration.Library --version 0.1.8782.10687-prerelease
dotnet tool install --global Coree.ProxyAutoConfiguration.PacEnv --version 0.1.8784.11408-prerelease
dotnet tool update --global Coree.ProxyAutoConfiguration.PacEnv --prerelease
```
