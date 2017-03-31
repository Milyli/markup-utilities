using System;
using System.Data;

namespace MarkupUtilities.Helpers.Models
{
  /// <summary>
  /// Represents a single row in the Manager Queue Table 
  /// </summary>
  public class ReproduceManagerQueueRecord
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
    public int ReproduceJobArtifactId;

    /// <summary>
    /// The identifier of the resource group that the workspace belongs to
    /// </summary>
    public int ResourceGroupId;

    #endregion Properties

    public ReproduceManagerQueueRecord(DataRow row)
    {
      if (row == null) { throw new ArgumentNullException(nameof(row)); }

      WorkspaceArtifactId = (int)row["WorkspaceArtifactID"];
      RecordId = (int)row["ID"];
      ReproduceJobArtifactId = (int)row["ReproduceJobArtifactID"];
      ResourceGroupId = (int)row["ResourceGroupID"];
    }
  }
}
