using System;
using System.Data;

namespace MarkupUtilities.Helpers.Models
{
  /// <summary>
  /// Represents a single row in the Manager Queue Table 
  /// </summary>
  public class ExportManagerQueueRecord
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
    /// An identifier for any use within the workspace 
    /// </summary>
    public int ExportJobArtifactId;

    /// <summary>
    /// The identifier of the resource group that the workspace belongs to
    /// </summary>
    public int ResourceGroupId;

    #endregion Properties

    public ExportManagerQueueRecord(DataRow row)
    {
      if (row == null) { throw new ArgumentNullException(nameof(row)); }

      WorkspaceArtifactId = (int)row["WorkspaceArtifactID"];
      RecordId = (int)row["ID"];
      ExportJobArtifactId = (int)row["ExportJobArtifactID"];
      ResourceGroupId = (int)row["ResourceGroupID"];
    }
  }
}
