using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace MissingCommands.Services {
  public class InvalidCommandFormat : Exception {
    public InvalidCommandFormat(string entryPoint) : base(
      $"It seems argument {entryPoint} does not comply to format prefix:command. Cannot resolve."
    ) {}
  }

  public class CommandBagNotFound : Exception {
    public CommandBagNotFound(string prefix) : base(
      $"Command bag by prefix {prefix} was not found. Either prefix wrong or command bag was not registered in MissingCommands"
    ) {}
  }

  public class CommandBagMethodNotFound : Exception {
    public CommandBagMethodNotFound(string command, string methodName) : base(
      $"Command bag method {methodName} for command {command} was not found"
    ) {}
  }

  public class CommandBagServiceNotRegistered : Exception {
    public CommandBagServiceNotRegistered(string type) : base(
      $"Command bag class {type} was not registered in you service container. Remember, you should add your command bag as service in your container"
    ) {}
  }

  public class ErrorParsingCliArguments : Exception {
    public ErrorParsingCliArguments(Exception e) : base(
      $"While trying to parse cli arguments and getting reflection of command bag method encountered an exception. Here it is: {e.ToString()}"
    ) {}
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

    public async Task ResolveCommand(string[] args) {
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

      // We are omitting first argument because it's bag and method name
      var cliArguments = args.Skip(1).ToArray();

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

        var actualMethodArguments = new List<object>();

        var parameters = method.GetParameters();

        try {
          for (int i = 0; i < parameters.Length; i++) {
            var p = parameters[i];

            var cliArgument = cliArguments.ElementAtOrDefault(i);

            var methodParameterType = p.GetType();
            var methodParameterDefaultValue = p.HasDefaultValue ? p.DefaultValue : null;

            var actualValue = cliArgument != null ? cliArgument : methodParameterDefaultValue;

            if (actualValue == null) {
              throw new Exception(
                $"Method {method.Name} argument has no default value and also there is no provided argument via cli. Cli arguments separated for method: {String.Join(", ", cliArguments)}. All arguments provided in cli: {String.Join(", ", args)}"
              );
            } else {
              try {
                actualMethodArguments.Add(
                  Convert.ChangeType(actualValue, p.ParameterType)
                );
              } catch (Exception e) {
                throw new Exception(
                  $"Cannot cast to needed type for method argument! Original cli argument: {cliArgument}, and needed type: {p.GetType().ToString()}"
                );
              }
            }
          }
        } catch (Exception e) {
          throw new ErrorParsingCliArguments(e);
        }

        if (method.ReturnType == typeof(Task)) {
          await (dynamic) method.Invoke(bagInstance, actualMethodArguments.ToArray());
        } else {
          method.Invoke(bagInstance, actualMethodArguments.ToArray());
        }
      }

      // Console.WriteLine("Everything is good!");
    }

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