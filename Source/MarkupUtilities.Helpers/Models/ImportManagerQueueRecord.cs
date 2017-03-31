using System;
using System.Data;
using MarkupUtilities.Helpers.Exceptions;

namespace MarkupUtilities.Helpers.Models
{
  /// <summary>
  /// Represents a single row in the Import Manager Queue Table 
  /// </summary>
  public class ImportManagerQueueRecord
  {
    #region Properties

    public int Id { get; private set; }
    public DateTime TimeStampUtc { get; private set; }
    public int WorkspaceArtifactId { get; private set; }
    public int QueueStatus { get; private set; }
    public int? AgentId { get; private set; }
    public int ImportJobArtifactId { get; private set; }
    public string JobType { get; private set; }
    public int ResourceGroupId { get; private set; }

    #endregion Properties

    public ImportManagerQueueRecord(DataRow dataRow)
    {
      if (dataRow == null)
      {
        throw new ArgumentNullException(nameof(dataRow));
      }

      if (dataRow[Constant.Sql.AdminTables.ImportManagerQueue.Columns.ID] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportManagerQueue.Columns.ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportManagerQueue.Columns.TIME_STAMP_UTC] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportManagerQueue.Columns.TIME_STAMP_UTC} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportManagerQueue.Columns.WORKSPACE_ARTIFACT_ID] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportManagerQueue.Columns.WORKSPACE_ARTIFACT_ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportManagerQueue.Columns.QUEUE_STATUS] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportManagerQueue.Columns.QUEUE_STATUS} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportManagerQueue.Columns.IMPORT_JOB_ARTIFACT_ID] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportManagerQueue.Columns.IMPORT_JOB_ARTIFACT_ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportManagerQueue.Columns.JOB_TYPE] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportManagerQueue.Columns.JOB_TYPE} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportManagerQueue.Columns.RESOURCE_GROUP_ID] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportManagerQueue.Columns.RESOURCE_GROUP_ID} is NULL");
      }

      Id = dataRow.Field<int>(Constant.Sql.AdminTables.ImportManagerQueue.Columns.ID);
      TimeStampUtc = dataRow.Field<DateTime>(Constant.Sql.AdminTables.ImportManagerQueue.Columns.TIME_STAMP_UTC);
      WorkspaceArtifactId = dataRow.Field<int>(Constant.Sql.AdminTables.ImportManagerQueue.Columns.WORKSPACE_ARTIFACT_ID);
      QueueStatus = dataRow.Field<int>(Constant.Sql.AdminTables.ImportManagerQueue.Columns.QUEUE_STATUS);
      AgentId = dataRow.Field<int?>(Constant.Sql.AdminTables.ImportManagerQueue.Columns.AGENT_ID);
      ImportJobArtifactId = dataRow.Field<int>(Constant.Sql.AdminTables.ImportManagerQueue.Columns.IMPORT_JOB_ARTIFACT_ID);
      JobType = dataRow.Field<string>(Constant.Sql.AdminTables.ImportManagerQueue.Columns.JOB_TYPE);
      ResourceGroupId = dataRow.Field<int>(Constant.Sql.AdminTables.ImportManagerQueue.Columns.RESOURCE_GROUP_ID);
    }
  }
}
