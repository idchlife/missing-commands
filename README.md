# Missing Commands

This library is created for using cli commands inside ASP.NET applications.

## Why and what would I need this

You know how those fancy frameworks from foreign places (python: Django, Flask, php: Symfony) have cli commands?

You may say: - why yes of course we do have them here too, just create CommandLineApplication project and use it.

But what if you need to share codebase? Well, of course you can create some kind of shared library, maybe have 2 projects near and reference them via .csproj files.

Or maybe connect to messaging MQ server and send command via it to your hosted background service?

But hey, that's can be just too hard or take too much time or make you use additional dependencies, track of them, etc

I introduce to you: **MissingCommands**

With this library you can create custom commands for your apps (usually web based apps: MVC, WebApi, Blazor) which you would be able to use like this:

```bash
dotnet run cli emails:send-batch

dotnet run cli analytics:create-files

dotnet run cli push:notify-all-clients-new-version

dotnet run cli database:seed-fake-reviews
```

Your server running, and in the same folder you can just activate your commands!

## Requirements

You need to use ASP.NET project (MVC, WebApi, Blazor) with default service
container (extension methods are used for it), Program.cs and Startup.cs files

## Installation
You can install it via nuget.org package registry.

Like

```bash
dotnet add package MissingCommands
```

## Usage
After installation you should use MissingCommands in your Program.cs file.

[new in 0.1.1] You can also configure here MissingCommands so you would be able to check in Startup.cs file whether application is in CLI mode (useful if you don't want to start some services or in cli mode you want to use different services).

```csharp
// Program.cs
// All other imports
// Add this:
using MissingCommands;

namespace MyApp {
  public class Program {
    public static void Main(string[] args) {
      // Instead of this in Main method:
      // CreateHostBuilder(args).Build().Run();

      // Use this:
      CreateHostBuilder(args).Build().RunOrExecuteCommands(args);

      // Or this, if you want different cli commands activation keyword instead of `cli`:
      CreateHostBuilder(args).Build().RunOrExecuteCommands(args, "commands");

      // Thanks to this code MissingCommand will take control of
      // running your web app and choose, if there is cli activation
      // keyword (default `cli`)
    }
    
    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureMissingCommands(config => {
          // [new in 0.1.1] you should enable this to check in Startup.cs if app is in CLI mode
          config.EnableServiceCliChecker(args);
        })
        // ...
  }
}

```

### Command Bags
To create commands you actually just need to create command bags, this
is just usual services which will hold commands for your specific
prefixes (remember above examples with "emails:command" or "analytics:command"?)

Remember, one service - command bag - holds all commands for specific prefix.

  **Each public method in your service can be command**

Here is an example:

```csharp
// It's easier to have commands in specific folder, so imagine storing
// them in Commands folder.
// BUT you can store them anywhere you want. Heck, you can store them
// in Services.Commands or just Services namespace
// Commands/EmailsCommandBag.cs
namespace MyApp.Commands {
  public class EmailsCommandBag {
    // This is just an example of service constructor so you
    // should be aware, as command bags are usual services,
    // you can use them as usual with service container,
    // with dependencies, etc
    public EmailsCommandBag(IDependency1 dependency) { ... }

    // This method can be called as command
    public void SendEmailAboutTomorrowSale() {
      // Here you are sending those emails for users
      // gathered from database
    }
    
    // [new in 0.1.1] now you can use arguments!
    // you would be able to use it like this
    // e.g.: dotnet run cli emails:add-new-npc Tom 25
    public void AddNewNpc(string name, int age) {
      // ...
    }
  }
}
```

After creating your first command bag we need to register it as usual service AND register it in the missing commands:

NOTE:
[new in 0.1.1] Here, if you made all needed preparations for this (see above when configuring Program.cs) you would be able to check whether app is in CLI mode.

```csharp
// Startup.cs

namespace MyApp {
  public class Startup {
    public void ConfigureServices(IServiceCollection services) {
      // Your usual services stuff
      // ...

      // Here you register your command bag as service.
      // You can use whatever service scope you need, of course
      services.AddScoped<EmailsCommandBag>();

      // Just for example we are adding another command bag
      services.AddScoped<DatabaseCleanupCommandBag>();

      if (services.IsMissingCommandsCli()) {
        // [new in 0.1.1]
        // You can do anything here what you want to do only in
        // cli mode
      }

      // This is where missing commands would know which
      // services are registered as command bags.
      // Notice .AddCommandBag method string argument:
      // this is prefix, which you would use so
      // missing commands know which command bag to use
      // E.g: database-cleanup:remove-empty-shopping-carts
      services.AddMissingCommands(
        config => config
          .AddCommandBag<EmailCommandBag>("email")
          .AddCommandBag<DatabaseCleanupCommandBag>("database-cleanup")
      );
    }

    // Other methods in Startup.cs
    // ...
  }
}



```

After that you can use your commands!

**Remember about naming**:

If you have method ReleaseVirtualMemory, it should be written in kebab-case:

    release-virtual-memory


So if you have command bag CleanupCommandBag with prefix "cleanup" and method .ReleaseVirtualMemory() you would call it like this:

```bash
dotnet run cli cleanup:release-virtual-memory

[new in 0.1.1] don't forget about arguments! you can use them now too.
dotnet run cli cleanup:release-virtual-memory-for-days 4
```

## Todo

- [x] Add arguments to commands (prefix:command arg1 arg2) (now accepts primitive types string/int/float/bool as true/false string)
- [ ] Add more complex command line arguments, like dates, params with lists etc.
- [ ] Add options to commands (prefix:command --option1 --option2 value)

## Release history

#### 0.2.0 Proper version bump (since new features were introduced in 0.1.1 it actually should be 0.2.0) + async commands
- Now command bag methods can be async. Just have return type Task :D

#### 0.1.1 Update with simple arguments and cli checker in Startup.cs file [07.02.2020]
- Ability can use simple arguments now: string, int, float, bool
- It's now possible to check if cli is in use in Startup.cs in ConfigureServices (useful when you don't want hosting/background services to start when using just cli)


#### 0.1.0 Initial release of library [11.12.2019]
- Ability to use command bags with method as commands
- As simple as it gets, without any input arguments/options
