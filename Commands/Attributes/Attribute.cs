using System;
namespace MissingCommands.Commands.Attributes {
  public abstract class Attribute<T> {
    public class ArgumentParsingException : Exception {
      public ArgumentParsingException(string value, Type type)
        : base(String.Format("Could not parse argument value {0} while converting to type {1}. Maybe wrong argument attribute used?")) {}
    }

    public abstract T GetValue(string input);
  }
}