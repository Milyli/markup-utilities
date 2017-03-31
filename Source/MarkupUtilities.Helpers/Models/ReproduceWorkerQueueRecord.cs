using System;
using System.Data;

namespace MarkupUtilities.Helpers.Models
{
  /// <summary>
  /// Represents a single row in the Worker Queue Table 
  /// </summary>
  public class ReproduceWorkerQueueRecord
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
    /// The start ID value to be processed 
    /// </summary>
    public int DocumentIdStart;


    /// <summary>
    /// The end ID value to be processed 
    /// </summary>
    public int DocumentIdEnd;

    /// <summary>
    /// The saved search hodling table 
    /// </summary>
    public string SavedSearchHoldingTable;

    /// <summary>
    /// The redaction set hodling table 
    /// </summary>
    public string RedactionsHoldingTable;

    /// <summary>
    /// The Queue Status for the Export Job
    /// </summary>
    public int QueueStatus;

    /// <summary>
    /// The reproduce job artifact id
    /// </summary>
    public int ReproduceJobArtifactId;

    /// <summary>
    /// The identifier of the resource group that the workspace belongs to
    /// </summary>
    public int ResourceGroupId;
    public int RedactionCodeTypeId;
    public int MarkupSetRedactionCodeArtifactId;
    public int MarkupSetAnnotationCodeArtifactId;
    public string RelationalGroupColumn;
    public string HasAutoRedactionsColumn;
    public string RelationalGroup;

    #endregion Properties

    public ReproduceWorkerQueueRecord(DataRow row)
    {
      if (row == null) { throw new ArgumentNullException(nameof(row)); }

      RecordId = (int)row["ID"];
      WorkspaceArtifactId = (int)row["WorkspaceArtifactID"];
      DocumentIdStart = ConvertFromDbVal<int>(row["DocumentIDStart"]);
      DocumentIdEnd = ConvertFromDbVal<int>(row["DocumentIDEnd"]);
      SavedSearchHoldingTable = (string)row["SavedSearchHoldingTable"];
      RedactionsHoldingTable = (string)row["RedactionsHoldingTable"];
      QueueStatus = (int)row["QueueStatus"];
      ReproduceJobArtifactId = (int)row["ReproduceJobArtifactID"];
      ResourceGroupId = (int)row["ResourceGroupID"];
      RedactionCodeTypeId = (int)row["RedactionCodeTypeID"];
      MarkupSetRedactionCodeArtifactId = (int)row["MarkupSetRedactionCodeArtifactID"];
      MarkupSetAnnotationCodeArtifactId = (int)row["MarkupSetAnnotationCodeArtifactID"];
      RelationalGroupColumn = ConvertFromDbVal<string>(row["RelationalGroupColumn"]);
      HasAutoRedactionsColumn = ConvertFromDbVal<string>(row["HasAutoRedactionsColumn"]);
      RelationalGroup = ConvertFromDbVal<string>(row["RelationalGroup"]);
    }


    public T ConvertFromDbVal<T>(object obj)
    {
      if (obj == null || obj == DBNull.Value)
      {
        return default(T); // returns the default value for the type
      }

      return (T)obj;
    }
  }
}
