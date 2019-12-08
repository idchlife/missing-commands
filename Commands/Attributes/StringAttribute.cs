namespace MissingCommands.Commands.Attributes {
  public class StringAttribute : Attribute<string> {
    public override string GetValue(string input) {
      return input;
    }
  }
}