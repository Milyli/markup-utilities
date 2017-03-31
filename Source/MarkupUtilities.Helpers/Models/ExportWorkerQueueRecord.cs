using System;
using System.Data;

namespace MarkupUtilities.Helpers.Models
{
  /// <summary>
  /// Represents a single row in the Worker Queue Table 
  /// </summary>
  public class ExportWorkerQueueRecord
  {
    #region Properties

    /// <summary>
    /// The primary key identifier of the table 
    /// </summary>
    public int RecordId;

    /// <summary>
    /// The workspace to be processed 
    /// </summary>
    public int WorkspaceArtifactId;

    /// <summary>
    /// The Document Artifact ID from the Saved Search 
    /// </summary>
    public int DocumentArtifactId;

    /// <summary>
    /// The Markup Set Artifact ID for the redactions
    /// </summary>
    public int MarkupSetArtifactId;

    /// <summary>
    /// The Queue Status for the Export Job
    /// </summary>
    public int QueueStatus;

    /// <summary>
    /// The Agent ID that is assigned the Export Job
    /// </summary>
    public int AgentId;

    /// <summary>
    /// An identifier for any use within the workspace 
    /// </summary>
    public int ExportJobArtifactId;

    /// <summary>
    /// The identifier of the resource group that the workspace belongs to
    /// </summary>
    public int ResourceGroupId;

    /// <summary>
    /// The list of MarkupSubTypes
    /// </summary>
    public string MarkupSubType;

    #endregion Properties

    public ExportWorkerQueueRecord(DataRow row)
    {
      if (row == null) { throw new ArgumentNullException(nameof(row)); }

      RecordId = (int)row["ID"];
      WorkspaceArtifactId = (int)row["WorkspaceArtifactID"];
      DocumentArtifactId = (int)row["DocumentArtifactID"];
      MarkupSetArtifactId = (int)row["MarkupSetArtifactID"];
      QueueStatus = (int)row["QueueStatus"];
      AgentId = (int)row["AgentID"];
      ExportJobArtifactId = (int)row["ExportJobArtifactID"];
      MarkupSubType = (string)row["MarkupSubType"];
      ResourceGroupId = (int)row["ResourceGroupID"];
    }
  }
}
