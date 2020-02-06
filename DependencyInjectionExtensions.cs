using System;
using Microsoft.Extensions.DependencyInjection;
using MissingCommands.Services;
using MissingCommands.Helpers;

namespace Microsoft.Extensions.DependencyInjection {
  public static class DependencyInjectionExtensions {
    public static IServiceCollection AddMissingCommands(
      this IServiceCollection services,
      Action<CommandResolverConfiguration> config
    ) {

      var resolverConfiguration = new CommandResolverConfiguration();

      // Using users configuration with added command bags
      config(resolverConfiguration);

      // We need to add configuration to services so
      // container would be able to inject it into command resolver
      services.AddSingleton<CommandResolverConfiguration>(
        p => resolverConfiguration
      );

      // Adding command resolver with type of commands storage
      services.AddSingleton<CommandResolver>();

      return services;
    }

    public static bool IsMissingCommandsCli(
      this IServiceCollection services
    ) {
      return RuntimeChecker.IsCli;
    }
  }
}