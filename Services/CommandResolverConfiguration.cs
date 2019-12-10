using System;
using System.Collections.Generic;

namespace MissingCommands.Services {
  public class CommandBagAlreadyRegistered : Exception {
    public CommandBagAlreadyRegistered(string prefix) : base(String.Format(
      "Command bag with prefix {0} already registered in MissingCommands",
      prefix
    )) {}
  }

  public class CommandResolverConfiguration {
    private Dictionary<string, Type> commandBags = new Dictionary<string, Type>();

    public CommandResolverConfiguration() {
    }

    public CommandResolverConfiguration AddCommandBag<TypeOfCommand>(string prefix) where TypeOfCommand : class {
      if (commandBags.ContainsKey(prefix)) {
        throw new CommandBagAlreadyRegistered(prefix);
      }

      commandBags.Add(prefix, typeof(TypeOfCommand));

      return this;
    }

    public Type GetCommandBagType(string prefix) {
      return commandBags[prefix];
    }
  }
}