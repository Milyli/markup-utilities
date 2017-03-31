using System;
using System.ComponentModel;
using System.Data;
using MarkupUtilities.Helpers;

namespace MarkupUtilities.CustomPages.Models
{
  public class WorkerQueueRecordModel
  {
    [DisplayName("Priority")]
    public int Priority { get; set; }

    [DisplayName("Workspace Artifact ID")]
    public int WorkspaceArtifactId { get; set; }

    [DisplayName("Job ID")]
    public int JobId { get; set; }

    [DisplayName("Added On")]
    public DateTime AddedOn { get; set; }

    [DisplayName("Workspace Name")]
    public string WorkspaceName { get; set; }

    [DisplayName("Status")]
    public string Status { get; set; }

    [DisplayName("Agent ID")]
    public int? AgentId { get; set; }

    [DisplayName("# Records Remaining")]
    public int RemainingRecordCount { get; set; }

    [DisplayName("Parent Record Artifact ID")]
    public int ParentRecordArtifactId { get; set; }

    public IQuery QueryHelper { get; set; }

    public WorkerQueueRecordModel()
    {
      QueryHelper = new Query();
    }

    public WorkerQueueRecordModel(DataRow row, IQuery queryHelper)
    {
      SetPropertiesFromDataRowAsync(row, queryHelper);
    }
    private void SetPropertiesFromDataRowAsync(DataRow row, IQuery queryHelper)
    {
      JobId = row["ID"] != DBNull.Value ? Convert.ToInt32(row["ID"]) : 0;
      AddedOn = row["Added On"] != DBNull.Value ? Convert.ToDateTime(row["Added On"]) : new DateTime();
      WorkspaceArtifactId = row["Workspace Artifact ID"] != DBNull.Value ? Convert.ToInt32(row["Workspace Artifact ID"]) : 0;
      WorkspaceName = row["Workspace Name"] != DBNull.Value ? Convert.ToString(row["Workspace Name"]) : string.Empty;
      Status = row["Status"] != DBNull.Value ? Convert.ToString(row["Status"]) : string.Empty;
      AgentId = (row["Agent Artifact ID"] != DBNull.Value) ? Convert.ToInt32(row["Agent Artifact ID"]) : new int?();
      Priority = row["Priority"] != DBNull.Value ? Convert.ToInt32(row["Priority"]) : 0;
      RemainingRecordCount = row["# Records Remaining"] != DBNull.Value ? Convert.ToInt32(row["# Records Remaining"]) : 0;
      ParentRecordArtifactId = row["Parent Record Artifact ID"] != DBNull.Value ? Convert.ToInt32(row["Parent Record Artifact ID"]) : 0;
      QueryHelper = queryHelper;
    }

  }
}
