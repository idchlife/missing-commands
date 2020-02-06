using System;

namespace Microsoft.Extensions.Hosting {
  public static class HostBuilderExtensions {
    public static IHostBuilder ConfigureMissingCommands(this IHostBuilder builder, Action<MissingCommands.Helpers.Configuration> func) {
      func(MissingCommands.Helpers.Configuration.Instance);

      return builder;
    }
  }
}