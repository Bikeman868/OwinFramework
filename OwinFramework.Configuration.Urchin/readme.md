# Owin Framework Urchin Configuration

Add this package to your application and your Owin Framework components will use Urchin for their configuration.

No other code changes are required. Simply adding the package via NuGet will register the `IConfiguration` implementation with IoC.

Urchin is a centralized rules based configuration management system where components are configured using snippets of Json, and the 
application developer can decide on the overall structure of their configuration file. The source code and documentation for Urchin is 
here https://github.com/Bikeman868/Urchin.