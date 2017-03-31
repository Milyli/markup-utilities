using System;

namespace MarkupUtilities.Helpers.Models
{
  public class RedactionAuditRecord
  {
    public int ArtifactId { get; set; }
    public int Action { get; set; }
    public string Details { get; set; }
    public int UserId { get; set; }
    public DateTime TimeStamp { get; set; }
    public string RequestOrigination { get; set; }
    public string RecordOrigination { get; set; }
    public int? ExecutionTime { get; set; }
    public int? SessionIdentifier { get; set; }

    public RedactionAuditRecord(int artifactId, int action, string details, int userId, DateTime timeStamp, string requestOrigination, string recordOrigination, int? executionTime, int? sessionIdentifier)
    {
      ArtifactId = artifactId;
      Action = action;
      Details = details;
      UserId = userId;
      TimeStamp = timeStamp;
      RequestOrigination = requestOrigination;
      RecordOrigination = recordOrigination;
      ExecutionTime = executionTime;
      SessionIdentifier = sessionIdentifier;
    }
  }
}
