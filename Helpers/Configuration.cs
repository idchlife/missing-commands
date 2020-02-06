namespace MissingCommands.Helpers {
  public class Configuration {
    public static Configuration Instance {
      get {
        if (instance == null) {
          instance = new Configuration();
        }

        return instance;
      }
    }

    private static Configuration instance;

    public string CliArg { get; private set; } = "cli";

    private bool enabledServiceCliChecker = false;
    private string[] args;

    public void EnableServiceCliChecker(string[] args) {
      RuntimeChecker.IsCli = args.Length > 0 && args[0] == CliArg;
      enabledServiceCliChecker = true;
    }

    public void ChangeCliKeyword(string keyword) {
      CliArg = keyword;

      if (enabledServiceCliChecker) {
        // If cli keyword was changed before we need to re-instantiate
        // because cli keyword has changed
        EnableServiceCliChecker(args);
      }
    }
  }
}