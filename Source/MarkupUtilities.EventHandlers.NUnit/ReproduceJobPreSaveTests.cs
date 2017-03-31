using MarkupUtilities.Helpers;
using NUnit.Framework;

namespace MarkupUtilities.EventHandlers.NUnit
{
  [TestFixture]
  public class ReproduceJobPreSaveTests
  {
    private ReproduceJobPreSave _job;
    [SetUp]
    public void SetUp()
    {
      _job = new ReproduceJobPreSave();
    }

    [Test]
    public void TestRequiredFields()
    {
      Assert.That(_job.RequiredFields.Count, Is.EqualTo(1));
      Assert.IsNotNull((_job.RequiredFields[Constant.Guids.Field.MarkupUtilityReproduceJob.Status.ToString()]));
    }
  }
}
