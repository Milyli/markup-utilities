using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using MarkupUtilities.Helpers;

namespace MarkupUtilities.CustomPages.Models
{
  public class ManagerQueueRecordModel
  {
    [DisplayName("Priority")]
    [Required(ErrorMessage = Constant.Messages.PRIORITY_REQUIRED)]
    public int? Priority { get; set; }

    [DisplayName("Workspace Artifact ID")]
    public int WorkspaceArtifactId { get; set; }

    [DisplayName("ID")]
    public int Id { get; set; }

    [DisplayName("Added On")]
    public DateTime AddedOn { get; set; }

    [DisplayName("Workspace Name")]
    public string WorkspaceName { get; set; }

    [DisplayName("Status")]
    public string Status { get; set; }

    [DisplayName("Agent ID")]
    public int? AgentId { get; set; }

    [DisplayName("Added By")]
    public string AddedBy { get; set; }

    [DisplayName("Record Artifact ID")]
    [Required(ErrorMessage = Constant.Messages.ARTIFACT_ID_REQUIRED)]
    public int? RecordArtifactId { get; set; }

    public IQuery QueryHelper { get; set; }

    public ManagerQueueRecordModel()
    {
      QueryHelper = new Query();
    }

    public ManagerQueueRecordModel(DataRow row, IQuery queryHelper)
    {
      SetPropertiesFromDataRow(row, queryHelper);
    }

    private void SetPropertiesFromDataRow(DataRow row, IQuery queryHelper)
    {
      Id = row["ID"] != DBNull.Value ? Convert.ToInt32(row["ID"]) : 0;
      AddedOn = row["Added On"] != DBNull.Value ? Convert.ToDateTime(row["Added On"]) : new DateTime();
      WorkspaceArtifactId = row["Workspace Artifact ID"] != DBNull.Value ? Convert.ToInt32(row["Workspace Artifact ID"]) : 0;
      WorkspaceName = row["Workspace Name"] != DBNull.Value ? Convert.ToString(row["Workspace Name"]) : string.Empty;
      Status = row["Status"] != DBNull.Value ? Convert.ToString(row["Status"]) : string.Empty;
      AgentId = (row["Agent Artifact ID"] != DBNull.Value) ? Convert.ToInt32(row["Agent Artifact ID"]) : new int?();
      Priority = row["Priority"] != DBNull.Value ? Convert.ToInt32(row["Priority"]) : 0;
      AddedBy = row["Added By"] != DBNull.Value ? Convert.ToString(row["Added By"]) : string.Empty;
      RecordArtifactId = row["Record Artifact ID"] != DBNull.Value ? Convert.ToInt32(row["Record Artifact ID"]) : 0;

      QueryHelper = queryHelper;
    }
  }
}
