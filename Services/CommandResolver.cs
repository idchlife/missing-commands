using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace MissingCommands.Services {
  public class InvalidCommandFormat : Exception {
    public InvalidCommandFormat(string entryPoint) : base(String.Format(
      "It seems argument {0} does not comply to format prefix:command. Cannot resolve.",
      entryPoint
    )) {}
  }

  public class CommandBagNotFound : Exception {
    public CommandBagNotFound(string prefix) : base(String.Format(
      "Command bag by prefix {0} was not found. Either prefix wrong or command bag was not registered in MissingCommands",
      prefix
    )) {}
  }

  public class CommandBagMethodNotFound : Exception {
    public CommandBagMethodNotFound(string command, string methodName) : base(String.Format(
      "Command bag method {1} for command {0} was not found",
      command,
      methodName
    )) {}
  }

  public class CommandBagServiceNotRegistered : Exception {
    public CommandBagServiceNotRegistered(string type) : base(String.Format(
      "Command bag class {0} was not registered in you service container. Remember, you should add your command bag as service in your container",
      type
    )) {}
  }

  public class CommandResolver {

    private IServiceScopeFactory scopeFactory;

    private CommandResolverConfiguration config;

    public CommandResolver(
      IServiceScopeFactory scopeFactory,
      CommandResolverConfiguration config
    ) {
      this.scopeFactory = scopeFactory;
      this.config = config;
    }

    public void ResolveCommand(string[] args) {
      var entryPoint = args[0];

      if (!entryPoint.Contains(":")) {
        throw new InvalidCommandFormat(entryPoint);
      }

      var blob = entryPoint.Split(':');

      if (blob.Length != 2) {
        throw new InvalidCommandFormat(entryPoint);
      }

      var prefix = blob[0];
      var command = blob[1];

      var bagType = config.GetCommandBagType(prefix);

      if (bagType == null) {
        throw new CommandBagNotFound(prefix);
      }

      var methodName = GetMethodNameFromArg(command);

      var method = bagType.GetMethod(methodName);


      if (method == null) {
        throw new CommandBagMethodNotFound(command, methodName);
      }

      using (var scope = scopeFactory.CreateScope()) {
        var bagInstance = scopeFactory.CreateScope().ServiceProvider.GetService(bagType);

        if (bagInstance == null) {
          throw new CommandBagServiceNotRegistered(bagType.ToString());
        }

        method.Invoke(bagInstance, null);
      }

      Console.WriteLine("Everything is good!");
    }

    // private object GetCommandBagService(Type type) {
      
    // }

    private string GetMethodNameFromArg(string arg) {
      return DashCaseToPascalCase(arg);
    }

    private string DashCaseToPascalCase(string input) {
      return String.Join(
        "",
        input
          .Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
          .ToList()
          .Select(s => s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower())
      );
    }
  }
}