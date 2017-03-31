using NUnit.Framework;

namespace MarkupUtilities.EventHandlers.NUnit
{
  [TestFixture]
  public class FilePreCascadeDeleteTests
  {
    private FilePreCascadeDelete _filePreCascadeDelete;

    [SetUp]
    public void SetUp()
    {
      _filePreCascadeDelete = new FilePreCascadeDelete();
    }

    [Test]
    public void TestExecuteFail()
    {
      var response = _filePreCascadeDelete.Execute();

      // Assert 
      Assert.IsFalse(response.Success);
      Assert.NotNull(response.Exception);
    }
  }
}