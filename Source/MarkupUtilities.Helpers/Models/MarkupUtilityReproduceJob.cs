namespace MarkupUtilities.Helpers.Models
{
  public class MarkupUtilityReproduceJob
  {
    public int ArtifactId { get; set; }
    public int CreatedBy { get; set; }
    public string Name { get; set; }
    public int SavedSearchArtifactId { get; set; }
    public int SourceMarkupSetArtifactId { get; set; }
    public int DestinationMarkupSetArtifactId { get; set; }
    public int JobType { get; set; }
    public int HasAutoRedactionsField { get; set; }
    public int RelationalField { get; set; }
    public string Status { get; set; }
    public string Details { get; set; }
    public string RelationalFieldColumnName { get; set; }
    public string HasAutoRedactionsFieldColumnName { get; set; }

    public MarkupUtilityReproduceJob(int artifactId, string name, int savedSearchArtifactId, int sourceMarkupSetArtifactId, int destinationMarkupSetArtifactId, string status, string details, int createdBy, int jobType, int hasAutoRedactionsField, int relationalField)
    {

      ArtifactId = artifactId;
      Name = name;
      SavedSearchArtifactId = savedSearchArtifactId;
      SourceMarkupSetArtifactId = sourceMarkupSetArtifactId;
      DestinationMarkupSetArtifactId = destinationMarkupSetArtifactId;
      Status = status;
      Details = details;
      CreatedBy = createdBy;
      JobType = jobType;
      HasAutoRedactionsField = hasAutoRedactionsField;
      RelationalField = relationalField;
    }
  }
}
