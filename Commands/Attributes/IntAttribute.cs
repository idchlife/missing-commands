using System;
namespace MissingCommands.Commands.Attributes {
  public class IntAttribute : Attribute<int> {
    public override int GetValue(string input) {
      try {
        return int.Parse(input);
      } catch (FormatException) {
        throw new ArgumentParsingException(input, typeof(int));
      }
    }
  }
}