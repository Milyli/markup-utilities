using System.Collections.Generic;
using MarkupUtilities.Helpers.Rsapi;
using NUnit.Framework;

namespace MarkupUtilities.Helpers.NUnit
{
  [TestFixture]
  public class MessageFormatterTests
  {
    [Description("When the results were successful, should return an empty string")]
    [Test]
    public void FormatMessage_ReceivesSuccessfulResults_ReturnsEmpty()
    {
      // Arrange
      const bool success = true;

      // Act
      var actual = MessageFormatter.FormatMessage(new List<string>(), string.Empty, success);

      // Assert
      Assert.AreEqual(string.Empty, actual);
    }

    [Description("When the results were unsuccessful, should return the message with each result appended")]
    [Test]
    public void FormatMessage_RecievesFailedResults_ReturnsFormattedString()
    {
      // Arrange
      var results = new List<string>() { "First result", "Second result", "Third result" };
      const string message = "This is my test message";
      const bool success = false;

      // Act
      var actual = MessageFormatter.FormatMessage(results, message, success);

      // Assert
      Assert.AreEqual("This is my test messageFirst resultSecond resultThird result", actual);
    }
  }
}
