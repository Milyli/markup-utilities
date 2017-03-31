using System;
using System.Data;
using MarkupUtilities.Helpers.Models;
using NUnit.Framework;

namespace MarkupUtilities.Helpers.NUnit
{
  [TestFixture]
  public class WorkerQueueRecordTests
  {
    [Test]
    public void Constructor_ReceivesTable_Initializes()
    {
      // Arrange
      var table = GetTable();

      // Act
      var record = new ExportWorkerQueueRecord(table.Rows[0]);

      // Assert
      Assert.AreEqual(2345678, record.WorkspaceArtifactId);
      Assert.AreEqual(1, record.RecordId);
      Assert.AreEqual(3456789, record.ExportJobArtifactId);
      Assert.AreEqual(3, record.DocumentArtifactId);
      Assert.AreEqual(4, record.MarkupSetArtifactId);
      Assert.AreEqual(5, record.QueueStatus);
      Assert.AreEqual(6, record.AgentId);
      Assert.AreEqual("7", record.MarkupSubType);
      Assert.AreEqual(8, record.ResourceGroupId);
    }

    [Test]
    public void Constructor_ReceivesNullTable_ThrowsArgumentNullException()
    {
      Assert.Throws<ArgumentNullException>(() => new ExportWorkerQueueRecord(null));
    }

    private static DataTable GetTable()
    {
      var table = new DataTable();
      table.Columns.Add("WorkspaceArtifactID", typeof(int));
      table.Columns.Add("ID", typeof(int));
      table.Columns.Add("ExportJobArtifactID", typeof(int));
      table.Columns.Add("Priority", typeof(int));
      table.Columns.Add("DocumentArtifactID", typeof(int));
      table.Columns.Add("MarkupSetArtifactID", typeof(int));
      table.Columns.Add("QueueStatus", typeof(int));
      table.Columns.Add("AgentID", typeof(int));
      table.Columns.Add("MarkupSubType", typeof(string));
      table.Columns.Add("ResourceGroupID", typeof(int));
      table.Rows.Add(2345678, 1, 3456789, 2, 3, 4, 5, 6, "7", 8);
      return table;
    }

    [Test]
    public void ReproduceWorkerQueueRecordInitTest()
    {
      var table = new DataTable();
      table.Columns.Add("WorkspaceArtifactID", typeof(int));
      table.Columns.Add("ID", typeof(int));
      table.Columns.Add("DocumentIDStart", typeof(int));
      table.Columns.Add("DocumentIDEnd", typeof(int));
      table.Columns.Add("SavedSearchHoldingTable", typeof(string));
      table.Columns.Add("RedactionsHoldingTable", typeof(string));
      table.Columns.Add("RedactionCodeTypeID", typeof(int));
      table.Columns.Add("QueueStatus", typeof(int));
      table.Columns.Add("MarkupSetRedactionCodeArtifactID", typeof(int));
      table.Columns.Add("MarkupSetAnnotationCodeArtifactID", typeof(int));
      table.Columns.Add("ReproduceJobArtifactID", typeof(int));
      table.Columns.Add("ResourceGroupID", typeof(int));
      table.Columns.Add("RelationalGroupColumn", typeof(string));
      table.Columns.Add("HasAutoRedactionsColumn", typeof(string));
      table.Columns.Add("RelationalGroup", typeof(string));
      table.Rows.Add(1, 2, 3, 4, "5", "6", 7, 8, 9, 10, 11, 12, "13", "14", "15");

      var reproduceManagerQueueRecord = new ReproduceWorkerQueueRecord(table.Rows[0]);
      Assert.AreEqual(1, reproduceManagerQueueRecord.WorkspaceArtifactId);
      Assert.AreEqual(2, reproduceManagerQueueRecord.RecordId);
      Assert.AreEqual(3, reproduceManagerQueueRecord.DocumentIdStart);
      Assert.AreEqual(4, reproduceManagerQueueRecord.DocumentIdEnd);
      Assert.AreEqual("5", reproduceManagerQueueRecord.SavedSearchHoldingTable);
      Assert.AreEqual("6", reproduceManagerQueueRecord.RedactionsHoldingTable);
      Assert.AreEqual(7, reproduceManagerQueueRecord.RedactionCodeTypeId);
      Assert.AreEqual(8, reproduceManagerQueueRecord.QueueStatus);
      Assert.AreEqual(9, reproduceManagerQueueRecord.MarkupSetRedactionCodeArtifactId);
      Assert.AreEqual(10, reproduceManagerQueueRecord.MarkupSetAnnotationCodeArtifactId);
      Assert.AreEqual(11, reproduceManagerQueueRecord.ReproduceJobArtifactId);
      Assert.AreEqual(12, reproduceManagerQueueRecord.ResourceGroupId);

      Assert.Throws<ArgumentNullException>(delegate { new ReproduceWorkerQueueRecord(null); });
    }
  }
}
