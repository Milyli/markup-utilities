using System;
using System.Data;
using MarkupUtilities.Helpers.Models;
using NUnit.Framework;

namespace MarkupUtilities.Helpers.NUnit
{
  [TestFixture]
  public class ManagerQueueRecordTests
  {
    [Test]
    public void Constructor_ReceivesTable_Initializes()
    {
      // Arrange
      var table = GetTable();

      // Act
      var record = new ExportManagerQueueRecord(table.Rows[0]);

      // Assert
      Assert.AreEqual(2345678, record.WorkspaceArtifactId);
      Assert.AreEqual(1, record.RecordId);
      Assert.AreEqual(3456789, record.ExportJobArtifactId);
    }

    [Test]
    public void Constructor_ReceivesNullTable_ThrowsArgumentNullException()
    {
      Assert.Throws<ArgumentNullException>(() => new ExportManagerQueueRecord(null));
    }

    private static DataTable GetTable()
    {
      var table = new DataTable();
      table.Columns.Add("WorkspaceArtifactID", typeof(int));
      table.Columns.Add("ID", typeof(int));
      table.Columns.Add("ExportJobArtifactID", typeof(int));
      table.Columns.Add("Priority", typeof(int));
      table.Columns.Add("ResourceGroupID", typeof(int));
      table.Rows.Add(2345678, 1, 3456789, 2, 100001);

      return table;
    }

    [Test]
    public void ReproduceManagerQueueRecordInitTest()
    {
      var table = new DataTable();
      table.Columns.Add("WorkspaceArtifactID", typeof(int));
      table.Columns.Add("ID", typeof(int));
      table.Columns.Add("ReproduceJobArtifactID", typeof(int));
      table.Columns.Add("ResourceGroupID", typeof(int));
      table.Rows.Add(1, 2, 3, 4);

      var reproduceManagerQueueRecord = new ReproduceManagerQueueRecord(table.Rows[0]);
      Assert.AreEqual(1, reproduceManagerQueueRecord.WorkspaceArtifactId);
      Assert.AreEqual(2, reproduceManagerQueueRecord.RecordId);
      Assert.AreEqual(3, reproduceManagerQueueRecord.ReproduceJobArtifactId);
      Assert.AreEqual(4, reproduceManagerQueueRecord.ResourceGroupId);

      Assert.Throws<ArgumentNullException>(delegate { new ReproduceManagerQueueRecord(null); });
    }

    [Test]
    public void TestMarkupUtilityReproduceJob()
    {
      var markupUtilityReproduceJob = new MarkupUtilityReproduceJob(1, "2", 3, 4, 5, "6", "7", 8, 1, 1, 1);
      Assert.AreEqual(1, markupUtilityReproduceJob.ArtifactId);
      Assert.AreEqual("2", markupUtilityReproduceJob.Name);
      Assert.AreEqual(3, markupUtilityReproduceJob.SavedSearchArtifactId);
      Assert.AreEqual(4, markupUtilityReproduceJob.SourceMarkupSetArtifactId);
      Assert.AreEqual(5, markupUtilityReproduceJob.DestinationMarkupSetArtifactId);
      Assert.AreEqual("6", markupUtilityReproduceJob.Status);
      Assert.AreEqual("7", markupUtilityReproduceJob.Details);
      Assert.AreEqual(8, markupUtilityReproduceJob.CreatedBy);
    }
  }
}
