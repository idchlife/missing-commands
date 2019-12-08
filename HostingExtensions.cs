using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using MissingCommands.Services;

namespace Microsoft.Extensions.Hosting {
  public static class HostingExtensions {
    // <summary>
    public static void RunOrExecuteCommands(this IHost host, string[] args, string cliArg = "cli") {
      if (args.Length > 0) {
        if (args[0] == cliArg) {
          // Calling service if available
          var resolver = host.Services.GetService(typeof(CommandResolver)) as CommandResolver;

          var listArgs = new List<string>(args);

          listArgs.RemoveAt(0);

          var fixedArrayArgs = listArgs.ToArray();

          if (resolver != null) resolver.ResolveCommand(fixedArrayArgs);
          return;
        }
      }

      host.Run();
    }
  }
}
